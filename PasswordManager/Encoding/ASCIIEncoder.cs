namespace PasswordManager.Encoding
{
	public class ASCIIEncoder : Encoder
	{
		public override byte[] Decode(string encoded)
		{
			return System.Text.Encoding.ASCII.GetBytes(encoded);
		}

		public override string Encode(byte[] data, int offset, int count)
		{
			return System.Text.Encoding.ASCII.GetString(data, offset, count);
		}
	}
}
