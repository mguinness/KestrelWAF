using MaxMind.Db;
using System.IO;
using System.Net;

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
