using WLEDControlApi.Domain;

namespace WLEDControlApi.Interfaces
{
    public interface IWLEDService
    {
        public void PlayGifForSpecifiedTime(
            string gifName,
            double durationInSeconds,
            string destinationHost,
            double fps = 25
        );
    }
}
