using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace Silvarea.Cache
{
	public class CacheIndex
	{

		private readonly FileStream _dataFile;

		private readonly FileStream _indexFile;

		public CacheIndex(string path, int id, CacheIndex index255)
		{
			_dataFile = File.OpenRead("../../../data/cache/main_file_cache.dat2");
			_indexFile = File.OpenRead("../../../data/cache/main_file_cache.idx" + id);
			if (index255 != null)
			{
				//some FileInformationTable stuff for old format
			}
		}

		public byte[] getFile(int file)
		{
			Console.Write("Processing file: " + file);
            BufferedStream indexStream = new BufferedStream(_indexFile, 6);
            indexStream.Seek(file * 6, SeekOrigin.Begin);
            BinaryReader indexReader = new BinaryReader(indexStream);
			Console.Write("Filesize = " + indexStream.Length);
            int fileSize = (indexReader.ReadByte() << 16) | (indexReader.ReadByte() << 8) | (indexReader.ReadByte());
			int fileSector = (indexReader.ReadByte() << 16) | (indexReader.ReadByte() << 8) | (indexReader.ReadByte());
			int remainingBytes = fileSize;
			int sector = fileSector;
			Console.Write("Fileinfo: filesize = " + fileSize + ", fileSector = " + fileSector);
			MemoryStream finalBuffer = new MemoryStream(fileSize);
			while (remainingBytes > 0)
			{
				BufferedStream mainBlockStream = new BufferedStream(_dataFile, 520);
				mainBlockStream.Seek(sector * 520, SeekOrigin.Begin);
				BinaryReader mainBlock = new BinaryReader(mainBlockStream);
                //int nextFile = (mainBlock.ReadUInt16() & 0xffff) / 256;//These being divided by 256 is a band-aid. We're reading the short incorrectly somehow. TODO fix later
                byte[] nextFileData = mainBlock.ReadBytes(2);
                Array.Reverse(nextFileData);
                int nextFile = BitConverter.ToUInt16(nextFileData);
                //int currentSector = (mainBlock.ReadUInt16() & 0xffff) / 256;//Hey, brainblast idiot, it's big endian. That's why it's not affecting bytes.
                byte[] currentSectorData = mainBlock.ReadBytes(2);
                Array.Reverse(currentSectorData);
                int currentSector = BitConverter.ToUInt16(currentSectorData);
                int nextSector = (mainBlock.ReadByte() << 16) | (mainBlock.ReadByte() >> 8) | (mainBlock.ReadByte());
				int nextCache = mainBlock.ReadByte();
				Console.WriteLine("nextFile = " + nextFile + ", currentSector = " + currentSector + ", nextSector = " + nextSector + ", nextCache = " + nextCache);
				int remaining = remainingBytes;
				if (remaining > 512)
				{
					remaining = 512;
				}
				byte[] finalData = new byte[remaining];
				mainBlock.Read(finalData);
				finalBuffer.Write(finalData, 0, remaining);
				remainingBytes -= remaining;
				sector = nextSector;
			}
			return finalBuffer.ToArray();
		}

        public void close()
        {
			_dataFile.Close();
			_indexFile.Close();

		}

		public int getLength()
		{
			return (int) _indexFile.Length;
		}

		

    }

}
