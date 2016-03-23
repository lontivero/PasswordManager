using System;
using System.Linq;
using System.Windows.Forms;
using Mono.Options;
using PasswordManager.Crypto;
using PasswordManager.Encoding;

namespace PasswordManager
{
	enum CommandAction
	{
		None,
		AddAccount,
		ListAccount,
		ViewAccount,
		ShowHelp,
		Initialize
	}

	class Program
	{
		private static readonly CmdOptions options = new CmdOptions();
		private static FileSystemRepository _repo;

		[STAThread]
		static void Main(string[] args)
		{
			try
			{
				var extra = options.Parse(args);
				if(extra.Any())
					options.Account = extra.First();

				if(options.Action == CommandAction.ShowHelp)
				{
					ShowHelp(options);
					return;
				}

				_repo = new FileSystemRepository(PromptPassword);

				switch (options.Action)
				{
					case CommandAction.Initialize:
						Initialize();
						break;
					case CommandAction.None:
						if (string.IsNullOrEmpty(options.Account))
						{
							ListCredentials();
						}
						else
						{
							ViewCredential(options.Account);
						}
						break;
					case CommandAction.ListAccount:
						ListCredentials();
						break;
					case CommandAction.ViewAccount:
						if (options.Account != null)
							ViewCredential(options.Account);
						else
							ShowHelp(options);
						break;
					case CommandAction.AddAccount:
						AddCredential(options.Account);
						ViewCredential(options.Account);
						break;
				}
			}
			catch (OptionException e)
			{
				Console.Write("pwd: ");
				Console.WriteLine(e.Message);
				Console.WriteLine("Try `pwd --help' for more information.");
			}
		}

		private static void ListCredentials()
		{
			foreach (var acc in _repo.GetAccountsBy(_=>true))
			{
				Console.WriteLine("{0} {1} {2} {3} ", 
					Encoders.Hex.Encode(acc.HashCode).Substring(0, 6), 
					acc.Length,
					acc.Expires,
					acc.AccountName );
			}
		}

		private static void ViewCredential(string accountName)
		{
			try
			{
				var pwd = _repo.GetPasswordFor(accountName);
				if (options.Action == CommandAction.ViewAccount)
				{
					Console.WriteLine(pwd);
				}
				else
				{
					Clipboard.SetText(pwd);
				}
			}
			catch(InvalidOperationException)
			{
				//Console.BackgroundColor = ConsoleColor.Blue;
				//Console.ForegroundColor = ConsoleColor.Yellow;

				Console.WriteLine("account not found!");
				ListCredentials();
			}
		}

		private static void AddCredential(string accountName)
		{
			_repo.AddAccount(accountName);
		}

		private static void Initialize()
		{
			_repo.Create();
		}

		private static string PromptPassword()
		{
			string password;
			PasswordScore strength;
			do
			{
				Console.Write("Master Password: ");
				password = ReadPassword();
				strength = PasswordAdvisor.CheckStrength(password);
				if(strength < PasswordScore.Medium)
				{
					Console.WriteLine("Too weak password, try again.");
				}
			} while (strength < PasswordScore.Medium);
			return password;
		}

		private static string ReadPassword()
		{
			var password = string.Empty;
			ConsoleKeyInfo key;
			do
			{
				key = Console.ReadKey(true);

				if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
				{
					password += key.KeyChar;
				}
				else
				{
					if (key.Key == ConsoleKey.Backspace && password.Length > 0)
					{
						password = password.Substring(0, (password.Length - 1));
					}
				}
			} while (key.Key != ConsoleKey.Enter);
			Console.WriteLine("");
			return password;
		}

		private static void ShowHelp(CmdOptions options)
		{
			Console.WriteLine("Usage: spm ");
			Console.WriteLine("Simple Password Manager for Windows (c) 2016 - Lucas Ontivero <lucasontivero@gmail.com>.");
			Console.WriteLine();
			Console.WriteLine("Options:");
			options.WriteOptionDescriptions(Console.Out);
		}
	}
}
