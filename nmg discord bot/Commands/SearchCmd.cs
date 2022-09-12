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
    internal static class SearchCmd // later i will re-factor to inherit a custom cmd class and handel things at a higher level but for now this works.
    {
        internal static void SetUp()
        {
            BotMngr.client.Ready += DefineSlashCommands;
            BotMngr.client.SlashCommandExecuted += SlashCommandHandler;
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
            string SearchTerm = (string)command.Data.Options.FirstOrDefault((o) => o.Name == SearchTermArgName)?.Value;
            string AuthorName = (string)command.Data.Options.FirstOrDefault((o) => o.Name == AuthorArgName)?.Value;
            string CategoryName = (string)command.Data.Options.FirstOrDefault((o) => o.Name == CategoryArgName)?.Value;

            var mods = manifestMngr.Manifest.searchMods(SearchTerm, AuthorName, CategoryName);

            string test = "";
            foreach(var mod in mods)
            {
                test += Environment.NewLine;
                test += mod.name;
            }


            await command.RespondAsync(test);
        }
    }
}