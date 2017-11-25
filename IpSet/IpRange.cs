using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using Extensions.Byte;

namespace System.Net
{
    /// <summary>
    /// Represents a range of consecutive IP addresses.
    /// </summary>
    public class IpRange : IEnumerable<IPAddress>
    {
        /// <summary>
        /// Bitwise length for IP v4 address.
        /// </summary>
        public const byte Ipv4Length = 32;

        /// <summary>
        /// Bitwise length for IP v6 address.
        /// </summary>
        public const byte Ipv6Length = 128;

        private BigInteger _low;
        private BigInteger _high;

        /// <summary>
        /// Initializes a new instance of the <see cref="IpRange"/> class.
        /// </summary>
        /// <param name="single">The single address in the range.</param>
        public IpRange(IPAddress single)
            : this(single, single)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IpRange"/> class.
        /// </summary>
        /// <param name="begin">The beginning address.</param>
        /// <param name="end">The ending address.</param>
        public IpRange(IPAddress begin, IPAddress end)
        {
            Begin = begin ?? throw new ArgumentNullException(nameof(begin));
            End = end ?? throw new ArgumentNullException(nameof(end));

            if (Begin.AddressFamily != End.AddressFamily)
            {
                throw new ArgumentException("IP addresses must be in same family, either v4 or v6");
            }

            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IpRange"/> class.
        /// </summary>
        /// <param name="baseAddress">The base address.</param>
        /// <param name="netmask">The length of masked bits.</param>
        public IpRange(IPAddress baseAddress, byte netmask)
        {
            if (baseAddress == null)
            {
                throw new ArgumentNullException(nameof(baseAddress));
            }

            var baseBytes = baseAddress.GetAddressBytes();

            if (netmask > baseBytes.Length * 8)
            {
                throw new ArgumentOutOfRangeException(nameof(netmask));
            }

            var maskBytes = GetMaskBytes(baseBytes.Length, netmask);
            Begin = new IPAddress(baseBytes.And(maskBytes));
            End = new IPAddress(baseBytes.Or(maskBytes.Not()));
            Initialize();
        }

        /// <summary>
        /// Gets the beginning address of the range.
        /// </summary>
        public IPAddress Begin { get; }

        /// <summary>
        /// Gets the ending address of the range.
        /// </summary>
        public IPAddress End { get; }

        /// <summary>
        /// Gets an unsigned <see cref="BigInteger"/> from the specified <see cref="IPAddress"/> object.
        /// </summary>
        /// <param name="address">The IP address.</param>
        /// <returns>A <see cref="BigInteger"/> that represents the given IP address.</returns>
        public static BigInteger GetBigInteger(IPAddress address)
        {
            if (address == null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            var bytes = address.GetAddressBytes();

            // Pads 0 to prevent accidental negative numbers.
            var unsignedBytes = new byte[bytes.Length + 1];
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes, 0, bytes.Length);
                Array.Copy(bytes, unsignedBytes, bytes.Length);
            }
            else
            {
                Array.Copy(bytes, 0, unsignedBytes, 1, bytes.Length);
            }

            return new BigInteger(unsignedBytes);
        }

        /// <summary>
        /// Converts the string representation of a IP address range to <see cref="IpRange"/> equivalent.
        /// A not null return value indicates the conversion succeeded.
        /// </summary>
        /// <example>
        /// 192.168.0.10 - 192.168.10.20
        /// 192.168.0.*
        /// 192.168.0.0/255.255.255.0
        /// 192.168.0.0/16
        /// fe80::/10
        /// </example>
        /// <param name="s">A string containing a IP address range to convert.</param>
        /// <returns><see cref="IpRange"/> equivalent if s was converted successfully; otherwise, null.</returns>
        public static IpRange ParseOrDefault(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return null;
            }

            s = s.Replace(" ", string.Empty);

            var partsByDash = s.Split('-');
            if (partsByDash.Length > 1)
            {
                // 192.168.0.10 - 192.168.10.20
                return ParseOrDefault(partsByDash[0], partsByDash[1]);
            }

            var partsBySlash = s.Split('/');
            if (partsBySlash.Length > 1)
            {
                if (byte.TryParse(partsBySlash[1], out var maskLength))
                {
                    // 192.168.0.0/16
                    return ParseOrDefault(partsBySlash[0], maskLength);
                }

                // 192.168.0.0/255.255.255.0
                return IPAddress.TryParse(partsBySlash[1], out var maskAddress) ? ParseOrDefault(partsBySlash[0], maskAddress) : null;
            }

            if (s.Contains("*"))
            {
                if (IPAddress.TryParse(s.Replace("*", "0"), out var beginAddress)
                    && IPAddress.TryParse(s.Replace("*", "255"), out var endAddress))
                {
                    // 192.168.0.*
                    return new IpRange(beginAddress, endAddress);
                }
            }

            // Final attempt, take string a single IP address such as 192.168.0.0
            if (IPAddress.TryParse(s, out var ipAddress))
            {
                return new IpRange(ipAddress);
            }

            return null;
        }

