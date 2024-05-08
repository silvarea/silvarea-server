using System.Text;

namespace Silvarea.Network
{

	public class Packet : Stream
	{
		private static int[] BitMasks = new int[32];
		private BufferedStream _stream { get; set; }

		private BinaryReader _streamReader {  get; set; }

		private BinaryWriter _streamWriter {  get; set; }

		public int Opcode { get; set; }

		public Packet()
		{
			Opcode = -1;
			initPacket(new MemoryStream());
		}

		public Packet(int opcode, byte[] data)
		{
			Opcode = opcode;
			initPacket(new MemoryStream(data));
        }

		public Packet(byte[] data)
		{
			Opcode = -1;
			initPacket(new MemoryStream(data));
        }

		public Packet(int opcode, Stream data)
		{
			Opcode = opcode;
			initPacket(data);
		}

        public Packet(Stream data)
        {
            Opcode = -1;
            initPacket(data);
        }

		public Packet(int opcode)
		{
			Opcode = opcode;
			initPacket(new MemoryStream());
		}

		static Packet()
		{
			for (int i = 0; i < 32; i++)
				BitMasks[i] = ((1 <<  i) - 1);
		}

		private void initPacket(Stream data)
		{
			BitPosition = 0;
			_stream = new BufferedStream(data);
			if(_stream.CanRead)  _streamReader = new BinaryReader(_stream);
			if(_stream.CanWrite) _streamWriter = new BinaryWriter(_stream);
        }

		public override bool CanRead => _stream.CanRead;

		public override bool CanSeek => _stream.CanSeek;

		public override bool CanWrite => _stream.CanWrite;

		public override long Length => _stream.Length;

		public override long Position { get => _stream.Position; set => _stream.Position = value; }

		public int BitPosition { get; set; }

		public override void Flush()
		{
			_stream.Flush();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return _stream.Read(buffer, offset, count);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return _stream.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			_stream.SetLength(value);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			_stream.Write(buffer, offset, count);
		}

		public void openBitBuffer()
		{
			//_stream.SetLength(5);
			BitPosition = (int) (Position * 8);
		}

		public void closeBitBuffer()
		{
			Position = ((BitPosition + 7) / 8) >>> 0;
		}

		public void p8(long value)
		{
			_streamWriter.Write((sbyte) (value >> 56));
			_streamWriter.Write((sbyte) (value >> 48));
			_streamWriter.Write((sbyte) (value >> 40));
			_streamWriter.Write((sbyte) (value >> 32));
			_streamWriter.Write((sbyte) (value >> 24));
			_streamWriter.Write((sbyte) (value >> 16));
			_streamWriter.Write((sbyte) (value >> 8));
			_streamWriter.Write((sbyte) value);
        }

		public void pdata(byte[] data, int count)
		{
			_streamWriter.Write(data, 0, count);

		}

		public void pdata_alt1(byte[] data, int count)
		{
			Array.Reverse(data, count, data.Length);
			pdata(data, data.Length);
			//for(int i = count - 1; i >= 0; i--)
			//	p1(data[i]);
		}

		public void pjstr(string str)
		{
			_stream.Write(Encoding.UTF8.GetBytes(str));

		}

		public void p1(int value)
		{
			_streamWriter.Write((sbyte) value);
		}

		public void p1_alt3(int value)
		{
			_streamWriter.Write((sbyte) (value + 128));
		}

        public void p2(int value)
        {
            _streamWriter.Write((sbyte)(value >> 8));
            _streamWriter.Write((sbyte)value);
        }

        public void p2_alt1(int value)
		{
            _streamWriter.Write((sbyte)value);
            _streamWriter.Write((sbyte)(value >> 8));
        }

        public void p3(int value)
        {
            _streamWriter.Write((sbyte)(value >> 16));
            _streamWriter.Write((sbyte)(value >> 8));
            _streamWriter.Write((sbyte)value);
        }

        public void p4(int value)
        {
            _streamWriter.Write((sbyte)(value >> 24));
            _streamWriter.Write((sbyte)(value >> 16));
            _streamWriter.Write((sbyte)(value >> 8));
            _streamWriter.Write((sbyte)value);
        }

        public void p4_alt1(int value)
        {
            _streamWriter.Write((sbyte) value);
            _streamWriter.Write((sbyte) (value >> 8));
            _streamWriter.Write((sbyte) (value >> 16));
            _streamWriter.Write((sbyte) (value >> 24));
        }

