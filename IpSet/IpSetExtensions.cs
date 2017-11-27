namespace System.Net
{
    /// <summary>
    /// Provides extension method for <see cref="IpSet"/>
    /// </summary>
    public static class IpSetExtensions
    {
        /// <summary>
        /// Checks whether the <see cref="IpSet"/> contains specified <see cref="IPAddress"/>.
        /// </summary>
        /// <param name="ipSet">The <see cref="IpSet"/> object.</param>
        /// <param name="ipAddress">The IP address to be checked.</param>
        /// <returns>True if contains; otherwise false.</returns>
        public static bool Contains(this IpSet ipSet, string ipAddress)
        {
            var address = IPAddress.Parse(ipAddress);

            return ipSet.Contains(address);
        }
    }
}
