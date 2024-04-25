using System.Text;

namespace Silvarea.Network
{
	public class Packet : Stream
	{
		private BufferedStream _stream { get; set; }

		private BinaryReader _streamReader {  get; set; }

		private BinaryWriter _streamWriter {  get; set; }

		public int _opcode { get; set; }

		public Packet(int opcode, byte[] data)
		{
			_opcode = opcode;
			initPacket(data);
        }

		public Packet(byte[] data)
		{
			_opcode = -1;
			initPacket(data);
        }

		public Packet(int opcode, Stream data)
		{
			_opcode = opcode;
			initPacket(data);
		}

        public Packet(Stream data)
        {
            _opcode = -1;
            initPacket(data);
        }

        private void initPacket(byte[] data)
		{
			_stream = new BufferedStream(new MemoryStream(data));
            if(_stream.CanRead) _streamReader = new BinaryReader(_stream);
            if(_stream.CanWrite) _streamWriter = new BinaryWriter(_stream);
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

		public void pjstr(string str)
		{
			_stream.Write(Encoding.UTF8.GetBytes(str));

		}

		public void p4(int value)
		{
			_streamWriter.Write((sbyte) (value >> 24));
            _streamWriter.Write((sbyte) (value >> 16));
            _streamWriter.Write((sbyte) (value >> 8));
            _streamWriter.Write((sbyte) value);
        }

		public void p3(int value)
		{
			_streamWriter.Write((sbyte) (value >> 16));
            _streamWriter.Write((sbyte) (value >> 8));
            _streamWriter.Write((sbyte) value);
        }

		public void p2(int value)
		{
            _streamWriter.Write((sbyte) (value >> 8));
            _streamWriter.Write((sbyte) value);
			
        }

		public void p1(int value)
		{
			_streamWriter.Write((sbyte) value);
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
