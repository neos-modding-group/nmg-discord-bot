using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using nmgBot.jsonSchemas;
using nmgBot.Managers;
using System;
using System.Linq.Expressions;
using System.Linq;
using System.Threading.Tasks;

namespace nmgBot.Commands
{
    internal static class ExtCmds //most of this is gross code that works... il fix it later (at least thats what i tell myself)
    {
        static private List<string> responses = new();
        static private Dictionary<SlashCmd, int> slashCmdtoResp = new();
        static private Dictionary<int, string> RespToMsgCmd = new();
        internal static void SetUp()
        {
            BotMngr.client.Ready += DefineSlashCommands;
            BotMngr.client.SlashCommandExecuted += SlashCommandHandler;
            BotMngr.client.MessageReceived += MsgHandler;
            deserializeJson();
        }

        static void deserializeJson()
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
                            RespToMsgCmd.Add(resItem, si);
                        }
                    }
                }
            }
            catch (Exception e)
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
                builder.AddOption("msg_to_reply_to", ApplicationCommandOptionType.String, "msg id to reply to when sending the msg", false);
                cmds.Add(builder.Build());
            }
            await BotMngr.client.BulkOverwriteGlobalApplicationCommandsAsync(cmds.ToArray());
        }

        static async Task SlashCommandHandler(SocketSlashCommand command)
        {
            KeyValuePair<SlashCmd, int>? cmd = slashCmdtoResp.FirstOrDefault((d) => d.Key.name.ToLower() == command.Data.Name);
            if (cmd == null) return;
            IUser? user = (IUser)command.Data.Options.FirstOrDefault((o)=>o.Name=="user_to_ping")?.Value;
            ulong? msgid = null;
            string mtrt = (string)command.Data.Options.FirstOrDefault((o) => o.Name == "msg_to_reply_to")?.Value;
            try
            {
                msgid = Util.NullableUlongParse(mtrt);
            }
            catch(Exception e)
            {
                logWraper.Error($"error parsing string: {mtrt}");
            }
            string output = (user != null ? (user.Mention + Environment.NewLine) : "") + responses[cmd.Value.Value];
            if (msgid != null)
            {
                try
                {
                    await command.Channel.SendMessageAsync(output, messageReference: new(msgid, command.ChannelId, command.GuildId));
                    await command.RespondAsync("msg sent", ephemeral: true); // discord requires a response
                    return;
                } catch { }
            }
            await command.RespondAsync(output);
        }

        private static async Task MsgHandler(SocketMessage msg)
        {
            if ((msg is not SocketUserMessage message) || (msg.Author.Id == BotMngr.client.CurrentUser.Id)) return;
            string msgStr = msg.Content.ToLower();
            List<KeyValuePair<int, int>> foundStrs = new();
            foreach (var item in RespToMsgCmd)
            {
                int index = msgStr.IndexOf(item.Value.ToLower());
                if (index != -1) 
                {
                    foundStrs.Add(new(index, item.Key));
                }
            }

            if(foundStrs.Count == 0) return;
            
            foundStrs.Sort(strCmdSort);

            string logFoundItemsStr = "";
            string msgOutput = "";

            foreach (var item in foundStrs)
            {
                logFoundItemsStr += $"{item.Key}:{RespToMsgCmd[item.Value]} ";// will have a trailing space
                msgOutput += responses[item.Value] + Environment.NewLine + Environment.NewLine;
            }
            logWraper.Log($"found {logFoundItemsStr}in {msg.Content} sent by {message.Author.FormatedName()} in {msg.Channel.FormatedName()} {message.Channel.GuildFromChannel()?.FormatedName()}");
                
            if (msg.Reference != null)
                await message.Channel.SendMessageAsync(msgOutput, messageReference: message.Reference);
            else
                await message.ReplyAsync(msgOutput); 
            //await Util.GuildFromId(901126079857692714).ChannelFromId(1003519455344738324).SendMessageAsync("perms test");
        }
        private static int strCmdSort(KeyValuePair<int, int> a, KeyValuePair<int, int> b)
        {
            int ret = a.Key - b.Key; //first sort by index of found str
            if (ret == 0)
            {
                ret = a.Value - b.Value; //secondly sort based on index of reply definition 
            }
            return ret;
        }
    }
}
