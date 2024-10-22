using System.Net;

namespace WLEDControlApi.Services
{
    public interface IDNSCacheService
    {
        public IPAddress? GetIPAddress(string hostname);
    }
}