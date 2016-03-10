using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PasswordManager.Crypto;
using PasswordManager.Encoding;

namespace PasswordManager
{
	class Account
	{
		public string AccountName { get; set; }
		public DateTimeOffset Expires { get; set; }
		public int Length { get; set; }
		public byte[] HashCode { get; set; }
		public ECDSASignature Signature { get; set; }

		public string ToString(Key key)
		{
			var str = ToString();
			var sb = new StringBuilder(str);

			var ecdsa = new ECDsaSigner(key);
			var signature = ecdsa.GenerateSignature(Encoders.ASCII.Decode(str));
			sb.AppendLine("Signature  : " + Encoders.Hex.Encode(signature.ToDER()));
			return sb.ToString();
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.AppendLine("AccountName: " + AccountName);
			sb.AppendLine("Expires    : " + Expires.ToUniversalTime());
			sb.AppendLine("Length     : " + Length);
			sb.AppendLine("HashCode   : " + Encoders.Hex.Encode(Hashes.SHA256(Encoders.ASCII.Decode(AccountName))));
			return sb.ToString();
		}

		internal static Account Parse(string content)
		{
			var dictionary = new Dictionary<string, string>();

			var lines = content.Split(new[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);
			foreach (var line in lines)
			{
				var ios = line.IndexOf(':');
				var key = line.Substring(0, ios).TrimEnd(' ');
				var value = line.Substring(ios+2).TrimEnd('\r');
				dictionary[key] = value;
			}

			return new Account {
				AccountName = dictionary["AccountName"], 
				Expires = DateTimeOffset.Parse(dictionary["Expires"]), 
				Length = int.Parse(dictionary["Length"]), 
				HashCode = Encoders.Hex.Decode(dictionary["HashCode"]), 
				Signature = ECDSASignature.FromDER(Encoders.Hex.Decode(dictionary["Signature"]))
			};
		}
	}
}
