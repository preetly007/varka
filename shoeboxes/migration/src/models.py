from typing import Optional, List, Annotated
from pydantic import BaseModel, Field, BeforeValidator
from datetime import datetime

class RoomMappingSettings(BaseModel): 
    isRoomMappingEnabled:Optional[bool] = Field(False, alias='IsRoomMappingEnabled')
    isRoomContentAvailable:Optional[bool] = Field(False, alias='IsRoomContentAvailable')

class KeyValue(BaseModel):
    key:str = Field(alias='Key')
    value:Optional[str] = Field(None, alias='Value')
    desc:Optional[str] = Field(None, alias='Desc')

class ClientProviderInfo(BaseModel):
    id:str = Field(alias='_id')
    accountId:Optional[str] = Field(None, alias='AccountId')
    credentialsAccountId:Optional[str] = Field(None, alias='CredentialsAccountId')
    providerFamily:Optional[str] = Field(None, alias='ProviderFamily')
    masterProviderKey:Optional[str] = Field(None, alias='MasterProviderKey')
    providerSchedule:Optional[str] = Field(None, alias='ProviderSchedule')
    isInvalidCredentials:Optional[bool] = Field(False, alias='IsInvalidCredentials')
    providerFamilyCode:Optional[str] = Field(None, alias='ProviderFamilyCode')
    isDisabled:Optional[bool] = Field(False, alias='IsDisabled')
    isAsyncPerHotelDownload:Optional[bool] = Field(False, alias='IsAsyncPerHotelDownload')
    attributes:Optional[List[KeyValue]] = Field(None, alias='Attributes')
    supportedLanguages:Optional[List[str]] = Field(None, alias='SupportedLanguages')
    downloaderType:Optional[str] = Field(None, alias='DownloaderType')
    isPerformExtMappingComparision:Optional[bool] = Field(False, alias='IsPerformExtMappingComparision')
    isExclusiveDownloader:Optional[bool] = Field(False, alias='IsExclusiveDownloader')
    isPrivate:Optional[bool] = Field(False, alias='IsPrivate')
    providerSchedules:Optional[List[str|None]] = Field(None, alias='ProviderSchedules')
    imageOperations:Optional[str] = Field(False, alias='ImageOperations')
    isSaveRawData:Optional[bool] = Field(False, alias='IsSaveRawData')
    createdOn:Optional[datetime] = Field(None, alias='CreatedOn')
    updatedOn:Optional[datetime] = Field(None, alias='UpdatedOn')
    updatedBy:Optional[str] = Field(None, alias='UpdatedBy')
    contentSourceOptions:Optional[List[str]] = Field(None, alias='ContentSourceOptions')
    selectedContentSourceOption:Optional[str] = Field(None, alias='SelectedContentSourceOption')
    availableChannelIds:Optional[List[str]] = Field(None, alias='AvailableChannelIds')
    requestedChannelIds:Optional[List[str]] = Field(None, alias='RequestedChannelIds')
    approvedChannelIds:Optional[List[str]] = Field(None, alias='ApprovedChannelIds')
    approvalStatus:Optional[str] = Field(None, alias='ApprovalStatus')
    isMergeMappingChannelIds:Optional[bool] = Field(False, alias='IsMergeMappingChannelIds')
    roomMappingSettings:Optional[RoomMappingSettings] = Field(None, alias='RoomMappingSettings')
    isRestrictInvalidHotelsDisabled:Optional[bool] = Field(False, alias='IsRestrictInvalidHotelsDisabled')
    shouldUseSuperMasterIndexForMatchingHotels:Optional[bool] = Field(False, alias='ShouldUseSuperMasterIndexForMatchingHotels')
    shouldCheckImagesAndRooms:Optional[bool] = Field(False, alias='ShouldCheckImagesAndRooms')
    techPartnerId:Optional[str] = Field(None, alias='TechPartnerId')
    isSaveMappingResults:Optional[bool] = Field(False, alias='IsSaveMappingResults')
    shouldProcessFtpFile:Optional[bool] = Field(False, alias='ShouldProcessFTPFile')


class GeoCoordinate(BaseModel):
    lat: float = Field(alias='Lat')
    lon: float = Field(alias='Long')

