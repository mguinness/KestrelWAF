 # Kestrel WAF
A basic WAF for the Kestrel web server.

## Introduction
A [web application firewall](https://en.wikipedia.org/wiki/Web_application_firewall) is software that monitors and blocks HTTP traffic to a web service.

Using [Reverse Proxy](https://microsoft.github.io/reverse-proxy/) from Microsoft allows this project to both filter and forward traffic to another server.

This project is an attempt to implement a rules based WAF using [ASP.NET Core Middleware](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/).

## BRE

[Business rules engine](https://en.wikipedia.org/wiki/Business_rules_engine) is software that executes one or more business rules in a configurable runtime environment.

This provides flexibility to the end user to define rules to control inbound web traffic with little or no programming experience.

## Setup

This projects uses the [Micro Rule Engine](https://github.com/runxc1/MicroRuleEngine) based on [Expression Trees](https://docs.microsoft.com/en-us/dotnet/csharp/expression-trees).

That project [README](https://github.com/runxc1/MicroRuleEngine/blob/master/README.md) covers the different kinds of expressions that can be used, so I'd encourage you to read that beforehand.

Rules will then be defined and stored in the appsettings.json file using ASP.NET Core [Configuration](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-5.0#bind-hierarchical-configuration-data-using-the-options-pattern) options pattern.

An instance of the WebRequest class is created for each request which exposes fields like URL, IP address, user agent etc. for the rules engine to interact with.

Below is a example of different rules that can be defined.  In addition rules may be nested for more complex logic.

```JSON
"Configuration": {
  "Ruleset": {
    "Operator": "OrElse",
    "Rules": [
      {
        "MemberName": "Path",
        "Operator": "EndsWith",
        "Inputs": [ ".php" ]
      },
      {
        "MemberName": "UserAgent",
        "Operator": "IsMatch",
        "TargetValue": "^(curl|java|python)"
      },
      {
        "Operator": "NotInSubnet",
        "Inputs": [ "192.168.10.0", 24 ]
      }
    ]
  }
}
```

When a web request is received and processed by the rules, if any of the above match the request will be rejected and will return a 403 Forbidden [status code](https://en.wikipedia.org/wiki/List_of_HTTP_status_codes#4xx_client_errors).

## GeoLite2

MaxMind provides free [Geolocation data](https://dev.maxmind.com/geoip/geoip2/geolite2/).  Register and download the GeoLite2 database and specify the file location in the settings file.

```JSON
"Configuration": {
  "GeoLiteFile": "C:\\MaxMind\\GeoLite2-Country.mmdb"
}
```

You will be able to lookup the geographic location of any IP address which will allow you to block requests by country if required.

```JSON
{
  "MemberName": "IpCountry",
  "Operator": "IsInInput",
  "Inputs": [ "CN", "RU" ]
}
```

## Conclusion

This is a very simple implementation of a WAF, but as you can see it can be expanded upon very easily.  Any contributions to this project would be welcomed.

## Credits

YARP: A Reverse Proxy
https://github.com/microsoft/reverse-proxy

Micro Rule Engine
https://github.com/runxc1/MicroRuleEngine

MaxMind DB Reader
https://github.com/maxmind/MaxMind-DB-Reader-dotnet
