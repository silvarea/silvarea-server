using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silvarea.Cache
{
	public class Cache
	{

		public static CacheIndex Index255 { get; set; }

		public static CacheIndex[] Indices { get; set; }

		public static byte[] getCacheFile(int index, int file) {

			if (index == 255)
			{
				return Index255.getFile(file);
			}
			
			return Indices[index].getFile(file); 
		}

		public static CacheIndex getIndex(int index)
		{
			if (index == 255)
			{
				return Index255;
			}
			return Indices[index];
		}

	}
}
