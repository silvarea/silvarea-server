using System.Text;

namespace Silvarea.Network
{

	public class Packet : Stream
	{
		private static int[] BIT_MASKS = new int[32];
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
				BIT_MASKS[i] = ((1 <<  i) - 1);
		}

		private void initPacket(Stream data)
		{
			

			_stream = new BufferedStream(data);
			if(_stream.CanRead)  _streamReader = new BinaryReader(_stream);
			if(_stream.CanWrite) _streamWriter = new BinaryWriter(_stream);
        }

		public override bool CanRead => _stream.CanRead;

		public override bool CanSeek => _stream.CanSeek;

		public override bool CanWrite => _stream.CanWrite;

		public override long Length => _stream.Length;

		public override long Position { get => _stream.Position; set => _stream.Position = value; }

		public int BitPosition;

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
			_stream.SetLength(5);
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
			//Array.Reverse(data, count, data.Length);
			//pdata(data, data.Length);
			for(int i = count - 1; i >= 0; i--)
				p1(data[i]);
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

		public void pBits(int count, int value)
		{
			int bytePosition = BitPosition >> 3;
			int offset = 8 - (BitPosition & 7);
			BitPosition += count;
			for (; count > offset; offset = 8)
			{
				_stream.Position = bytePosition;
				int temp = g1();
				temp &= ~BIT_MASKS[offset];
				temp |= (value >> (count - offset)) & BIT_MASKS[offset];
                _stream.Position = bytePosition;
				p1(temp);
				count -= offset;
            }
			if (count == offset)
			{
                _stream.Position = bytePosition;
                int temp = g1();
                temp &= ~BIT_MASKS[offset];
				temp |= value & BIT_MASKS[offset];
				Position--;
                p1(temp);
				Position--;
            } else
			{
                _stream.Position = bytePosition;
                int temp = g1();
                temp &= ~BIT_MASKS[offset];
                temp |= (value & BIT_MASKS[count]) << (offset - count);
				Position--;
                p1(temp);
				Position--;
            }
		}

		public int gBits(int count)
		{
			int bytePosition = BitPosition >>> 3;
			int remaining = 8 - (BitPosition & 7);
			int value = 0;
			BitPosition += count;

			for (; count > remaining; remaining = 8)
			{
				value += (g1() & BIT_MASKS[remaining]) << (count - remaining);
				count -= remaining;
			}

			if (count == remaining)
			{
				value += g1() & BIT_MASKS[remaining];
				Position--;
			} else
			{
				value += (g1() >>> (remaining - count)) & BIT_MASKS[count];
				Position--;
			}
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
