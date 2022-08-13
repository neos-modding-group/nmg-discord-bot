using Discord.WebSocket;

namespace nmgBot.Managers
{
    static internal class msgFileMngr
    {
        internal static void SetUp()
        {
            BotMngr.client.MessageReceived += MsgHandler;
        }

        private static Task MsgHandler(SocketMessage msg)
        {
            foreach (var att in msg.Attachments)
            {
                logWraper.Debug(att.Filename);
                logWraper.Debug(att.Url);
                logWraper.Debug(att.ProxyUrl);
                logWraper.Debug(att.ContentType);
                logWraper.Debug(att.Description);
                logWraper.Debug(att.ToString());
            }
            return Task.CompletedTask;
        }
    }
}
