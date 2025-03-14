import sys
from pymongo import MongoClient
from models import ClientProviderInfo, HotelMapping, CoreHotel, GeoPoint
from elasticsearch import Elasticsearch
import requests
from geojson import Feature, Polygon, Point, FeatureCollection, MultiPolygon, LineString, MultiLineString, MultiPoint
import geojson
import hashlib
import time
from datetime import datetime, timezone

#MONGO_URL = 'mongodb://app:vervozaq1ZAQ!@mongodb-core-0.mapping.io:27017/admin?authMechanism=SCRAM-SHA-1&directConnection=true'
MONGO_URL = 'mongodb://app:vervozaq1ZAQ!@mongo-core-0.mapping.vervotech.com:27017/admin?authMechanism=SCRAM-SHA-1&directConnection=true'

UNICA_CONFIG_DB = 'unicaConfiguration'
ACCOUNT_CONFIG_COLLECTION = 'accountConfig'
PROVIDERS_COLLECTION = 'providers'

CLIENT_MAPPING_DB = 'ClientMappings'
CLIENT_MAPPING_COLLECTION_PREFIX = 'hotelmappings__{accountId}'

CORE_HOTEL_DB = 'vervotech-CoreHotels-en-US'
CORE_HOTEL_COLLECTION = 'HotelsUnica'

ES_CERT_FINGERPRINT = 'bb8f570cb2c7d839d90107a4621f49da9d755432c7abcacc7a6839741fc40333'
ES_API_KEY = 'UGpqY05vNEJwRVo4cVAwUVdVYnY6OUlPNWk2ZFBUdS12V3lLSVlJU0xlQQ=='

# es_client = Elasticsearch(
#         ["https://localhost:9200"],
#         ssl_assert_fingerprint=ES_CERT_FINGERPRINT,
#         api_key=ES_API_KEY
#     )

es_client = Elasticsearch(
        ["http://localhost:9200"]
    )

mongo_client = MongoClient(MONGO_URL)
mongo_client_geo = MongoClient('mongodb://localhost:27017')


