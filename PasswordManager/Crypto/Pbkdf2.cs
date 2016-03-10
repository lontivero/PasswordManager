using System;
using System.IO;
using System.Security.Cryptography;
using PasswordManager.Utils;

namespace PasswordManager.Crypto
{
	public class Pbkdf2 : Stream
	{
		private readonly byte[] _saltBuffer;
		private readonly byte[] _digest;
		private readonly byte[] _digestT1;
		private readonly KeyedHashAlgorithm _hmacAlgorithm;
		private readonly int _iterations;
		private long _blockStart, _blockEnd, _pos;

		public static byte[] ComputeDerivedKey(KeyedHashAlgorithm hmacAlgorithm,
			byte[] salt, int iterations, int derivedKeyLength)
		{
			if (derivedKeyLength < 0 || derivedKeyLength > int.MaxValue)
				throw new ArgumentOutOfRangeException("derivedKeyLength");

			using (var kdf = new Pbkdf2(hmacAlgorithm, salt, iterations))
			{
				return kdf.Read(derivedKeyLength);
			}
		}

		public Pbkdf2(KeyedHashAlgorithm hmacAlgorithm, byte[] salt, int iterations)
		{
			if (hmacAlgorithm == null)
				throw new ArgumentNullException("hmacAlgorithm");
			if (salt == null)
				throw new ArgumentNullException("salt");
			if (salt.Length < 0 || salt.Length > int.MaxValue - 4)
				throw new ArgumentOutOfRangeException("salt");
			if (iterations < 1 || iterations > int.MaxValue)
				throw new ArgumentOutOfRangeException("iterations");
			if (hmacAlgorithm.HashSize == 0 || hmacAlgorithm.HashSize % 8 != 0)
				throw new ArgumentException("hmacAlgorithm.HashSize");

			var hmacLength = hmacAlgorithm.HashSize / 8;
			_saltBuffer = new byte[salt.Length + 4]; 
			Array.Copy(salt, _saltBuffer, salt.Length);
			_iterations = iterations; _hmacAlgorithm = hmacAlgorithm;
			_digest = new byte[hmacLength]; _digestT1 = new byte[hmacLength];
		}


		private void ComputeBlock(long pos)
		{
			var pos2 = Packer.BigEndian.GetBytes((int) pos);
			Array.Copy(pos2, 0, _saltBuffer, _saltBuffer.Length-4, 4);
			ComputeHmac(_saltBuffer, _digestT1);
			Array.Copy(_digestT1, _digest, _digestT1.Length);

			for (var i = 1; i < _iterations; i++)
			{
				//_digestT1 = Hashes.HMACSHA256(_saltBuffer, _digestT1);
				ComputeHmac(_digestT1, _digestT1);
				for (var j = 0; j < _digest.Length; j++)
				{
					_digest[j] ^= _digestT1[j];
				}
			}
		}

		private void ComputeHmac(byte[] input, byte[] output)
		{
			_hmacAlgorithm.Initialize();
			_hmacAlgorithm.TransformBlock(input, 0, input.Length, input, 0);
			_hmacAlgorithm.TransformFinalBlock(new byte[0], 0, 0);
			Array.Copy(_hmacAlgorithm.Hash, output, output.Length);
		}

		public override void Flush()
		{
		}


		public byte[] Read(int count)
		{
			if (count < 0 || count > int.MaxValue)
				throw new ArgumentOutOfRangeException("count");

			var buffer = new byte[count];
			var bytes = Read(buffer, 0, count);
			if (bytes < count)
				throw new ArgumentException("count", "Can only return " + bytes + " bytes.");

			return buffer;
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			if(buffer == null) 
				throw new ArgumentNullException("buffer");
			if (offset < 0 || offset > int.MaxValue )
				throw new ArgumentOutOfRangeException("offset");
			if (count < 0  || count > int.MaxValue)
				throw new ArgumentOutOfRangeException("count");
			if (offset + count > buffer.Length)
				throw new ArgumentException("offset + count is out of bound");

			var bytes = 0;

			while (count > 0)
			{
				if (Position < _blockStart || Position >= _blockEnd)
				{
					if (Position >= Length)
					{
						break;
					}

					var pos = Position / _digest.Length;
					ComputeBlock(pos + 1);
					_blockStart = pos * _digest.Length;
					_blockEnd = _blockStart + _digest.Length;
				}

				var bytesSoFar = (int)(Position - _blockStart);
				var bytesThisTime = Math.Min(_digest.Length - bytesSoFar, count);
				Array.Copy(_digest, bytesSoFar, buffer, bytes, bytesThisTime);
				count -= bytesThisTime;
				bytes += bytesThisTime;
				Position += bytesThisTime;
			}

			return bytes;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			long pos;

			switch (origin)
			{
				case SeekOrigin.Begin:
					pos = offset;
					break;
				case SeekOrigin.Current:
					pos = Position + offset;
					break;
				case SeekOrigin.End:
					pos = Length + offset;
					break;
				default:
					throw new ArgumentOutOfRangeException("origin", "Unknown seek type.");
			}

			if (pos < 0)
				throw new ArgumentException("Can't seek before the stream start.", "offset");

			Position = pos;
			return pos;
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException();
		}

		public override bool CanRead
		{
			get
			{
				return true;
			}
		}

		public override bool CanSeek
		{
			get
			{
				return true;
			}
		}

		public override bool CanWrite
		{
			get
			{
				return false;
			}
		}

		public override long Length
		{
			get
			{
				return _digest.Length * uint.MaxValue;
			}
		}

		public override long Position
		{
			get
			{
				return _pos;
			}
			set
			{
				if (_pos < 0)
				{
					throw new ArgumentException("Can't seek before the stream start.");
				}
				_pos = value;
			}
		}
	}
}
