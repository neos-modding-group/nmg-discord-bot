using Newtonsoft.Json;
using System.Net;
using nmgBot.jsonSchemas;

namespace nmgBot.Managers
{
    internal static class manifestMngr
    {
        private static ManifestInfo latest;

        private static string lastSHA = "realghas"; //tmp for debugging will replace with db

        private static Timer Timer;

        public static void SetUp()
        {
            Timer = new Timer(e => updateManifest(), null, TimeSpan.Zero, TimeSpan.FromMinutes(15)); // disabled to not spam the github api while testing other stuff
        }

        public static ManifestInfo Manifest
        {
            get
            {
                if (latest == null) latest = GetWebManifest();
                return latest;
            }
        }

        private static async void updateManifest()
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    string file = client.DownloadString($"https://api.github.com/repos/neos-modding-group/neos-mod-manifest/commits/master");
                    var commits = JsonConvert.DeserializeObject<SimpleGithubCommitsJsonSchema>(file);
                    if (commits.sha == lastSHA) return;
                    lastSHA = commits.sha;
                }
            }
            catch (Exception e)
            {
                logWraper.Error("Exception getting commits from manifest", e);
            }
            latest = GetWebManifest();
        }

        static private ManifestInfo GetWebManifest()
        {
            using (WebClient client = new WebClient())
            {
                string file = client.DownloadString($"https://raw.githubusercontent.com/neos-modding-group/neos-mod-manifest/master/manifest.json");
                return JsonConvert.DeserializeObject<ManifestInfo>(file);
            }
        }

    }
}
