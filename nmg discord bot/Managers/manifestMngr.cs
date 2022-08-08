using Newtonsoft.Json;
using System.Net;
using Octokit;
using nmgBot.jsonSchemas;

namespace nmgBot.Managers
{
    internal static class manifestMngr
    {
        private static ManifestInfo latest;

        private static string lastSHA = "realghas"; //tmp for debugging will replace with db

        private static Timer Timer;

        private static GitHubClient gitHub;

        private const string repoOwner = "neos-modding-group";
        private const string repoName = "neos-mod-manifest";

        public static void SetUp()
        {
            gitHub = new GitHubClient(new ProductHeaderValue("nmg-discord-bot"));
            string? token = Environment.GetEnvironmentVariable("nmg_bot_github_token", EnvironmentVariableTarget.User);
            if (token != null)
            {
                gitHub.Credentials = new Credentials(token);
            }
            //Timer = new Timer(e => updateManifest(), null, TimeSpan.Zero, TimeSpan.FromMinutes(15)); // disabled to not spam the github api while testing other stuff
        }

        public static ManifestInfo Manifest
        {
            get
            {
                if (latest == null)
                    latest = GetWebManifest();
                return latest;
            }
        }

        private static async void updateManifest()
        {
            string curentSHA = "";
            try
            {
                var refs = await gitHub.Repository.Commit.GetAll(repoOwner, repoName);
                curentSHA = refs[0].Sha;
            }
            catch (Exception e)
            {
                logWraper.Error("Exception getting commits from manifest", e);
            }
            if (curentSHA == lastSHA)
            {
                return;
            }
            lastSHA = curentSHA;
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
