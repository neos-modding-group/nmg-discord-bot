using Discord.WebSocket;

namespace nmgBot.Managers
{
	internal static class logMngr
	{
		internal static void SetUp()
		{
			BotMngr.client.SlashCommandExecuted += SlashCommandHandler;
			BotMngr.client.SelectMenuExecuted += LogSocketMessageComponent;
			BotMngr.client.ButtonExecuted += LogSocketMessageComponent;
		}

		private static async Task SlashCommandHandler(SocketSlashCommand command) =>
			logWraper.Log($"{command.User.FormatedName()} ran SlashCommand: {command.Data.Name}{(command.Data.Options.Count > 0 ? " with options " + command.Data.Options.OptionsToString() : " ")}in {command.Channel.Name} ({command.Channel.Id}) {(command.GuildId.HasValue ? Util.GuildNameFromId(command.GuildId.Value) : "")} ({command.GuildId})");
		private static async Task LogSocketMessageComponent(SocketMessageComponent component) =>
			logWraper.Log($"{component.User.FormatedName()} interacted with a {component.Data.Type} CustomId: {component.Data.CustomId}{(component.Data.Value == null ? "" : " Value: "+component.Data.Value)}{(component.Data?.Values?.Count>0? " Values: " + component.Data.Values.ConcatStrs() :"")} on {component.Message.Id} in {component.Channel.Name} ({component.Channel.Id}) {(component.GuildId.HasValue ? Util.GuildNameFromId(component.GuildId.Value) : "")} ({component.GuildId})");
	}
}
