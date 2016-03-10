using System;
using System.Numerics;
using PasswordManager.Crypto.Extensions;
using PasswordManager.Encoding;
using PasswordManager.Utils;

namespace PasswordManager.Crypto
{
	public class Key
	{
		private readonly byte[] _key;
		private readonly bool _isCompressed;
		private PublicKey _pubKey;

		public static Key Create()
		{
			var prng = new WinCryptoPrng();
			var rnd = prng.Next();
			var key = Hashes.SHA256(rnd);
			return new Key(key);
		}

		public static Key Create(string entropy)
		{
			var prng = new WinCryptoPrng();
			var rnd = prng.Next();
			var data = Encoders.ASCII.Decode(entropy);
			var key = Hashes.HMACSHA256(rnd, data);
			return new Key(key);
		}

		public Key(byte[] key)
			: this(key, true)
		{}

		public Key(byte[] key, bool compressed)
		{
			if(key.Length != 32)
				throw new ArgumentException("private key must be a 32 bytes length array", "key");
			_isCompressed = compressed;
			CheckValidKey(key);

			_key = key;
		}

		internal static void CheckValidKey(byte[] key)
		{
			var candidateKey = key.ToBigIntegerUnsigned(false);
			if (candidateKey <= 0 || candidateKey >= Secp256k1.N)
				throw new ArgumentException("Invalid key");
		}

		public bool IsCompressed
		{
			get { return _isCompressed; }
		}

		public PublicKey PublicKey
		{
			get { return _pubKey ?? (_pubKey = new PublicKey(PublicPoint, IsCompressed)); }
		}

		internal ECPoint PublicPoint
		{
			get { return K * Secp256k1.G; }
		}

		internal BigInteger K
		{
			get { return _key.ToBigIntegerUnsigned(true); }
		}

		public ECDSASignature Sign(byte[] input)
		{
			var signer = new ECDsaSigner(this);
			return signer.GenerateSignature(input);
		}

		public virtual byte[] ToByteArray()
		{
			return _key.SafeSubarray(0);
		}

		public override string ToString()
		{
			return Encoders.Hex.Encode(Packer.BigEndian.GetBytes(ToByteArray()));
		}
	}
}
