using System.Drawing;
using WLEDControlApi.Domain;
using WLEDControlApi.Interfaces;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WLEDControlApi.Business
{
    public class ChannelMapperWLED : IChannelMapper
    {
        public IEnumerable<E131SubPixelInfo> MapPixel(Color color, int x, int y, int width, int height)
        {
            var ret = new List<E131SubPixelInfo>();

            // WLED uses 510 channels per universe (170 LEDs), i.e. no LED is shared between universes
            var absChannelNumberStart = x * 3 + y * width * 3;

            var localUniverseNumber = (ushort)(absChannelNumberStart / 510);
            var localChannelNumber = (ushort)(absChannelNumberStart % 510);

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
