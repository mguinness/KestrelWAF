using MaxMind.Db;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace KestrelWAF
{
    public class MaxMindDb
    {
        private Reader _reader;

        public MaxMindDb(string file)
        {
            if (File.Exists(file))
                _reader = new Reader(file, FileAccessMode.Memory);
        }

        public Dictionary<string, object> Lookup(IPAddress ip)
        {
            return _reader?.Find<Dictionary<string, object>>(ip);
        }
    }
}
