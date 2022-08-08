using Discord.WebSocket;
using nmgBot;

namespace nmgBot.Managers
{
    internal static class logMngr
    {
        internal static void SetUp()
        {
            BotMngr.client.SlashCommandExecuted += SlashCommandHandler;
        }
        static async Task SlashCommandHandler(SocketSlashCommand command)
        {
            logWraper.Log($"{command.User.FormatedName()} ran SlashCommand: {command.Data.Name}{(command.Data.Options.Count>0?" with options "+command.Data.Options.OptionsToString():" ")}in {command.Channel.Name} ({command.Channel.Id}) {(command.GuildId.HasValue?Util.GuildNameFromId(command.GuildId.Value):"")} ({command.GuildId})");

        }
    }
}