        /// <summary>
        /// Converts the string representation of a IP address range to <see cref="IpRange"/> equivalent.
        /// A not null return value indicates the conversion succeeded.
        /// </summary>
        /// <param name="begin">The string for beginning address.</param>
        /// <param name="end">The string for ending address.</param>
        /// <returns><see cref="IpRange"/> equivalent if s was converted successfully; otherwise, null.</returns>
        public static IpRange ParseOrDefault(string begin, string end)
        {
            if (IPAddress.TryParse(begin, out var beginAddress) && IPAddress.TryParse(end, out var endAddress))
            {
                return new IpRange(beginAddress, endAddress);
            }

            return null;
        }

        /// <summary>
        /// Converts the string representation of a IP address and netmask to <see cref="IpRange"/> equivalent.
        /// A not null return value indicates the conversion succeeded.
        /// </summary>
        /// <param name="ipBase">A string containing a base IP address.</param>
        /// <param name="netmask">A byte to indicate the length of netmask.</param>
        /// <returns><see cref="IpRange"/> equivalent if s was converted successfully; otherwise, null.</returns>
        public static IpRange ParseOrDefault(string ipBase, byte netmask)
        {
            if (string.IsNullOrWhiteSpace(ipBase))
            {
                return null;
            }

            if (IPAddress.TryParse(ipBase, out var ipAddress))
            {
                return ParseOrDefault(ipAddress, netmask);
            }

            return null;
        }

        /// <summary>
        /// Converts the string representation of a IP address and netmask to <see cref="IpRange"/> equivalent.
        /// A not null return value indicates the conversion succeeded.
        /// </summary>
        /// <param name="ipBase">A string containing a base IP address.</param>
        /// <param name="netmask">A <see cref="IPAddress"/> object to indicate the netmask.</param>
        /// <returns><see cref="IpRange"/> equivalent if s was converted successfully; otherwise, null.</returns>
        public static IpRange ParseOrDefault(string ipBase, IPAddress netmask)
        {
            if (string.IsNullOrWhiteSpace(ipBase) || netmask == null)
            {
                return null;
            }

            if (IPAddress.TryParse(ipBase, out var ipAddress))
            {
                var maskBytes = netmask.GetAddressBytes();
                var maskLength = maskBytes.Select(b => Convert.ToString(b, 2).Count(c => c == '1')).Sum();
                return ParseOrDefault(ipAddress, (byte)maskLength);
            }

            return null;
        }

        /// <summary>
        /// Converts the string representation of a IP address and netmask to <see cref="IpRange"/> equivalent.
        /// A not null return value indicates the conversion succeeded.
        /// </summary>
        /// <param name="ipBase">An base IP address.</param>
        /// <param name="netmask">A byte to indicate the length of netmask.</param>
        /// <returns><see cref="IpRange"/> equivalent if s was converted successfully; otherwise, null.</returns>
        public static IpRange ParseOrDefault(IPAddress ipBase, byte netmask)
        {
            if (ipBase == null)
            {
                return null;
            }

            if (ipBase.AddressFamily == AddressFamily.InterNetwork && netmask > Ipv4Length)
            {
                return null;
            }

            if (ipBase.AddressFamily == AddressFamily.InterNetworkV6 && netmask > Ipv6Length)
            {
                return null;
            }

            var baseBytes = ipBase.GetAddressBytes();
            var maskBytes = GetMaskBytes(baseBytes.Length, netmask);
            var begin = new IPAddress(baseBytes.And(maskBytes));
            var end = new IPAddress(baseBytes.Or(maskBytes.Not()));

            return new IpRange(begin, end);
        }

        /// <inheritdoc />
        public IEnumerator<IPAddress> GetEnumerator()
        {
            for (var ip = _low; ip <= _high; ip++)
            {
                yield return new IPAddress(ip.ToByteArray());
            }
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Checks whether the <see cref="IpRange"/> contains specified <see cref="IPAddress"/>.
        /// </summary>
        /// <param name="ipAddress">The IP address to be checked.</param>
        /// <returns>True if contains; otherwise false.</returns>
        public bool Contains(IPAddress ipAddress)
        {
            if (ipAddress == null)
            {
                throw new ArgumentNullException(nameof(ipAddress));
            }

            if (ipAddress.AddressFamily != Begin.AddressFamily)
            {
                return false;
            }

            var number = GetBigInteger(ipAddress);

            return number >= _low && number <= _high;
        }

        private static byte[] GetMaskBytes(int byteLength, int maskLength)
        {
            var maskBytes = new byte[byteLength];
            var bytesLen = maskLength / 8;
            var bitsLen = maskLength % 8;

            for (var i = 0; i < bytesLen; i++)
            {
                maskBytes[i] = 0xff;
            }

            if (bitsLen > 0)
            {
                // Shifts right and then shifts left, so we can get desired number of '1's.
                var b = 0xff >> (8 - bitsLen);
                maskBytes[bytesLen] = (byte)(b << (8 - bitsLen));
            }

            return maskBytes;
        }

        private void Initialize()
        {
            _low = GetBigInteger(Begin);
            _high = GetBigInteger(End);

            if (_low > _high)
            {
                throw new ArgumentException("[begin] must be less than or equals to [end]");
            }
        }
    }
}
