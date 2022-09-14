using Discord;
using Discord.WebSocket;
using nmgBot.Managers;

namespace nmgBot
{
	internal static class Util
	{
		public static string FormatedName(this IUser user) => $"{user.Username} # {user.Discriminator} ({user.Id})";
		public static string FormatedName(this ISocketMessageChannel channel) => $"{channel.Name} (st{channel.Id})";
		public static string FormatedName(this IGuild guild) => $"{guild.Name} ({guild.Id})";
		public static string FormatedName(this IChannel Channel) => $"{Channel.Name} ({Channel.Id})";
		public static string GuildNameFromId(ulong GuildId) => GuildFromId(GuildId).Name;
		public static SocketGuild GuildFromId(ulong GuildId) => BotMngr.client.Guilds.First((e) => GuildId == e.Id);
		public static SocketGuild? GuildFromChannel(this ISocketMessageChannel channel) => (channel as SocketGuildChannel)?.Guild;
		public static SocketTextChannel ChannelFromId(this SocketGuild guild, ulong ChannelId) => guild.TextChannels.First((e) => ChannelId == e.Id);
		public static string OptionsToString(this IReadOnlyCollection<IApplicationCommandInteractionDataOption> options)
		{
			string output = "";
			foreach (var cmd in options) output += $"[ {cmd.Type} {cmd.Name}:{cmd.AutoFormatedValue()} {cmd.Options.OptionsToString()}] ";
			return output;
		}
		public static string AutoFormatedValue(this IApplicationCommandInteractionDataOption oacodo)
		{
			switch (oacodo.Type)
			{
				case ApplicationCommandOptionType.User:
					return ((IUser)oacodo.Value).FormatedName();
				case ApplicationCommandOptionType.Channel:
					return ((IChannel)oacodo.Value).FormatedName();
				case ApplicationCommandOptionType.Mentionable:
					return (oacodo.Value is IUser) ? ((IUser)oacodo.Value).FormatedName() : oacodo.Value.ToString();
				default:
					return oacodo.Value.ToString();
			}
		}
		public static ulong? NullableUlongParse(string? u) => u == null ? null : ulong.Parse(u); //there is probably a better way to do this
		public static async Task<string> GetString(string url)
		{
			using (HttpClient client = new())
			{
				client.DefaultRequestHeaders.Add("User-Agent", "nmg-discord-bot");
				return await client.GetStringAsync(url);
			}
		}

		public static string LenCap(this string str, int cap)
		{
			if (str == null) return null;
			return str.Length > cap ? str.Substring(0, cap) : str;
		}

		public static string ConcatStrs(this IEnumerable<string> strings, string seporator = " ")
		{
			string o = "";
			foreach (string str in strings)
			{
				o += str + seporator;
			}
			if (strings.Count() > 0) o = o.Substring(0, o.Length - seporator.Length);
			return o;
		}
		public static EmbedBuilder WithDefults(this EmbedBuilder builder, string val) => builder.WithFooter(val, "https://avatars.githubusercontent.com/u/101987083?s=200&v=4").WithCurrentTimestamp().WithColor(new Color(255, 255, 255));

		public static string GetLastUrlSection(this string url)
		{
			if (url.Last() == '/') url = url.Substring(0, url.Length - 1);
			for (int i = url.Length - 1; i >= 0; i--)
			{
				if (url[i] == '/')
				{
					url = url.Substring(i + 1);
					break;
				}
			}
			return url;
		}
	}
}