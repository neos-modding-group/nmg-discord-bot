using Discord;
using Discord.WebSocket;
using nmgBot.Managers;
using nmgBot.Schemas;

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

			//get arguments //will probably swap to using discord.net's attributes after reading their docs
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

			//postfix limit passed warning msgs if needed
			if (isOverLen) text = text + overLen;
			text += postfix;//this one is second so its closer to the dropdown

			await command.RespondAsync(text, components: new ComponentBuilder().WithSelectMenu(builder).Build());
		}

		static async Task Client_SelectMenuExecuted(SocketMessageComponent Component)
		{
			string modid = Component.Data.Values.First(); //get value. for some reason SocketMessageComponentData.Value seems to always be blank

			//get mod from local manifest
			ModInfo mod;
			manifestMngr.Manifest.mods.TryGetValue(modid, out mod);
			//inform user if mod is not in the manifest, discord requires a response so may aswell make it useful 
			if (mod == null)
			{
				await Component.RespondAsync($"no mod found for: {modid}{Environment.NewLine}it may have been removed from the manifest. removals dont happen unless the mod has been fully deleted.");
				return;
			}
			EmbedBuilder builder = new();
			//add all the information that always exists
			builder.WithTitle(mod.name)
				.WithDescription(mod.description)
				.WithFooter(mod.id)
				.AddField("Category", mod.category)
				.WithColor(new Color(255, 255, 255));

			if (mod.authors.Count() == 1)
			{
				var author = mod.authors.First();
				builder.WithAuthor(author.Key, url: author.Value.url);
			}
			else
				foreach (var author in mod.authors) builder.AddField(author.Key, author.Value.url);

			if (mod.website != null) builder.WithUrl(mod.website); else if (mod.sourceLocation != null) builder.WithUrl(mod.sourceLocation);
			if (mod.website != null && mod.sourceLocation != null && mod.website != mod.sourceLocation) builder.AddField("SourceLocation", mod.sourceLocation);
			if (mod.tags != null && mod.tags.Length > 0) builder.AddField("Tags", mod.tags.ConcatStrs("," + Environment.NewLine));
			if (mod.flags != null && mod.flags.Length > 0)
			{
				builder.AddField("Flags", mod.flags.ConcatStrs("," + Environment.NewLine));
				builder.WithColor(GetColorFromFlags(mod.flags));
			}

			await Component.RespondAsync("", embed: builder.Build());
		}

		private static Color GetColorFromFlags(string[] flags)
		{
			float r = 1f;
			float g = 1f;
			float b = 1f;

			bool isPrerelease = false;
			bool isDeprecated = false;

			foreach (var flag in flags)
			{
				if (flag.StartsWith("vulnerability:"))
				{
					return flag switch
					{
						"vulnerability:low" => Color.DarkOrange,
						"vulnerability:medium" => Color.Orange,
						"vulnerability:high" => Color.Red,
						"vulnerability:critical" => Color.DarkRed,
						_ => Color.Red,
					};
				}

				if (flag == "broken:") //everything from here down is just bs coloring
				{
					switch (flag)
					{
						case "broken:linux-native":
							r = .25f;
							break;
						case "broken:linux-wine":
							g = .25f;
							break;
						case "broken:windows":
							b = .25f;
							break;
					}
				}
				if (flag == "prerelease") isPrerelease = true;
				if (flag == "deprecated") isDeprecated = true;
			}

			if (isPrerelease)
			{
				r *= .7f;
				g *= .7f;
				b *= .7f;
			}

			if (isDeprecated)
			{
				r *= .4f;
				g *= .4f;
				b *= .4f;
			}

			return new(r, g, b);
		}
	}
}