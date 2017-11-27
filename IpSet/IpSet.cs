using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace System.Net
{
    /// <summary>
    /// Represents a set of IP addresses.
    /// </summary>
    public class IpSet : IEnumerable<IPAddress>
    {
        private readonly HashSet<IpRange> _ranges = new HashSet<IpRange>();

        /// <summary>
        /// Initializes a new instance of the <see cref="IpSet"/> class.
        /// </summary>
        /// <param name="range">The IP range.</param>
        public IpSet(IpRange range)
            : this(new[] { range })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IpSet"/> class.
        /// </summary>
        /// <param name="ranges">The IP ranges.</param>
        public IpSet(IEnumerable<IpRange> ranges)
        {
            foreach (var range in ranges)
            {
                _ranges.Add(range);
            }
        }

        /// <summary>
        /// Converts the string representation of a group of IP address ranges to <see cref="IpSet"/> equivalent.
        /// IP address ranges should be separated by comma.
        /// A not null return value indicates the conversion succeeded.
        /// </summary>
        /// <example>
        /// 192.168.0.10 - 192.168.10.20,192.168.0.*,192.168.0.0/255.255.255.0,192.168.0.0/16,fe80::/10,192.168.0.0
        /// </example>
        /// <param name="s">A string containing a group of IP address ranges to convert.</param>
        /// <returns><see cref="IpSet"/> equivalent if s was converted successfully; otherwise, null.</returns>
        public static IpSet ParseOrDefault(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return null;
            }

            var parts = s.Split(',');
            var ranges = parts.Select(IpRange.ParseOrDefault).Where(r => r != null);

            return new IpSet(ranges);
        }

        /// <summary>
        /// Converts the string representations of a group of IP address ranges to <see cref="IpSet"/> equivalent.
        /// A not null return value indicates the conversion succeeded.
        /// </summary>
        /// <example>
        /// 192.168.0.10 - 192.168.10.20,192.168.0.*,192.168.0.0/255.255.255.0,192.168.0.0/16,fe80::/10,192.168.0.0
        /// </example>
        /// <param name="rangeStrings">Strings containing a group of IP address ranges to convert.</param>
        /// <returns><see cref="IpSet"/> equivalent if strings were converted successfully; otherwise, null.</returns>
        public static IpSet ParseOrDefault(IEnumerable<string> rangeStrings)
        {
            if (rangeStrings == null)
            {
                return null;
            }

            var list = new List<IpRange>();

            foreach (var s in rangeStrings)
            {
                if (string.IsNullOrWhiteSpace(s))
                {
                    continue;
                }

                var parts = s.Split(',');
                var ranges = parts.Select(IpRange.ParseOrDefault).Where(r => r != null);
                list.AddRange(ranges);
            }

            return new IpSet(list);
        }

        /// <inheritdoc />
        public IEnumerator<IPAddress> GetEnumerator()
        {
            foreach (var range in _ranges)
            {
                foreach (var ip in range)
                {
                    yield return ip;
                }
            }
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Checks whether the <see cref="IpSet"/> contains specified <see cref="IPAddress"/>.
        /// </summary>
        /// <param name="ipAddress">The IP address to be checked.</param>
        /// <returns>True if contains; otherwise false.</returns>
        public bool Contains(IPAddress ipAddress)
        {
            if (ipAddress == null)
            {
                throw new ArgumentNullException(nameof(ipAddress));
            }

            return _ranges.Any(r => r.Contains(ipAddress));
        }
    }
}
