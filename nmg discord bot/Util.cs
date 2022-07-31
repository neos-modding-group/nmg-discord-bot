using Discord.WebSocket;

namespace nmgBot
{
    internal static class Util
    {
        public static string FormatedName(this SocketUser user) => $"{user.Username} # {user.Discriminator} ({user.Id})";
    }
}
