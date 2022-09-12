﻿using Newtonsoft.Json;
using System.Net;
using nmgBot.Schemas;

namespace nmgBot.Managers
{
    internal static class manifestMngr
    {
        private static ManifestInfo latest;
        
        private static Timer Timer;
        
        public static void SetUp() => Timer = new Timer(e => updateManifest(), null, TimeSpan.Zero, TimeSpan.FromMinutes(15));

        public static ManifestInfo Manifest
        {
            get
            {
                if (latest == null) latest = GetWebManifest().Result;
                return latest;
            }
        }
        
        private static async void updateManifest() => latest = await GetWebManifest();
        
        private static async Task<ManifestInfo> GetWebManifest() => JsonConvert.DeserializeObject<ManifestInfo>(await Util.GetString("https://raw.githubusercontent.com/neos-modding-group/neos-mod-manifest/master/manifest.json"));
    }
}