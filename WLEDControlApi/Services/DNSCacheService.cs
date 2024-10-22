using Microsoft.Extensions.Caching.Memory;
using System.Net;

namespace WLEDControlApi.Services
{
    public class DNSCacheService : IDNSCacheService
    {
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _cacheDuration;

        public DNSCacheService(IMemoryCache cache, TimeSpan cacheDuration)
        {
            _cache = cache;
            _cacheDuration = cacheDuration;
        }

        public IPAddress? GetIPAddress(string hostname)
        {
            if (_cache.TryGetValue(hostname, out IPAddress? cachedIP))
            {
                // Return the cached IP address
                return cachedIP;
            }

            // Resolve the hostname to IP address
            IPAddress[] addresses = Dns.GetHostAddresses(hostname);
            if (addresses.Length > 0)
            {
                // Cache the first IP address
                _cache.Set(hostname, addresses[0], _cacheDuration);
                return addresses[0];
            }

            throw new Exception($"Unable to resolve host {hostname}");
        }
    }
}