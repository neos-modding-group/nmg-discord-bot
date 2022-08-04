using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace nmgBot
{
    //credit to delta https://github.com/XDelta
    public class ManifestInfo
    {
        [JsonPropertyName("schemaVersion")]
        public string SchemaVersion { get; set; }
        [JsonPropertyName("mods")]
        public Dictionary<string, ModInfo> mods { get; set; }

        public int Count()
        {
            if (mods is null)
            {
                return 0;
            }
            return mods.Count;
        }
    }
    public class ModInfo
    {
        [JsonPropertyName("name")]
        public string name { get; set; }

        [JsonPropertyName("description")]
        public string description { get; set; }

        [JsonPropertyName("authors")]
        public Dictionary<string, Author> authors { get; set; }

        [JsonPropertyName("sourceLocation")]
        public string sourceLocation { get; set; }

        [JsonPropertyName("tags")]
        public string[] tags { get; set; }

        [JsonPropertyName("category")]
        public string category { get; set; }

        [JsonPropertyName("flags")]
        public string[] flags { get; set; }

        [JsonPropertyName("versions")]
        public Dictionary<string, ModVersion> versions { get; set; }

        //Data from installed version if one exists
        public string installedVersion { get; set; }

        public string installedHash { get; set; }

        public bool modEnabled { get; set; }
        //public string installedFilePath { get; set; };
    }

    public class Author
    {
        //public string author { get; set; }
        [JsonPropertyName("url")]
        public string url { get; set; }

    }
    public class ModVersion
    {

        [JsonPropertyName("changelog")]
        public string changelog { get; set; }

        [JsonPropertyName("releaseUrl")]
        public string releaseUrl { get; set; }

        [JsonPropertyName("neosVersionCompatibility")]
        public string neosVersionCompatibility { get; set; }

        //TODO Dependencies

        //TODO Artifacts
        [JsonPropertyName("artifacts")]
        public Artifact[] artifacts { get; set; }
    }

    public class Artifact
    {
        [JsonPropertyName("url")]
        public string url { get; set; }
        [JsonPropertyName("filename")]
        public string filename { get; set; }
        [JsonPropertyName("sha256")]
        public string sha256 { get; set; }

        //should be fine to keep or add new formats as needed, should fill any that exist
        [JsonPropertyName("blake3")]
        public string blake3 { get; set; } //for LJ's mods xD
        [JsonPropertyName("installLocation")]
        public string installLocation { get; set; }
    }
}