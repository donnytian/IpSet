[![Build Status](https://dev.azure.com/dtian/IpSet/_apis/build/status/donnytian.IpSet?branchName=master)](https://dev.azure.com/dtian/IpSet/_build/latest?definitionId=1&branchName=master)
# IpSet
A simple tool to enumerate IpAddress and check if an IP address is contained by the defined range or set.

Support both **IPv4** and **IPv6**.

Support **CIDR** notation.

## Install from NuGet
In the Package Manager Console:

`PM> Install-Package IpSet`

## Use IpRange

`IpRange` represents a consecutive IP addresses.

Check an IP address.
```csharp
    var range = IpRange.ParseOrDefault("192.168.0.0/16");
    var result = range.Contains("192.168.2.99"); // true
```

Support multiple parsing formats.
```csharp
    [Theory]
    [InlineData("192.168.0.10 - 192.168.10.20", "192.168.20", true)]
    [InlineData("192.168.0.*", "192.168.0.255", true)]
    [InlineData("192.168.0.*", "192.168.1.0", false)]
    [InlineData("192.168.0.0/255.255.255.0", "192.168.1.100", false)]
    [InlineData("192.168.0.0/16", "192.168.2.99", true)]
    [InlineData("fe80::/10", "192.168.0.1", false)]
    [InlineData("192.168.0.10", "192.168.0.11", false)]
    public void Parse_And_Contains_Tests(string s, string testIp, bool expected)
```

## Use IpSet

`IpSet` represents a group of `IpRange` instances. Ranges should be separated by **comma** when parsing.

**`IpSet` is the recommended class** over `IpRange` since it can hold one or multiple ranges.
More features will be added on it later.

Check an IP address.
```csharp
    var set = IpSet.ParseOrDefault("192.168.0.*,10.10.1.0/24,fe80::/10");
    var result = set.Contains("192.168.2.99"); // false
    result = set.Contains("10.10.1.200"); // true
    result = set.Contains("fe80::"); // true
```

Support multiple parsing formats.

Support any string conjunctions of IpRange formats by comma(","). Such as:

```csharp
var set = IpSet.ParseOrDefault("192.168.0.10 - 192.168.10.20, 192.168.1.*, fe80::/10, 10.10.1.200");
```