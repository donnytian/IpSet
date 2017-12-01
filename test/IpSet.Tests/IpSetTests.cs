using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace IpSetTests
{
    public class IpSetTests
    {
        [Theory]
        [InlineData("192.168.0.10 - 192.168.10.20", "192.168.20", true)]
        [InlineData("192.168.0.*,10.10.1.0/24", "10.10.1.200", true)]
        [InlineData("192.168.0.1,192.168.0.3", "192.168.0.2", false)]
        [InlineData("fe80::/10", "fe80::", true)]
        [InlineData("fe80::/10", "a000::", false)]
        public void Parse_And_Contains_Tests(string s, string testIp, bool expected)
        {
            // Act
            var set = IpSet.ParseOrDefault(s);
            var result = set.Contains(testIp);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
