using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace KestrelWAF
{
    public class WebRequest
    {
        private readonly HttpRequest _request;
        private readonly MaxMindDb _geo;
        private readonly IMemoryCache _cache;

        public WebRequest(HttpRequest request, MaxMindDb geo, IMemoryCache cache)
        {
            _request = request;
            _geo = geo;
            _cache = cache;
        }

        public string Method
        {
            get { return _request.Method; }
        }

        public string Path
        {
            get { return _request.Path.Value; }
        }

        public string QueryString
        {
            get { return _request.QueryString.Value; }
        }

        public string Referer
        {
            get { return _request.Headers[HeaderNames.Referer].ToString(); }
        }

        public string UserAgent
        {
            get { return _request.Headers[HeaderNames.UserAgent].ToString(); }
        }

        public string RemoteIp
        {
            get { return _request.HttpContext.Connection.RemoteIpAddress.ToString(); }
        }

        public bool Authenticated
        {
            get { return _request.HttpContext.User.Identity.IsAuthenticated; }
        }

        private string ipCountry;
        public string IpCountry
        {
            get
            {
                if (ipCountry == null)
                {
                    var data = _geo.Lookup(_request.HttpContext.Connection.RemoteIpAddress);
                    if (data != null)
                    {
                        var cnty = data["country"] as Dictionary<string, object>;
                        ipCountry = (string)cnty["iso_code"];
                    }
                }
                return ipCountry;
            }
        }

        public bool InSubnet(string ip, int mask)
        {
            var network = new IPNetwork(IPAddress.Parse(ip), mask);
            return network.Contains(_request.HttpContext.Connection.RemoteIpAddress);
        }

        public bool IpInFile(string path)
        {
            var keyname = System.IO.Path.GetFileNameWithoutExtension(path);

            var data = _cache.Get<IEnumerable<string>>(keyname);
            if (data == null && File.Exists(path))
            {
                data = _cache.Set<IEnumerable<string>>(keyname, File.ReadAllLines(path),
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(15)));
            }

            return data?.Contains(RemoteIp, StringComparer.OrdinalIgnoreCase) ?? false;
        }
    }
}
