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
			BotMngr.client.ButtonExecuted += Client_ButtonExecuted;
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
			string args = "";
			if (SearchTerm != null) args += "SearchTerm: " + SearchTerm + " ";
			if (AuthorName != null) args += "AuthorName: " + AuthorName + " ";
			if (CategoryName != null) args += "CategoryName: " + CategoryName + " ";

			if (args == "") args = "no arguments";

			switch (mods.Length)
			{
				case 0:
					//inform user there were no results
					await command.RespondAsync("no mods found in manifest for: " + args);
					return;
				case 1:
					await modResult(new idNversion(mods[0]), command, args + Environment.NewLine + "only a single result found. showing single result:");
					return;
			}

			SelectMenuBuilder builder = new();
			string Postfix = ModDropDown(builder, "modResults", mods, "select mod to view more info about") ? Environment.NewLine + "only showing first 25 results in dropdown" : "";

			string text = args + LimitedModList(mods.Select((m) => m.name).ToArray(), "result to long to show all names", Postfix.Length + args.Length) + Postfix;

			await command.RespondAsync(text, components: new ComponentBuilder().WithSelectMenu(builder).Build());
		}

		static async Task Client_SelectMenuExecuted(SocketMessageComponent Component)
		{
			switch (Component.Data.CustomId)
			{
				case "modResults":
					modResults(Component);
					break;
				case "modVersion":
					modVersions(Component);
					break;
				case "modVersionArtifact":
					modVersionArtifacts(Component);
					break;
				case "versionArtifact":
					break;
			}
		}

		private static async Task Client_ButtonExecuted(SocketMessageComponent Component)
		{
			int sep = Component.Data.CustomId.IndexOf(": ") + 2;
			string arg = Component.Data.CustomId.Substring(sep);
			string type = Component.Data.CustomId.Substring(0, sep);
			switch (type)
			{
				case "ViewVersions: ":
					idNversion idver = new(arg);
					if (await tryGetMod(Component, idver)) return;
					SelectMenuBuilder builder = new();
					//todo: add handeling for mods with over 24 versions
					ModDropDown(builder, "modVersion", idver.ModInfo.versions, (v) => v.Key, (v) => idver.id + "-" + v.Key, (v) => v.Value.changelog, "select version to view more info about", 1);
					builder.AddOption("LatestVersion", idver.ModInfo.id + "-latest", "show info about the latest version");
					await Component.RespondAsync("", components: new ComponentBuilder().WithSelectMenu(builder).Build());
					break;
				case "View: ":
					idNversion idverz = new(arg, true); // werid compiler naming bs workaround
					if (await tryGetmodAndVersion(Component, idverz)) return;
					await modVersion(idverz, Component);
					break;
				case "Download: ":
					break;
				case "Download+Dependencies: ":
					break;
				case "ViewDependencies: ":
					break;
				case "ViewArtifacts: ":
					idNversion idverzz = new(arg, true);
					if (await tryGetmodAndVersion(Component, idverzz)) return;
					await modVersionArtifact(idverzz, Component);
					break;
				case "ViewConflicts: ":
					break;
			}
		}

		static async Task modResults(SocketMessageComponent Component)
		{
			idNversion idver = new(Component);
			if (await tryGetMod(Component, idver)) return;
			await modResult(idver, Component);
		}

		static async Task modResult(idNversion idver, IDiscordInteraction interaction, string msgText = "")
		{
			EmbedBuilder builder = new();
			//add all the information that always exists
			builder.WithTitle(idver.ModInfo.name)
				.WithDescription(idver.ModInfo.description).WithDefults(idver.value);


			if (idver.ModInfo.authors.Count() == 1)
			{
				var author = idver.ModInfo.authors.First();
				builder.WithAuthor(author.Key, author.Value.iconUrl, author.Value.url);
			}
			else
			{
				string authorsStr = "";
				foreach (var author in idver.ModInfo.authors) authorsStr += $"[{author.Key.Replace(" ", "-")}]({author.Value.url}) ";
				builder.AddField("authors", authorsStr);
			}

			if (idver.ModInfo.website != null) builder.WithUrl(idver.ModInfo.website); else if (idver.ModInfo.sourceLocation != null) builder.WithUrl(idver.ModInfo.sourceLocation);
			if (idver.ModInfo.website != null && idver.ModInfo.sourceLocation != null && idver.ModInfo.website != idver.ModInfo.sourceLocation) builder.AddField("SourceLocation", idver.ModInfo.sourceLocation, inline: true);

			if (idver.ModInfo.tags != null && idver.ModInfo.tags.Length > 0) builder.AddField("Tags", idver.ModInfo.tags.ConcatStrs("," + Environment.NewLine));
			if (idver.ModInfo.flags != null && idver.ModInfo.flags.Length > 0) builder.AddField("Flags", idver.ModInfo.flags.ConcatStrs("," + Environment.NewLine));
			if (idver.ModInfo.latestVersion.HasValue) builder.AddField("LatestVersion", idver.ModInfo.latestVersion.Value.Key);


			builder.WithColor(GetColorFromFlags(idver.ModInfo.allFlags, idver.ModInfo?.flags, idver.ModInfo?.latestVersion?.Value.flagList));

			//need to add handeling for when there is no mod versions
			ComponentBuilder buttonBuilder = new();
			if (idver.ModInfo?.versions != null && idver.ModInfo?.versions.Count > 0)
			{
				if (idver.ModInfo.latestVersion?.Value.artifacts?.Length > 0)
				{
					if (idver.ModInfo.latestVersion?.Value?.dependencies != null && idver.ModInfo.latestVersion?.Value?.dependencies?.Count > 0) buttonBuilder.WithButton("Download Latest with Dependencies", $"Download+Dependencies: {idver.id}-latest");
					buttonBuilder.WithButton("Download Latest", $"Download: {idver.id}-latest");
				}
				buttonBuilder.WithButton("View Latest Version", $"View: {idver.id}-latest");
				buttonBuilder.WithButton("View Versions", $"ViewVersions: {idver.id}");
			}
			await interaction.RespondAsync(msgText, embed: builder.Build(), components: buttonBuilder.Build());
		}
		static async Task modVersions(SocketMessageComponent Component, string msgText = "")
		{
			var idver = GetIdnVersion(Component);
			if (await tryGetmodAndVersion(Component, idver)) return;
			await modVersion(idver, Component, msgText);
		}

		static async Task modVersion(idNversion idver, SocketMessageComponent Component, string msgText = "")
		{

			EmbedBuilder builder = new();

			builder.WithTitle($"[{idver.ModInfo.name}/{idver.version}]")
				.WithDescription(idver.ModVer.changelog ?? idver.ModInfo.description)
				.WithDefults(idver.value);

			if (idver.ModVer.releaseUrl != null) builder.WithUrl(idver.ModVer.releaseUrl);

			if (!string.IsNullOrEmpty(idver.ModVer.neosVersionCompatibility)) builder.AddField("neosVersionCompatibility", idver.ModVer.neosVersionCompatibility);
			if (idver.ModVer.modloaderVersionCompatibility != null) builder.AddField("modloaderVersionCompatibility", idver.ModVer.modloaderVersionCompatibility);

			if (idver.ModVer.flagList != null) GetColorFromFlags(idver.ModVer.flagList, null, null); else if (idver.ModInfo.flags != null) GetColorFromFlags(idver.ModInfo.flags, null, null);
			if (idver.ModVer.flagList != null && idver.ModInfo.flags.Length > 0) builder.AddField("Flags", idver.ModVer.flagList.ConcatStrs("," + Environment.NewLine));

			//implement buttons to: downloade version, download version with depencacies, view artifacts, view depencacyes, view conflicts, 
			ComponentBuilder buttonBuilder = new();
			if (idver.ModVer.artifacts?.Count() > 0)
			{
				if (idver.ModVer?.dependencies != null && idver.ModVer?.dependencies?.Count > 0) buttonBuilder.WithButton("Download with Dependencies", $"Download+Dependencies: {idver.value}");
				buttonBuilder.WithButton("Download", $"Download: {idver.value}");
				buttonBuilder.WithButton("View Artifacts", $"ViewArtifacts: {idver.value}");
			}
			if (idver.ModVer?.dependencies?.Count > 0) buttonBuilder.WithButton("View Dependencies", $"ViewDependencies: {idver.value}");
			if (idver.ModVer?.conflicts?.Count > 0) buttonBuilder.WithButton("View Conflicts", $"ViewConflicts: {idver.value}");
			await Component.RespondAsync(msgText, embed: builder.Build(), components: buttonBuilder.Build());
		}

		static async Task modVersionArtifacts(SocketMessageComponent Component, string msgText = "") => await modVersionArtifact(GetIdnVersion(Component), Component, msgText);

		static async Task modVersionArtifact(idNversion idver, SocketMessageComponent Component, string msgText = "")
		{
			SelectMenuBuilder builder = new();
			ModDropDown(builder, "versionArtifact", idver.ModVer.artifacts, (v) => v.filename ?? v.url.GetLastUrlSection(), (v) => v.sha256, (v) => v.installLocation ?? "/nml_mods");
			await Component.RespondAsync(msgText, components: new ComponentBuilder().WithSelectMenu(builder).Build());
		}

		static bool ModDropDown(SelectMenuBuilder builder, string customId, ModInfo[] mods, string Placeholder = "", int reservedElements = 0) => ModDropDown(builder, customId, mods, (m) => "", Placeholder, reservedElements); // this is some stupid bs, for some reason doing something like idSlug == null? "" : idSlug(m) throws null ref 
		static bool ModDropDown(SelectMenuBuilder builder, string customId, ModInfo[] mods, Func<ModInfo, string> idSlug, string Placeholder = "", int reservedElements = 0) => ModDropDown(builder, customId, mods, (m) => m.name, (m) => m.id + idSlug(m), (m) => m.description, Placeholder, reservedElements);
		static bool ModDropDown<T>(SelectMenuBuilder builder, string customId, IEnumerable<T> mods, Func<T, string> name, Func<T, string> val, Func<T, string> desc = null, string Placeholder = "", int reservedElements = 0)
		{
			if(!string.IsNullOrEmpty(Placeholder))builder.WithPlaceholder(Placeholder);
			builder.WithCustomId(customId); // discord requires an id
			int max = 25 - reservedElements;

			int i = 0;

			foreach (var mod in mods)
			{
				i++;
				//create dropdown
				if (i < max) builder.AddOption(name(mod).LenCap(100), val(mod), desc(mod).LenCap(100)); else break;
			}
			return mods.Count() >= max;
		}
		static async Task<bool> tryGetmodAndVersion(SocketMessageComponent component, idNversion idver)
		{
			if (await tryGetMod(component, idver)) return true;

			if (idver.version == "latest")
			{
				KeyValuePair<string, ModVersion>? latest = idver.ModInfo?.latestVersion;
				idver.ModVer = latest?.Value;
				idver.version = latest?.Key;
			}
			else idver.ModInfo.versions.TryGetValue(idver.version, out idver.ModVer);
			if (idver.ModVer == null)
			{
				await component.RespondAsync($"no mod version found for: {idver.id}{Environment.NewLine}it may have been removed from the manifest. removals dont happen unless the mod has been fully deleted.");
				return true;
			}
			return false;
		}
		static async Task<bool> tryGetMod(SocketMessageComponent component, idNversion idver)
		{
			//get mod from local manifest
			manifestMngr.Manifest.mods.TryGetValue(idver.id, out idver.ModInfo);
			//inform user if mod is not in the manifest, discord requires a response so may aswell make it useful 
			if (idver.ModInfo == null)
			{
				await component.RespondAsync($"no mod found for: {idver.id}{Environment.NewLine}it may have been removed from the manifest. removals dont happen unless the mod has been fully deleted.");
				return true;
			}
			return false;
		}

		static string LimitedModList(string[] mods, string overLen, int charsUnderLimit)
		{
			overLen = Environment.NewLine + overLen;
			bool isOverLen = false;

			string text = "";

			int cap = 2000 - (overLen.Length + charsUnderLimit);//define this so its not calulated every loop interation
			int i = 0;
			foreach (var mod in mods)
			{
				i++;
				//create msg content
				string add = Environment.NewLine + mod;
				//ensure staying within discord's limits while leaving room to inform the user that a limit has been met
				if (text.Length + add.Length + charsUnderLimit < (mods.Length == i + 1 ? 2000 : cap))
					text += add;
				else
				{
					isOverLen = true;
					break;
				}
			}

			//postfix limit passed warning msgs if needed
			if (isOverLen) text = text + overLen;

			return text;
		}

		private static Color GetColorFromFlags(string[] Allflags, string[]? direct, string[]? latest)
		{
			foreach (var flag in Allflags)
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
			}
			float r = 1f;
			float g = 1f;
			float b = 1f;

			bool isPrerelease = false;
			bool isDeprecated = false;


			List<string> ModernFlags = new();
			if (direct != null) ModernFlags.AddRange(direct);
			if (latest != null) ModernFlags.AddRange(latest);
			foreach (var flag in ModernFlags)
			{
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

		static idNversion GetIdnVersion(SocketMessageComponent component) => new(component, true);

		class idNversion
		{
			public string value;
			public string id;
			public string version;
			public ModInfo ModInfo;
			public ModVersion ModVer;

			public idNversion() { }
			public idNversion(ModInfo mod)
			{
				setId(mod.id);
				ModInfo = mod;
			}

			public idNversion(string id, bool Dashed = false) => parceDashed(id, Dashed);

			public idNversion(SocketMessageComponent component, bool Dashed = false) => parceDashed(component.Data.Values.First(), Dashed);

			private void parceDashed(string val, bool Dashed)
			{
				value = val;
				if (Dashed)
				{
					version = value.Split("-").Last();
					id = value.Substring(0, value.Length - version.Length - 1);
				}
				else
					id = value;
			}

			private void setId(string id)
			{
				this.id = id;
				value = id;
			}
		}
	}
}