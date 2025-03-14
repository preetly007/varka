using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unica.Core.Contracts.Models;

namespace Varca.Domain.Models;

public static class MethodExtensions
{
    public static string GetPublishKey(this ClientProviderInfo clientProviderInfo) {
        var publishKey = string.Empty;
        if (clientProviderInfo.IsExclusiveDownloader 
            || clientProviderInfo.IsPrivate
            || (clientProviderInfo.DownloaderType != null 
                && clientProviderInfo.DownloaderType.Trim().ToLowerInvariant().StartsWith("ftp")) ) {
            if (!string.IsNullOrEmpty(clientProviderInfo.Id))
                publishKey = clientProviderInfo.Id;
        }  
        else if (!string.IsNullOrEmpty(clientProviderInfo.MasterProviderKey)) {
            publishKey = clientProviderInfo.MasterProviderKey;
        }
        return publishKey.Trim().ToLower();
    }
}
