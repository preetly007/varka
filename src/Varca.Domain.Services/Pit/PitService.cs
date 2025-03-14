using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Varca.Domain.Models;
using Varca.Domain.Repositories;

namespace Varca.Domain.Services;

public class PitService : IPitService
{
    private readonly IPitStreamProducer _pitStreamProducer;
    private readonly IPitResultStore _pitResultStore;
    private readonly IAccountService _accountService;
    private readonly ILogger<PitService> _logger;

    public PitService(ILogger<PitService> logger, IPitStreamProducer pitStreamProducer, IPitResultStore pitResultStore, IAccountService accountService) {
        _pitStreamProducer = pitStreamProducer;
        _accountService = accountService;
        _pitResultStore = pitResultStore;
        _logger = logger;
    }
    public async Task<PitGeometryNew?> CreateByMultiPolygonAsync(PitGeometryNew pitNew)
    {
        pitNew.PitId = VarcaId.New();
        pitNew.CreatedAtUtc = DateTimeOffset.UtcNow;
        pitNew.TTL = 20;

        pitNew = await _accountService.BuildAccessKeysFilterAsync(pitNew);
        if (pitNew.Filters.AccessKeys.Count == 0)
            return null;

        await _pitResultStore.SaveGeometryAsync(pitNew);
        var pitNewResult = await _pitStreamProducer.SendGeometryAsync(pitNew);
        
        return pitNewResult;
    }

    public async Task<PitPlaceNew?> CreateByPlaceAsync(PitPlaceNew pitNew)
    {
        pitNew.PitId = VarcaId.New();
        pitNew.CreatedAtUtc = DateTimeOffset.UtcNow;
        pitNew.TTL = 20;

        pitNew = await _accountService.BuildAccessKeysFilterAsync(pitNew);
        if (pitNew.Filters.AccessKeys.Count == 0)
            return null;

        await _pitResultStore.SavePlaceAsync(pitNew);
        var pitNewResult = await _pitStreamProducer.SendPlaceAsync(pitNew);
        return pitNewResult;
    }

    public async Task<PitGeometryNew?> CreateByPointAsync(PitGeometryNew pitNew)
    {
        pitNew.PitId = VarcaId.New();
        pitNew.CreatedAtUtc = DateTimeOffset.UtcNow;
        pitNew.TTL = 20;

        pitNew = await _accountService.BuildAccessKeysFilterAsync(pitNew);
        if (pitNew.Filters.AccessKeys.Count == 0)
            return null;

        await _pitResultStore.SaveGeometryAsync(pitNew);
        var pitNewResult = await _pitStreamProducer.SendGeometryAsync(pitNew);
        return pitNewResult;
    }

    public async Task<PitGeometryNew?> CreateByPolygonAsync(PitGeometryNew pitNew)
    {
        pitNew.PitId = VarcaId.New();
        pitNew.CreatedAtUtc = DateTimeOffset.UtcNow;
        pitNew.TTL = 20;

        pitNew = await _accountService.BuildAccessKeysFilterAsync(pitNew);
        if (pitNew.Filters.AccessKeys.Count == 0)
            return null;

        await _pitResultStore.SaveGeometryAsync(pitNew);
        var pitNewResult = await _pitStreamProducer.SendGeometryAsync(pitNew);
        
        return pitNewResult;
    }

    public async Task<PitInfo?> GetByIdAsync(string pitId, string accountId)
    {
        var pitInfo = await _pitResultStore.GetByIdAsync(pitId, accountId);
        return pitInfo;
    }
}