def start(accountId, app_config_filename):

    provider_family_migrating = ['Priceline', 'Agoda' , 'EANV3', 'EAN' ]
    unica_config_db = mongo_client.get_database(UNICA_CONFIG_DB)
    account_configs_collection = unica_config_db.get_collection(ACCOUNT_CONFIG_COLLECTION)
    providers_db_collection = unica_config_db.get_collection(PROVIDERS_COLLECTION)


    mongo_geo_db = mongo_client_geo.get_database("varcaDb")
    geo_db_mappings = mongo_geo_db.get_collection("varcaGeoHotelMappingsV1")

    providers = providers_db_collection.find({'$and': [
                                                {'AccountId': accountId},
                                                {'IsDisabled': False}
                                            ]
                                        })    
    clients_provider_info = []
    
    for provider in providers:
        clients_provider_info.append(ClientProviderInfo(**provider))


    client_mapping_db = mongo_client.get_database(CLIENT_MAPPING_DB)
    client_mapping_collection = client_mapping_db.get_collection(CLIENT_MAPPING_COLLECTION_PREFIX.format(accountId = accountId))

    core_hotel_db = mongo_client.get_database(CORE_HOTEL_DB)
    core_hotel_collection = core_hotel_db.get_collection(CORE_HOTEL_COLLECTION)

    for client_provider_info in clients_provider_info:
        if client_provider_info.providerFamily not in provider_family_migrating:
            continue

        hotelmappings_cursor = client_mapping_collection.find({'$and': [
            {'AccountId': client_provider_info.accountId },
            {'IsDisabled': False},
            {'ProviderKey': client_provider_info.id},
        ]})
        
        
        for hotelmapping_record in hotelmappings_cursor:
            hotelmapping = HotelMapping(**hotelmapping_record)

            core_hotel_record = core_hotel_collection.find_one({
                '$and': [
                    {'ProviderFamily': 'Vervotech' },
                    {'Disabled': False},
                    {'ProviderHotelId': f'V{hotelmapping.unicaId}'},
                ]
            })

            if core_hotel_record is None:
                core_hotel_record = core_hotel_collection.find_one({'$and': [
                        {'ProviderFamily': hotelmapping.providerFamily },
                        {'Disabled': False},
                        {'ProviderHotelId': hotelmapping.providerHotelId},
                    ]})
                
            if core_hotel_record is None:
                continue

            core_hotel = CoreHotel(**core_hotel_record)
            if core_hotel.geocode is None: continue
            hotelmapping.geoLocation = GeoPoint(Coordinates=[core_hotel.geocode.lon, core_hotel.geocode.lat])
            hotelmapping.countryCode = core_hotel.contact.address.countryCode if (core_hotel.contact.address.countryCode and core_hotel.contact.address.countryCode.strip()) else None
            hotelmapping.publishKey = get_client_provider_publish_key(client_provider_info)
            hotelmapping.rating = core_hotel.rating if (core_hotel.rating and core_hotel.rating.strip()) else None
            hotelmapping.brandCode = core_hotel.brandCode if (core_hotel.brandCode and core_hotel.brandCode.strip()) else None
            hotelmapping.chainCode = core_hotel.chainCode if (core_hotel.chainCode and core_hotel.chainCode.strip()) else None
            hotelmapping.propertyType = core_hotel.propertyType if (core_hotel.propertyType and core_hotel.propertyType.strip()) else None
            
            if (hotelmapping.channelIds is not None and len(hotelmapping.channelIds) == 0):
                hotelmapping.channelIds = None

            hotelmapping.lastUpdatedAt = datetime.now(timezone.utc)
            hotelmapping.accessKeys = get_access_keys(hotelmapping.publishKey, hotelmapping.channelIds)

            hotelmapping_dict = hotelmapping.model_dump(by_alias=False, exclude_none=True);
            del hotelmapping_dict['id']
            hotelmapping_dict['_id'] = hotelmapping.id

            geo_db_mappings.replace_one({'_id': hotelmapping.id }, hotelmapping_dict, upsert=True)
            print(hotelmapping.id)

            # hotelmapping_json = hotelmapping.model_dump_json(by_alias=True, exclude_none=True)
            # es_client.index(index='varca-geo-hotelmappings-v3', id=hotelmapping.id, document=hotelmapping_json)


            # city = core_hotel.contact.address.city if (core_hotel.contact.address.city and core_hotel.contact.address.city.strip()) else None
            # state = core_hotel.contact.address.state if (core_hotel.contact.address.state and core_hotel.contact.address.state.strip()) else None
            # geo_geometry(city, state, hotelmapping.countryCode)
            # print(resp)

