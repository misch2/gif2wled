using System.Drawing;
using WLEDControlApi.Domain;
using WLEDControlApi.Interfaces;

namespace WLEDControlApi.Business
{
    public class ChannelMapperWLEDSimple : IChannelMapper
    {
        // WLED uses 510 channels per universe (170 LEDs), no LED is shared between universes
        private const int WLED_PIXELS_PER_UNIVERSE = 170;
        private const int WLED_CHANNELS_PER_PIXEL = 3;
        private const int WLED_CHANNELS_PER_UNIVERSE = WLED_PIXELS_PER_UNIVERSE * WLED_CHANNELS_PER_PIXEL;

        public IEnumerable<E131SubPixelInfo> MapPixel(Color color, int x, int y, int width, int height)
        {
            var ret = new List<E131SubPixelInfo>();

            var absChannelNumberStart = (x + y * width) * WLED_CHANNELS_PER_PIXEL;

            var localUniverseNumber = (ushort)(absChannelNumberStart / WLED_CHANNELS_PER_UNIVERSE);
            var localChannelNumber = (ushort)(absChannelNumberStart % WLED_CHANNELS_PER_UNIVERSE);

            ret.Add(new E131SubPixelInfo
            {
                Universe = localUniverseNumber,
                Channel = localChannelNumber,
                Value = color.R
            });
            ret.Add(new E131SubPixelInfo
            {
                Universe = localUniverseNumber,
                Channel = (ushort)(localChannelNumber + 1),
                Value = color.G
            });
            ret.Add(new E131SubPixelInfo
            {
                Universe = localUniverseNumber,
                Channel = (ushort)(localChannelNumber + 2),
                Value = color.B
            });

            return ret;
        }
    }
}
