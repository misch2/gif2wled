namespace WLEDControlApi.Domain
{
    public class E131Universe
    {
        const int MaxChannels = 512;

        public E131Universe(ushort universeNumber)
        {
            Number = universeNumber;
        }

        public ushort Number { get; set; }
        public byte[] ChannelData { get; set; } = new byte[MaxChannels];
    }
}
