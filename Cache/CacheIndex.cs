using Silvarea.Network;
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
			Console.WriteLine($"Processing file: {file}");

			Packet index = new Packet(_indexFile);
            index.Seek(file * 6, SeekOrigin.Begin);

			Console.WriteLine($"Filesize = {index.Length}");

			int fileSize = index.g3();
			int fileSector = index.g3();
			int remainingBytes = fileSize;
			int sector = fileSector;
			
			Console.WriteLine($"Fileinfo: filesize = {fileSize}, fileSector = {fileSector}");

			MemoryStream finalBuffer = new MemoryStream(fileSize);

			while (remainingBytes > 0)
			{
				Packet mainBlock = new Packet(_dataFile);
				mainBlock.Seek(sector * 520, SeekOrigin.Begin);
				int nextFile = mainBlock.g2();
				int currentSector = mainBlock.g2();
				int nextSector = mainBlock.g3();
				int nextCache = mainBlock.g1();
				//Console.WriteLine("nextFile = " + nextFile + ", currentSector = " + currentSector + ", nextSector = " + nextSector + ", nextCache = " + nextCache);
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
