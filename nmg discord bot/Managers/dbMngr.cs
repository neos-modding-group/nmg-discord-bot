using LiteDB;
using nmgBot.Schemas;

namespace nmgBot.Managers
{
    internal static class dbMngr
    {
        static LiteDatabase db;
        static ILiteCollection<string> staticsCol;
        public static void SetUp()
        {
            db = new LiteDatabase(Path.Combine(Directory.GetCurrentDirectory(), "db.litedb"));
            staticsCol = db.GetCollection<string>("State");
            staticsCol.EnsureExistance("sha");
        }

        public static string sha
        {
            get
            {
                return staticsCol.FindOne("$.sha");
            }
            set
            {
                staticsCol.Update("$.sha", value);
            }
        }
    }
}
