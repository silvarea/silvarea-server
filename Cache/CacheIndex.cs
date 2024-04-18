using System;
using System.Collections.Generic;
using System.Linq;
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
			_dataFile = File.Create("./data/cache/main_file_cache.dat2");
			_indexFile = File.Create("./data/cache/main_file_cache.idx" + id);
			if (index255 != null)
			{
				//some FileInformationTable stuff for old format
			}
		}

		public byte[] getFile(int file)
		{
            BufferedStream indexStream = new BufferedStream(_indexFile);
            indexStream.Seek(file * 6, SeekOrigin.Begin);
            BinaryReader indexReader = new BinaryReader(indexStream);
            int fileSize = (indexReader.Read() << 16) | (indexReader.Read() << 8) | (indexReader.Read());
			int fileSector = (indexReader.Read() << 16) | (indexReader.Read() << 8) | (indexReader.Read());
			int remainingBytes = fileSize;
			int sector = fileSector;
			MemoryStream finalBuffer = new MemoryStream(fileSize);
			while (remainingBytes > 0)
			{
				BufferedStream mainBlockStream = new BufferedStream(_dataFile);
				mainBlockStream.Seek(sector * 520, SeekOrigin.Begin);
				BinaryReader mainBlock = new BinaryReader(mainBlockStream);
				int nextFile = mainBlock.ReadUInt16();
				int currentSector = mainBlock.ReadUInt16();
				int nextSector = (mainBlock.Read() << 16) | (mainBlock.Read() >> 8) | (mainBlock.Read());
				int nextCache = mainBlock.Read();
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
	}
}
