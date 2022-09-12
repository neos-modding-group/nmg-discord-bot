using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nmgBot.Schemas;
using nmgBot.Managers;

namespace nmgBot.Commands
{
    internal static class SearchCmd
    {
        internal static void SetUp()
        {
            BotMngr.client.Ready += DefineSlashCommands;
            BotMngr.client.SlashCommandExecuted += SlashCommandHandler;
            BotMngr.client.SelectMenuExecuted += Client_SelectMenuExecuted;
        }

        const string CmdName = "search";
        const string SearchTermArgName = "search_term";
        const string AuthorArgName = "author";
        const string CategoryArgName = "category";

        static async Task DefineSlashCommands()
        {
            SlashCommandBuilder builder = new();
            builder.WithName(CmdName);
            builder.WithDescription("search our modmanifest, no arguments will show all mods");
            builder.AddOption(SearchTermArgName, ApplicationCommandOptionType.String, "what mod names/tags to search for", false);
            builder.AddOption(AuthorArgName, ApplicationCommandOptionType.String, "what author to search within", false);
            builder.AddOption(CategoryArgName, ApplicationCommandOptionType.String, "what Category to search within", false); // maybe this should be converted to a multi choice
            await BotMngr.client.CreateGlobalApplicationCommandAsync(builder.Build());
        }

        static async Task SlashCommandHandler(SocketSlashCommand command)
        {
            if (command.CommandName != CmdName) return;

            //get arguments //will probably swap to using discord.net's attributes after reading there docs
            string SearchTerm = (string)command.Data.Options.FirstOrDefault((o) => o.Name == SearchTermArgName)?.Value;
            string AuthorName = (string)command.Data.Options.FirstOrDefault((o) => o.Name == AuthorArgName)?.Value;
            string CategoryName = (string)command.Data.Options.FirstOrDefault((o) => o.Name == CategoryArgName)?.Value;

            //Query Manifest
            var mods = manifestMngr.Manifest.searchMods(SearchTerm, AuthorName, CategoryName);

            //build arguments string
            string text = "";
            if (SearchTerm != null) text += "SearchTerm: " + SearchTerm + " ";
            if (AuthorName != null) text += "AuthorName: " + AuthorName + " ";
            if (CategoryName != null) text += "CategoryName: " + CategoryName + " ";

            if (text == "") text = "no arguments";


            if (mods.Length == 0)
            {
                //inform user there were no results
                await command.RespondAsync("no mods found in manifest for: " + text);
                return;
            }

            SelectMenuBuilder builder = new();
            builder.WithPlaceholder("select mod to view more info about");
            builder.WithCustomId("modResults"); // discord requires an id

            //msgs to show users when one of discord's limits has been met
            string postfix = "";
            if (mods.Length > 25) postfix = Environment.NewLine + "only showing first 25 results in dropdown";

            string overLen = Environment.NewLine + "result to long to show all names";
            bool isOverLen = false;

            int i = 0;
            foreach (var mod in mods)
            {
                i++;
                
                //create msg content
                string add = Environment.NewLine + mod.name;
                //ensure staying within discord's limits while leaving room to inform the user that a limit has been met
                if (!isOverLen && text.Length + add.Length + postfix.Length < (mods.Length == i + 1 ? 2000 : 2000 - (overLen.Length + postfix.Length)))
                    text += add;
                else
                    isOverLen = true;
                
                //create dropdown
                if (i < 25) builder.AddOption(mod.name.LenCap(100), mod.id, mod.description.LenCap(100));
            }

            //add limit passed warning msgs if needed
            if (isOverLen) text = text + overLen;
            text += postfix;

            await command.RespondAsync(text, components: new ComponentBuilder().WithSelectMenu(builder).Build());
        }

        static async Task Client_SelectMenuExecuted(SocketMessageComponent arg)
        {
            arg.RespondAsync(arg.Data.Values.First());
        }
    }
}