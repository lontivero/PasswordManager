using System.Security.Cryptography;
using PasswordManager.Encoding;
using PasswordManager.Utils;

namespace PasswordManager.Crypto
{
	public class EncryptedKey : Key
	{
		private readonly string _password;

		public EncryptedKey(string password)
			: this(Create(), password)
		{
		}

		public EncryptedKey(byte[] encryptedKey, string password)
			: this(new Key(Decrypt(encryptedKey, Derive(password))), password)
		{
		}

		public EncryptedKey(Key key, string password)
			: base(key.ToByteArray())
		{
			_password = password;
		}

		internal static byte[] Derive(string password)
		{
			using(var der = new Rfc2898DeriveBytes(password, GetSalt(password)))
			{
				return der.GetBytes(64);
			}
		}

		private static byte[] GetSalt(string str)
		{
			return Hashes.SHA256(Encoders.ASCII.Decode(str));
		}

		private static Aes CreateAES256()
		{
			var aes = Aes.Create();
			aes.KeySize = 256;
			aes.Mode = CipherMode.ECB;
			aes.IV = new byte[16];
			return aes;
		}

		private static byte[] Encrypt(byte[] key, byte[] derived)
		{
			var keyhalf1 = key.SafeSubarray(0, 16);
			var keyhalf2 = key.SafeSubarray(16, 16);
			return Encrypt(keyhalf1, keyhalf2, derived);
		}

		private static byte[] Encrypt(byte[] keyhalf1, byte[] keyhalf2, byte[] derived)
		{
			var derivedhalf1 = derived.SafeSubarray(0, 32);
			var derivedhalf2 = derived.SafeSubarray(32, 32);

			var encryptedhalf1 = new byte[16];
			var encryptedhalf2 = new byte[16];
			var aes = CreateAES256();
			aes.Key = derivedhalf2;
			var encrypt = aes.CreateEncryptor();

			for (var i = 0; i < 16; i++)
			{
				derivedhalf1[i] = (byte)(keyhalf1[i] ^ derivedhalf1[i]);
			}
			encrypt.TransformBlock(derivedhalf1, 0, 16, encryptedhalf1, 0);
			for (var i = 0; i < 16; i++)
			{
				derivedhalf1[16 + i] = (byte)(keyhalf2[i] ^ derivedhalf1[16 + i]);
			}
			encrypt.TransformBlock(derivedhalf1, 16, 16, encryptedhalf2, 0);
			return encryptedhalf1.Concat(encryptedhalf2);
		}

		internal static byte[] Decrypt(byte[] encrypted, byte[] derived)
		{
			var derivedhalf1 = derived.SafeSubarray(0, 32);
			var derivedhalf2 = derived.SafeSubarray(32, 32);

			var encryptedHalf1 = encrypted.SafeSubarray(0, 16);
			var encryptedHalf2 = encrypted.SafeSubarray(16, 16);

			var key1 = new byte[16];
			var key2 = new byte[16];

			var aes = CreateAES256();
			aes.Key = derivedhalf2;
			var decrypt = aes.CreateDecryptor();
			decrypt.TransformBlock(encryptedHalf1, 0, 16, key1, 0);
			decrypt.TransformBlock(encryptedHalf1, 0, 16, key1, 0);

			for (var i = 0; i < 16; i++)
			{
				key1[i] ^= derivedhalf1[i];
			}
			decrypt.TransformBlock(encryptedHalf2, 0, 16, key2, 0);
			decrypt.TransformBlock(encryptedHalf2, 0, 16, key2, 0);

			for (var i = 0; i < 16; i++)
			{
				key2[i] ^= derivedhalf1[16 + i];
			}

			return key1.Concat(key2);
		}

		public override byte[] ToByteArray()
		{
			return Encrypt(base.ToByteArray(), Derive(_password));
		}

	    public Key UnencryptedKey
	    {
	        get
	        {
	            return new Key(base.ToByteArray());
	        }
	    }
	}
}
