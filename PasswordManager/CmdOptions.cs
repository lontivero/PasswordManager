using System.Collections.Generic;
using System.IO;
using Mono.Options;

namespace PasswordManager
{
	class CmdOptions
	{
		private readonly OptionSet _optionSet;
		public CommandAction Action { get; private set; }
		public string Account;

		public CmdOptions()
		{
			_optionSet = new OptionSet{
				{ "h|?|help",  "Shows this help and exit", v=>ShowHelp() },
				{ "c",         "Copy to clipboard", v=> CopyToClipboard() },
				{ "l|ls",      "Lists all the credentials in the repository", ListAccounts },
				{ "a|add=",    "Adds a new {CREDENTIAL} to the repository", AddAccount },
				{ "v|view=",   "Displays the {CREDENTIAL} details", ViewAccount },
				{ "init",      "Initializes the repository", _=> Initialize() },
			};
		}

		private void Initialize()
		{
			Action = CommandAction.Initialize;
		}

		public List<string> Parse(string[] args)
		{
			return _optionSet.Parse(args);
		}

		private void ShowHelp()
		{
			Action = CommandAction.ShowHelp;
		}

		private void AddAccount(string userAccount)
		{
			Action = CommandAction.AddAccount;
			Account=userAccount;
		}

		private void ViewAccount(string userAccount)
		{
			Action = CommandAction.ViewAccount;
			Account=userAccount;
		}

		private void ListAccounts(string s)
		{
			Action = CommandAction.ListAccount;
		}

		private void CopyToClipboard()
		{
			Action = CommandAction.ViewAccount;
		}

		public void WriteOptionDescriptions(TextWriter o)
		{
			_optionSet.WriteOptionDescriptions(o);
		}
	}
}