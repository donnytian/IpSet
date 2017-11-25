namespace System.Net
{
    /// <summary>
    /// Provides extension method for <see cref="IpRange"/>
    /// </summary>
    public static class IpRangeExtensions
    {
        /// <summary>
        /// Checks whether the <see cref="IpRange"/> contains specified <see cref="IPAddress"/>.
        /// </summary>
        /// <param name="ipRange">The <see cref="IpRange"/> object.</param>
        /// <param name="ipAddress">The IP address to be checked.</param>
        /// <returns>True if contains; otherwise false.</returns>
        public static bool Contains(this IpRange ipRange, string ipAddress)
        {
            var address = IPAddress.Parse(ipAddress);

            return ipRange.Contains(address);
        }
    }
}
