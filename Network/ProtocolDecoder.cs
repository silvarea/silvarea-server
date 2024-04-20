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
			// TODO: Fix this trash
			byte opcode = 0;
			byte[] data = session.inBuffer;
			if (session.CurrentState != RS2ConnectionState.UPDATE)
			{
                opcode = session.inBuffer[0];
                data = new byte[session.inBuffer.Length - 1];
                Array.Copy(session.inBuffer, 1, data, 0, data.Length);
            }
            Packet packet = new Packet(opcode, data);

            switch (session.CurrentState) 
			{
				case RS2ConnectionState.INITIAL:
					if (opcode == (byte)ProtocolOpcode.UPDATE)
					{
						var versionId = packet.g4();

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
                        var requestType = packet.g1();
						var indexNumber = packet.g1();
						var fileNumber = packet.g2();

						if (requestType < 2) //non-urgent and urgent requests
						{ 
							session.Stream.Write(UpdateServer.getRequest(indexNumber, fileNumber).ToArray());
						}
						Console.WriteLine($"Request Type: {requestType} Index Number: {indexNumber} File Number: {fileNumber}");
                    }
					break;
            }
            session.Stream.Flush();
        }
	}
}
