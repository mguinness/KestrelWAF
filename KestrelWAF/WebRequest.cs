using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Net.Http.Headers;
using System.IO;
using System.Net;
using IPNetwork = System.Net.IPNetwork;

namespace KestrelWAF;

public record WebRequest(HttpRequest request, MaxMindDb geo, IMemoryCache cache)
{
    public string Method
    {
        get { return request.Method; }
    }

    public string Path
    {
        get { return request.Path.Value; }
    }

    public string QueryString
    {
        get { return request.QueryString.Value; }
    }

    public string Referer
    {
        get { return request.Headers[HeaderNames.Referer].ToString(); }
    }

    public string UserAgent
    {
        get { return request.Headers[HeaderNames.UserAgent].ToString(); }
    }

    public string RemoteIp
    {
        get { return request.HttpContext.Connection.RemoteIpAddress.ToString(); }
    }

    public bool Authenticated
    {
        get { return request.HttpContext.User.Identity.IsAuthenticated; }
    }

    private string ipCountry;

    public string IpCountry
    {
        get
        {
            if (ipCountry == null)
            {
                var data = geo.Lookup(request.HttpContext.Connection.RemoteIpAddress);
                if (data != null)
                {
                    var cnty = data["country"] as Dictionary<string, object>;
                    ipCountry = (string)cnty["isocode"];
                }
            }

            return ipCountry;
        }
    }

    public bool InSubnet(string ip, int mask)
    {
        var network = new IPNetwork(IPAddress.Parse(ip), mask);
        return network.Contains(request.HttpContext.Connection.RemoteIpAddress);
    }

    public bool IpInFile(string path)
    {
        var keyname = System.IO.Path.GetFileNameWithoutExtension(path);

        var data = cache.Get<IEnumerable<string>>(keyname);
        if (data == null && File.Exists(path))
        {
            data = cache.Set<IEnumerable<string>>(keyname, File.ReadAllLines(path),
                new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(15)));
        }

        return data?.Contains(RemoteIp, StringComparer.OrdinalIgnoreCase) ?? false;
    }
}