using System.Text;
using Silvarea.Cache;

namespace Silvarea.Network
{

	public enum ProtocolOpcode
	{
		LOGIN = 14,
		UPDATE = 15
	}

	public class ProtocolDecoder
	{

		public static void Decode(Session session, int size)
		{

			MemoryStream memoryStream = new MemoryStream(session.inBuffer, 0, session.inBuffer.Length);
			BinaryReader binaryReader = new BinaryReader(memoryStream, Encoding.UTF8);

			switch(session.CurrentState) 
			{
				case RS2ConnectionState.INITIAL:
					var opcode = binaryReader.ReadByte();

					if (opcode == (byte)ProtocolOpcode.UPDATE)
					{
						var bytesToFlip = BitConverter.GetBytes(binaryReader.ReadInt32());
						Array.Reverse(bytesToFlip, 0, bytesToFlip.Length);
						var versionId = BitConverter.ToInt32(bytesToFlip, 0);

						// TODO: Don't hardcode this
						if (versionId == 410)
						{
							session.Stream.Write([0]);
							session.CurrentState = RS2ConnectionState.UPDATE;
						}
					}
					break;
				case RS2ConnectionState.UPDATE:
					if (size >= 4)
					{
						var requestType = binaryReader.ReadByte();
						var indexNumber = binaryReader.ReadByte();

						var bytesToFlip = BitConverter.GetBytes(binaryReader.ReadInt16());
						Array.Reverse(bytesToFlip, 0, bytesToFlip.Length);
						var fileNumber = BitConverter.ToInt16(bytesToFlip, 0);
						if (requestType < 2) {//non-urgent and urgent requests
							session.Stream.Write(UpdateServer.getRequest(indexNumber, fileNumber).ToArray());
						}
						Console.WriteLine($"Request Type: {requestType} Index Number: {indexNumber} File Number: {fileNumber}");

					}
					break;
			}
		}
	}
}