def geo_geometry(city, state=None, countryCode=None):

    index_name = 'varca-geo-places-vervotech-v4'

    if city is not None and len(city) < 1:
        return
    
    countryCodeX = countryCode
    if countryCodeX is not None:
        countryCodeX = countryCodeX.strip()
    else:
        countryCodeX = ''
    
    place_id_text = (str.lower(city.replace(' ', '').strip()) + str.lower(countryCodeX)).encode('utf-8')
    place_id = hashlib.sha1(place_id_text).hexdigest()

    # vervotech_place_feature_res = es_client.get(index=index_name, id=vervotech_place_id)
    place_exist = es_client.exists(index=index_name, id=place_id)
    if place_exist.body is True:
        return

    url = 'https://nominatim.openstreetmap.org/search'
    #print(1)

    params = {
        'format': 'jsonv2',
        'polygon_geojson': '1',
        'extratags':'0',
        'namedetails':'1',
        'addressdetails': '1',
        'limit': '1',
        'email': 'sujan@vervotech.com',
        'city': city
    }
    
    if state is not None:
        params['state'] = state
    if countryCode is not None:
        params['country'] = countryCode

    headers = {
        'Accept': 'application/json',
        'User-Agent': 'vervotech.com'     
    }

    time.sleep(1)
    res = requests.get(url, params=params, headers=headers)
    json_res = res.json()
    if (len(json_res) < 1): return

    osm_feature_res = json_res[0]
    
    geometry_type = osm_feature_res['geojson']['type']
    geometry_coord = osm_feature_res['geojson']['coordinates']
    location = Point([float(osm_feature_res['lon']), float(osm_feature_res['lat'])])
    extra = {
        'bbox': [float(ele) for ele in osm_feature_res['boundingbox']]
    }

    osm_name_details = get_dict_any(osm_feature_res, 'namedetails')
    
    alternate_names = []
    old_names = []

    if osm_name_details is not None:
        altername_name = get_dict_any(osm_name_details,'name:en')
        if altername_name is not None:
            alternate_names.append(altername_name)

        altername_name = get_dict_any(osm_name_details,'alt_name:en')
        if altername_name is not None:
            alternate_names.append(altername_name)
    
        old_name = get_dict_any(osm_name_details, 'old_name:en')
        if (old_name) is not None:
            old_names.append(old_name)

    osm_address = get_dict_any(osm_feature_res, 'address')

    properties = {
        'name': osm_feature_res['name'],
        'category': osm_feature_res['category'],
        'type': osm_feature_res['type'],
        'displayName': osm_feature_res['display_name'],
        'alternateNames': alternate_names,
        'oldNames': old_names,
        'geoLocation': location,
        'sourceId': 'osm-' + str(osm_feature_res['place_id']),
        'placeId': place_id,
        'address': {
            'city': get_dict_any(osm_address, 'city', ''),
            'stateDistrict': get_dict_any(osm_address, 'state_district', ''),
            'state': get_dict_any(osm_address, 'state', ''),
            'country': get_dict_any(osm_address,'country', ''),
            'countryCode': str.upper(get_dict_any(osm_address, 'country_code', '')),
            'postalCode': get_dict_any(osm_address, 'postcode', '')
        },
        'parentPlaceId': '',
        'source': 'vervotech'
    }

    
    if geometry_type.lower() == 'polygon':
        p = Polygon(geometry_coord)
    elif geometry_type.lower() == 'point':
        p = Point(coordinates=geometry_coord)
    elif geometry_type.lower() == 'multipolygon':
        p = MultiPolygon(coordinates=geometry_coord)
    elif geometry_type.lower() == 'multipoint':
        p = MultiPoint(coordinates=geometry_coord)
    elif geometry_type.lower() == 'linestring':
        p = LineString(coordinates=geometry_coord)
    elif geometry_type.lower() == 'multilinestring':
        p = MultiLineString(coordinates=geometry_coord)
    else:
        p = None

    f = Feature(place_id, geometry=p, properties=properties, **extra)
    fj = geojson.dumps(f)

    try:
        es_res = es_client.index(index=index_name, id=place_id, document=fj)
    except:
        print("error: " + city)
        return

    print(str(res.status_code) + ' - ' + geometry_type)

def get_dict_any(dictionary: dict, key: str, default=None) -> any:
    if dictionary is None: return default
    val = dictionary.get(key)
    if val is None:
        return default
    return val

def get_access_keys(publish_key: str, channel_ids: list[str]|None) -> list:
    access_keys = []

    if channel_ids is None:
        access_keys.append(publish_key.lower())
    else:
        for channel_id in channel_ids:
            access_keys.append(f'{publish_key}__{channel_id}'.lower())

    return access_keys


def get_client_provider_publish_key(client_provider_info: ClientProviderInfo) -> str:
    publish_key: str = ''
    if (client_provider_info.isExclusiveDownloader 
        or client_provider_info.isPrivate 
        or (client_provider_info.downloaderType is not None and client_provider_info.downloaderType.lower().startswith('ftp'))):
        publish_key = client_provider_info.id
    else:
        publish_key = client_provider_info.masterProviderKey
    return publish_key.lower()

def main(args):
    start(args[1], args[0])
    #geo_geometry('pune', countryCode='IN')
    es_client.close()
    mongo_client.close()

if __name__ == "__main__":
    main(sys.argv[1:])