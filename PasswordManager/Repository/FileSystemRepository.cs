using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using PasswordManager.Crypto;
using PasswordManager.Encoding;

namespace PasswordManager
{
	class FileSystemRepository
	{
		private readonly Func<string> _passwordProvider;
		private string _password;

		public FileSystemRepository(Func<string> passwordProvider)
		{
			_passwordProvider = passwordProvider;
		}

		private string Password
		{
			get
			{
				if(string.IsNullOrEmpty(_password))
				{
					_password = _passwordProvider();
				}
				return _password;
			}
		}

		public string RootFolder
		{
			get
			{
				var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.None);
				return Path.Combine(appdata, "Simple Password Manager");
			}
		}

		public string KeyFolder
		{
			get
			{
				return Path.Combine(RootFolder, ".spmkey");
			}
		}

		public Key Key
		{
			get
			{
				var encryptedKeyBytes = File.ReadAllBytes(Path.Combine(KeyFolder, "repository.key"));
				return new EncryptedKey(encryptedKeyBytes, Password);
			}
		}

		public void Create()
		{
			Directory.CreateDirectory(RootFolder);
			Directory.CreateDirectory(KeyFolder);

			var encryptedKey = new EncryptedKey(Password);
			File.WriteAllBytes(Path.Combine(KeyFolder, "repository.key"), encryptedKey.ToByteArray());
		}

		public void AddAccount(string accountName)
		{
			var account = new Account();
			account.AccountName = accountName;
			account.Expires = DateTimeOffset.UtcNow.AddDays(45);
			account.Length = 20;

			File.WriteAllText(Path.Combine(RootFolder, SanityFileName(accountName)), account.ToString(Key));
		}

		public IEnumerable<Account> GetAccountsBy(Func<Account, bool> predicate)
		{
			var signer = new ECDsaSigner(Key);
			foreach (var file in Directory.EnumerateFiles(RootFolder, "*", SearchOption.TopDirectoryOnly))
			{
				var content = File.ReadAllText(file);
				var account = Account.Parse(content);

				if(predicate(account) && signer.VerifySignature(Encoders.ASCII.Decode(account.ToString()), account.Signature))
					yield return account;
			}
		}

		public string GetPasswordFor(string accountName)
		{
			var account = GetAccountsBy(x => x.AccountName.Contains(accountName)).First();
			var derived = Pbkdf2.ComputeDerivedKey(new HMACSHA256(Key.ToByteArray()), account.HashCode, 1000, 32);
			return Encoders.Base58.Encode(derived).Substring(0, account.Length);
		}

		private static string SanityFileName(string name)
		{
			var invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
			var invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

			return Regex.Replace(name, invalidRegStr, "_");
		}
	}
}
