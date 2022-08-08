using Discord;
using Discord.WebSocket;
using nmgBot.Commands;
using System;
using System.Threading.Tasks;

namespace nmgBot.Managers
{
    internal static class BotMngr
    {
        static internal readonly DiscordSocketClient client = new();
        public static async Task MainAsync()
        {
            client.Log += async (msg) =>
            {
                Console.WriteLine(msg.ToString());
                await Task.CompletedTask;
            };

            var token = Environment.GetEnvironmentVariable("nmg_bot_token", EnvironmentVariableTarget.User);

            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();


            logMngr.SetUp();
            ExtCmds.SetUp();
            msgFileMngr.SetUp();
            manifestMngr.SetUp();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }
    }
}