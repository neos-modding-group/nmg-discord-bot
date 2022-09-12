using Discord.WebSocket;

namespace nmgBot.Managers
{
    internal static class logMngr
    {
        internal static void SetUp()
        {
            BotMngr.client.SlashCommandExecuted += SlashCommandHandler;
            BotMngr.client.SelectMenuExecuted += Client_SelectMenuExecuted;
        }

        private static async Task SlashCommandHandler(SocketSlashCommand command) =>
            logWraper.Log($"{command.User.FormatedName()} ran SlashCommand: {command.Data.Name}{(command.Data.Options.Count > 0 ? " with options " + command.Data.Options.OptionsToString() : " ")}in {command.Channel.Name} ({command.Channel.Id}) {(command.GuildId.HasValue ? Util.GuildNameFromId(command.GuildId.Value) : "")} ({command.GuildId})");


        private static async Task Client_SelectMenuExecuted(SocketMessageComponent component) =>
            logWraper.Log($"{component.User.FormatedName()} interacted with MessageComponent CustomId: {component.Data.CustomId} Type: {component.Data.Type} Value: {component.Data.Value} Values: {component.Data.Values.ConcatStrs()} in {component.Channel.Name} ({component.Channel.Id}) {(component.GuildId.HasValue ? Util.GuildNameFromId(component.GuildId.Value) : "")} ({component.GuildId})");
    }
}
