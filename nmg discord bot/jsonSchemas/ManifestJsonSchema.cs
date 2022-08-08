namespace nmgBot.jsonSchemas
{
    //credit to delta https://github.com/XDelta
    public class ManifestInfo
    {
        public string schemaVersion { get; set; }
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
        public string name { get; set; }
        public string description { get; set; }
        public Dictionary<string, Author> authors { get; set; }
        public string sourceLocation { get; set; }
        public string website { get; set; }
        public string[] tags { get; set; }
        public string category { get; set; }
        public string[] flags { get; set; }
        public Dictionary<string, ModVersion> versions { get; set; }
    }

    public class Author
    {
        public string url { get; set; }
    }
    public class ModVersion
    {
        public string changelog { get; set; }
        public string releaseUrl { get; set; }
        public string neosVersionCompatibility { get; set; }
        public string modloaderVersionCompatibility { get; set; }
        public string[] flagList { get; set; }
        public string[] conflicts { get; set; }
        public Dictionary<string, Dependency> dependencies { get; set; }
        public Artifact[] artifacts { get; set; }
    }

    public class Dependency
    {
        public string version { get; set; } //(semver version specifier)
    }

    public class Artifact
    {
        public string url { get; set; }
        public string filename { get; set; }
        public string sha256 { get; set; }
        //should be fine to keep or add new formats as needed, should fill any that exist
        public string blake3 { get; set; } //for LJ's mods xD
        public string installLocation { get; set; }
    }
}