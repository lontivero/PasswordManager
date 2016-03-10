using PasswordManager.Utils;

namespace PasswordManager.Crypto
{
	public class PublicKey
	{
		private readonly byte[] _key;
		private byte[] _pubKeyHash;

		public PublicKey(byte[] key)
		{
			_key = key;
		}

		public PublicKey(ECPoint point, bool isCompressed)
			: this(point.Encode(isCompressed))
		{
		}

		public byte[] Hash
		{
			get { return _pubKeyHash ?? (_pubKeyHash = Hashes.RIPEMD160(Hashes.SHA256(_key))); }
		}

		public byte[] ToByteArray()
		{
			return _key.SafeSubarray(0);
		}

		public ECPoint Point
		{
			get { return ECPoint.Decode(_key); }
		}

		public bool Verify(byte[] data, ECDSASignature signature)
		{
			return ECDsaSigner.VerifySignature(data, signature, this);
		}

		public override string ToString()
		{
			return Encoding.Encoders.Hex.Encode(ToByteArray());
		}
	}
}