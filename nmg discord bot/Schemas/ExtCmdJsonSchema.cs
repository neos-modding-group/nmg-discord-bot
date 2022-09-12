namespace nmgBot.Schemas
{
	internal class jsonFile
	{
		public CmdResp[] Responses;
	}
	internal class CmdResp
	{
		public string Response;
		public SlashCmd[] SlashCmds;
		public string[] MsgCmds;
	}
	internal class SlashCmd
	{
		public string name;
		public string desc;
	}
}
