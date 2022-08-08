using Discord.WebSocket;
using nmgBot.Managers;
using Discord;
using System.Collections.Generic;

namespace nmgBot
{
    internal static class Util
    {
        public static string FormatedName(this SocketUser user) => $"{user.Username} # {user.Discriminator} ({user.Id})";
        public static string FormatedName(this ISocketMessageChannel channel) => $"{channel.Name} (st{channel.Id})";
        public static string FormatedName(this SocketGuild guild) => $"{guild.Name} ({guild.Id})";
        public static string GuildNameFromId(ulong GuildId) => GuildFromId(GuildId).Name;
        public static SocketGuild GuildFromId(ulong GuildId) => BotMngr.client.Guilds.First((e) => GuildId == e.Id);
        public static SocketGuild? GuildFromChannel(this ISocketMessageChannel channel) => (channel as SocketGuildChannel)?.Guild;
        public static SocketTextChannel ChannelFromId(this SocketGuild guild, ulong ChannelId) => guild.TextChannels.First((e) => ChannelId == e.Id);
        public static string OptionsToString(this IReadOnlyCollection<IApplicationCommandInteractionDataOption> options)
        {
            string output = "";
            foreach(var cmd in options) output += $"[ {cmd.Type} {cmd.Name}:{cmd.Value} {cmd.Options.OptionsToString()}] ";
            return output;
        }
        public static ulong? NullableUlongParse(string? u) => u == null ? null : ulong.Parse(u); //there is probably a better way to do this
    }
}