		public void pBits(int n, int value)
		{
			int bytePosition = BitPosition >>> 3;
			int remaining = 8 - (BitPosition & 7);
			BitPosition += n;

			byte[] memStream = toByteArray(); // [];

			//using (var memoryStream = new MemoryStream())
			//{
			//	_stream.CopyTo(memoryStream);
			//	memStream = memoryStream.ToArray();
			//}

			if (bytePosition + 1 > memStream.Length)
			{
				if (memStream.Length < bytePosition + 1)
				{
					var temp = new MemoryStream(bytePosition + 1);
					temp.SetLength(bytePosition + 1);
					memStream = temp.ToArray();
				}
			}

			for (; n > remaining; remaining = 8)
			{
				memStream[bytePosition] &= (byte)(~BitMasks[remaining]);
				memStream[bytePosition++] |= (byte)((value >> (n - remaining)) & BitMasks[remaining]);
				n -= remaining;

				if (bytePosition + 1 > memStream.Length)
				{
					if (memStream.Length < bytePosition + 1)
					{
						var temp = new MemoryStream(bytePosition + 1);
						temp.SetLength(bytePosition + 1);
						memStream = temp.ToArray();
					}
				}
			}

			if (n == remaining)
			{
				memStream[bytePosition] &= (byte)(~BitMasks[remaining]);
				memStream[bytePosition] |= (byte)(value & BitMasks[remaining]);
			}
			else
			{
				memStream[bytePosition] &= (byte)(~BitMasks[remaining] << (remaining - n));
				memStream[bytePosition] |= (byte)((value & BitMasks[remaining]) << (remaining - n));
			}

			//var mem = new MemoryStream(memStream);
			//mem.CopyTo(_stream);
			_streamWriter.Write(memStream);
			//_stream.Write(memStream, 0, memStream.Length);
		}

		public void pBits2(int count, int value)
		{
			int bytePosition = BitPosition >> 3;
			int offset = 8 - (BitPosition & 7);
			BitPosition += count;

			if ((bytePosition + 1) > Length)
			{
				SetLength(bytePosition + 1);
			}

			for (; count > offset; offset = 8)
			{
				_stream.Position = bytePosition;
				int temp = g1();
				temp &= ~BitMasks[offset];
				temp |= (value >> (count - offset)) & BitMasks[offset];
				_stream.Position = bytePosition;
				p1(temp);
				count -= offset;

				if ((bytePosition + 1) > Length)
				{
					SetLength(bytePosition + 1);
				}

			}
			if (count == offset)
			{
				_stream.Position = bytePosition;
				int temp = g1();
				temp &= ~BitMasks[offset];
				temp |= value & BitMasks[offset];
				Position--;
				p1(temp);
				Position--;
			}
			else
			{
				_stream.Position = bytePosition;
				int temp = g1();
				temp &= ~BitMasks[offset] << (offset - count);
				temp |= (value & BitMasks[count]) << (offset - count);
				Position--;
				p1(temp);
				Position--;
			}
		}

		public int gBits(int n)
		{
			int bytePosition = BitPosition >>> 3;
			int remaining = 8 - (BitPosition & 7);
			int value = 0;
			BitPosition += n;

			byte[] data = toByteArray();

			//using (var memoryStream = new MemoryStream())
			//{
			//	_stream.CopyTo(memoryStream);
			//	memStream = memoryStream.ToArray();
			//}

			for (; n > remaining; remaining = 8)
			{
				value += (data[bytePosition++] & BitMasks[remaining]) << (n - remaining);
				n -= remaining;
			}

			if (n == remaining)
			{
				value += data[bytePosition] & BitMasks[remaining];
			}
			else
			{
				value += (data[bytePosition] >>> (remaining - n)) & BitMasks[n];
			}

			//var mem = new MemoryStream(data);
			//mem.CopyTo(_stream);

			//for (; n > remaining; remaining = 8)
			//{
			//	value += (g1() & BitMasks[remaining]) << (n - remaining);
			//	n -= remaining;
			//}

			//if (n == remaining)
			//{
			//	value += g1() & BitMasks[remaining];
			//	Position--;
			//}
			//else
			//{
			//	value += (g1() >>> (remaining - n)) & BitMasks[n];
			//	Position--;
			//}
			return value;
		}

        public int g1()
		{
			return _streamReader.ReadByte();
		}

		public int g2()
		{
			return (_streamReader.ReadByte() << 8) + _streamReader.ReadByte();
		}

		public int g3()
		{
			return (_streamReader.ReadByte() << 16) + (_streamReader.ReadByte() << 8) + _streamReader.ReadByte();
		}

		public int g4()
		{
			return (_streamReader.ReadByte() << 24) + (_streamReader.ReadByte() << 16) + (_streamReader.ReadByte() << 8) + _streamReader.ReadByte();
		}

		public long g8()
		{
			var l1 = g4() & 0xFFFFFFFFL;
			var l2 = g4() & 0xFFFFFFFFL;
			return l2 + (l1 << 32);
		}


		public string gjstr()
		{
			StringBuilder sb = new StringBuilder();
			byte c;
			while ((c = (byte) g1()) !=  10)
			{
				sb.Append(c);
			}
			return sb.ToString();
		}

		public byte[] toByteArray()
		{
			byte[] data = new byte[Position];
			_stream.Position = 0;
			_stream.Read(data, 0, data.Length);
			return data;
		}
    }
}
