using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Xunit;

namespace IpSetTests
{
    public class IpRangeTests
    {
        [Theory]
        [InlineData("192.168.0.10 - 192.168.10.20", "192.168.20", true)]
        [InlineData("192.168.0.*", "192.168.0.255", true)]
        [InlineData("192.168.0.*", "192.168.1.0", false)]
        [InlineData("192.168.0.0/255.255.255.0", "192.168.1.100", false)]
        [InlineData("192.168.0.0/16", "192.168.2.99", true)]
        [InlineData("fe80::/10", "192.168.0.1", false)]
        [InlineData("192.168.0.10", "192.168.0.11", false)]
        public void Parse_And_Contains_Tests(string s, string testIp, bool expected)
        {
            // Act
            var range = IpRange.ParseOrDefault(s);
            var result = range.Contains(testIp);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
