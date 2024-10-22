using System.Drawing;
using WLEDControlApi.Domain;

namespace WLEDControlApi.Interfaces
{
    public interface IChannelMapper
    {
        public IEnumerable<E131SubPixelInfo> MapPixel(Color color, int x, int y, int width, int height);
    }
}
