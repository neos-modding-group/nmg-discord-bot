using Newtonsoft.Json;
using System.Net;
using nmgBot.Schemas;

namespace nmgBot.Managers
{
    internal static class manifestMngr
    {
        private static ManifestInfo latest;

        private static string lastSHA = "real sha"; //tmp for debugging will replace with db

        private static Timer Timer;

        public static void SetUp()
        {
            Timer = new Timer(e => updateManifest(), null, TimeSpan.Zero, TimeSpan.FromMinutes(15));
        }

        public static ManifestInfo Manifest
        {
            get
            {
                if (latest == null) latest = GetWebManifest().Result;
                return latest;
            }
        }

        private static async void updateManifest()
        {
            try
            {
                string file = await Util.GetString("https://api.github.com/repos/neos-modding-group/neos-mod-manifest/commits/master");
                var commits = JsonConvert.DeserializeObject<SimpleGithubCommitsJsonSchema>(file);
                if (commits.sha == lastSHA) return;
                lastSHA = commits.sha;
            }
            catch (Exception e)
            {
                logWraper.Error("Exception getting commits from manifest", e);
            }
            latest = await GetWebManifest();
        }

        private static async Task<ManifestInfo> GetWebManifest()
        {
            return JsonConvert.DeserializeObject<ManifestInfo>(await Util.GetString("https://raw.githubusercontent.com/neos-modding-group/neos-mod-manifest/master/manifest.json"));
        }
    }
}
