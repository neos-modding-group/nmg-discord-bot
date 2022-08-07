using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace nmgBot
{
    internal static class ExtCmds //most of this is gross code that works... il fix it later (at least thats what i tell myself)
    {
        static private List<string> responses = new();
        static private Dictionary<SlashCmd, int> slashCmdtoResp = new();
        static private Dictionary<string, int> msgCmdtoResp = new();
        internal static async void SetUp()
        {
            BotMngr.client.Ready += DefineSlashCommands;
            BotMngr.client.SlashCommandExecuted += SlashCommandHandler;
            BotMngr.client.MessageReceived += MsgHandler;
            await deserializeJson();
        }


        static async Task deserializeJson()
        {
            try
            {
                jsonFile json;
                using (StreamReader sr = new StreamReader(Path.Combine(Directory.GetCurrentDirectory(), "ExtCmds.json")))
                {
                    json = JsonConvert.DeserializeObject<jsonFile>(sr.ReadToEnd());
                }
                foreach (var item in json.Responses)
                {
                    int resItem = responses.Count; // this is stupid and jank but works for now
                    responses.Add(item.Response);

                    if (item.SlashCmds != null)
                    {
                        foreach (var si in item.SlashCmds)
                        {
                            slashCmdtoResp.Add(si, resItem);
                        }
                    }
                    if (item.MsgCmds != null)
                    {
                        foreach (var si in item.MsgCmds)
                        {
                            msgCmdtoResp.Add(si, resItem);
                        }
                    }
                }
            }
            catch(Exception e)
            {
                logWraper.Error("error deserializing json", exception: e);
            }
        }
        static async Task DefineSlashCommands()
        {
            List<ApplicationCommandProperties> cmds = new();
            foreach (var item in slashCmdtoResp)
            {
                SlashCommandBuilder builder = new();
                builder.WithName(item.Key.name.ToLower()); // discord requires slash commands name's to be lower case
                builder.WithDescription(item.Key.desc);
                builder.AddOption("user_to_ping", ApplicationCommandOptionType.User, "user to ping when sending the msg", false);
                cmds.Add(builder.Build());
            }
            await BotMngr.client.BulkOverwriteGlobalApplicationCommandsAsync(cmds.ToArray());
        }

        static async Task SlashCommandHandler(SocketSlashCommand command)
        {
            logWraper.Log($"{command.User.FormatedName()} ran SlashCommand: {command.Data.Name} in {command.Channel.Name} ({command.Channel.Id}) {command.GuildId}");
            await command.RespondAsync(((command.Data.Options.Count > 0)?(((IUser)command.Data.Options.First().Value).Mention+Environment.NewLine):"")+responses[slashCmdtoResp.First((d) => d.Key.name.ToLower() == command.Data.Name).Value]);
        }

        private static async Task MsgHandler(SocketMessage msg)
        {
            if (!(msg is SocketUserMessage message)) return;
            string msgStr = msg.Content.ToLower();
            foreach (var item in msgCmdtoResp) {
                if (msgStr.Contains(item.Key.ToLower()))
                {
                    logWraper.Log($"found {item.Key} in {msg.Content} sent by {message.Author.FormatedName()} in {msg.Channel.Name}");
                    await message.ReplyAsync(responses[item.Value]);
                    return;
                }
            }
        }
    }
}