class HotelAddress(BaseModel):
    countryCode:Optional[str] = Field(None, alias='CountryCode')
    state:Optional[str] = Field(None, alias='State')
    city:Optional[str] = Field(None, alias='City')
    locality:Optional[str] = Field(None, alias='Locality')
    postalCode:Optional[str] = Field(None, alias='PostalCode')


class HotelContact(BaseModel):
    address:Optional[HotelAddress] = Field(None, alias='Address')
    phones:Optional[List[str|None]] = Field(None, alias='Phones')
    fax:Optional[str] = Field(None, alias='Fax')
    emails:Optional[List[str|None]] = Field(None, alias='Emails')
    web:Optional[str] = Field(None, alias='Web')
    formattedAddress:Optional[str] = Field(None, alias='FormattedAddress')

class CoreHotel(BaseModel):
    unicaId:int = Field(alias='UnicaId')
    name:Optional[str] = Field(None, alias='Name')
    providerFamily:Optional[str] = Field(None, alias='ProviderFamily')
    providerHotelId:Optional[str] = Field(None, alias='ProviderHotelId')
    geocode:Optional[GeoCoordinate] = Field(None, alias='GeoCode')
    channelIds:Optional[List[str|None]] = Field(None, alias='ChannelIds')
    extMasterId:Optional[str] = Field(None, alias='ExtMasterId')
    disabled:bool = Field(False, alias='Disabled')
    propertyType:Optional[str] = Field(None, alias='PropertyType')
    masterPropertyType:Optional[str] = Field(None, alias='MasterPropertyType')
    contact:Optional[HotelContact] = Field(None, alias='Contact')
    brandCode:Optional[str] = Field(None, alias='BrandCode')
    chainCode:Optional[str] = Field(None, alias='ChainCode')
    rating:Optional[str] = Field(None, alias='Rating')
    masterBrandCode:Optional[str] = Field(None, alias='MasterBrandCode')
    masterChainCode:Optional[str] = Field(None, alias='MasterChainCode')
    


class GeoPoint(BaseModel):
    type:str = Field('Point', alias='Type')
    coordinates:List[float] = Field(None, alias='Coordinates')


PyObjectId = Annotated[str, BeforeValidator(str)]

class HotelMapping(BaseModel):
    id:str = Field(alias='_id', serialization_alias='_id')
    # accountId:str = Field(alias='AccountId')
    # providerKey:str = Field(alias='ProviderKey')
    # masterProviderKey:str = Field(alias='MasterProviderKey')
    publishKey:Optional[str] = Field(None, alias='PublishKey')
    unicaId:int = Field(alias='UnicaId')
    # createdOn:Optional[datetime] = Field(None, alias='CreatedOn')
    # modifiedOn:Optional[datetime] = Field(None, alias='ModifiedOn')
    providerFamily:str = Field(None, alias='ProviderFamily')
    providerHotelId:str = Field(None, alias='ProviderHotelId')
    providerLocationCode:Optional[str] = Field(None, alias='ProviderLocationCode')
    propertyType:Optional[str] = Field(None, alias='PropertyType')
    channelIds:Optional[List[str|None]] = Field(None, alias='ChannelIds')
    runId:str = Field(None, alias='RunId')
    geoLocation:Optional[GeoPoint] = Field(None, alias='GeoLocation')
    #extMasterId:Optional[str] = Field(None, alias='ExtMasterId')
    countryCode:Optional[str] = Field(None, alias='CountryCode')
    rating:Optional[str] = Field(None, alias='Rating')
    chainCode:Optional[str] = Field(None, alias='ChainCode')
    brandCode:Optional[str] = Field(None, alias="BrandCode")
    accessKeys:Optional[List[str|None]] = Field(None, alias="AccessKeys")
    lastUpdatedAt:Optional[datetime] = Field(None, alias="LastUpdatedAt")

    # state:Optional[str] = Field(None, alias='State')
    # city:Optional[str] = Field(None, alias='City')
    # locality:Optional[str] = Field(None, alias='Locality')
    # postalCode:Optional[str] = Field(None, alias='PostalCode')
    #downloaderType:Optional[str] = Field(None, alias='DownloaderType')
    # class Config:
    #     allow_population_by_field_name = False
    #     arbitrary_types_allowed = True


 