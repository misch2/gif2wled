using WLEDControlApi.Interfaces;
using System.Text.RegularExpressions;
using System.Drawing;
using System;
using System.IO;
using System.Drawing.Imaging;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using WLEDControlApi.Domain;
using Haukcode.sACN;
using Haukcode.sACN.Model;
using System.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WLEDControlApi.Services
{
    public class WLEDService : IWLEDService
    {
        readonly Guid acnSourceId = new Guid("38FC4E60-D4B9-4577-ABC6-931A341899B9");
        readonly string acnSourceName = "WLEDControlApi";

        ILogger<WLEDService> _logger;
        IConfiguration _config;
        IDNSCacheService _dnsCacheService;
        IChannelMapper _channelMapper;

        public WLEDService(
            ILogger<WLEDService> logger,
            IConfiguration config,
            IDNSCacheService dnsCacheService,
            IChannelMapper channelMapper
        )
        {
            _logger = logger ?? throw new ArgumentNullException("logger can't be null");
            _config = config ?? throw new ArgumentNullException("config can't be null");
            _dnsCacheService = dnsCacheService ?? throw new ArgumentNullException("dnsCacheService can't be null");
            _channelMapper = channelMapper ?? throw new ArgumentNullException("channelMapper can't be null");
        }

        public void PlayGifForSpecifiedTime(string gifBaseNameWithoutExtension, double durationInSeconds, string destinationHost, double fps = 25)
        {
            if (Regex.IsMatch(gifBaseNameWithoutExtension, @"[^a-zA-Z0-9\-/_]"))
            {
                throw new ArgumentException($"Invalid characters in gif path: {gifBaseNameWithoutExtension}");
            }
            if (durationInSeconds <= 0)
            {
                throw new ArgumentOutOfRangeException("Invalid duration, must be > 0");
            }
            if (fps <= 0)
            {
                throw new ArgumentOutOfRangeException("Invalid fps, must be > 0");
            }
            if (destinationHost == null)
            {
                throw new ArgumentNullException("Missing E131 host");
            }

            _logger.LogDebug("here 1");
            var gifFullPath = _config["Images:StoragePath"]?.TrimEnd('/') + "/" + gifBaseNameWithoutExtension + ".gif";
            if (!File.Exists(gifFullPath))
            {
                _logger.LogWarning($"GIF '{gifBaseNameWithoutExtension}' not found at location: {gifFullPath}");
                throw new FileNotFoundException($"GIF image '{gifBaseNameWithoutExtension}' not found in storage");
            }

            _logger.LogInformation($"Playing GIF {gifBaseNameWithoutExtension} on {destinationHost} for {durationInSeconds} seconds at {fps} fps");

            Image gifImg = Image.FromFile(gifFullPath);
            _logger.LogDebug("here 4");
            FrameDimension dimension = new FrameDimension(gifImg.FrameDimensionsList[0]);

            _logger.LogDebug("here 5");
            var destinationHostIP = _dnsCacheService.GetIPAddress(destinationHost);

            _logger.LogDebug("here 6");
            var sendClient = new SACNClient(
                senderId: acnSourceId,
                senderName: acnSourceName,
                localAddress: IPAddress.Any,
                port: _config.GetValue<int>("E131:Port")
                );


            var startTime = DateTime.Now;
            while (DateTime.Now - startTime < TimeSpan.FromSeconds(durationInSeconds))
            {
                // Number of frames
                int frameCount = gifImg.GetFrameCount(dimension);
                _logger.LogDebug("here 7");
                _logger.LogInformation($"Number of frames: {frameCount}, size: {gifImg.Width} x {gifImg.Height}");
                // Return an Image at a certain index
                for (int index = 0; index < frameCount; index++)
                {
                    gifImg.SelectActiveFrame(dimension, index);

                    // get the frame content as RGB data
                    var frame = new Bitmap(gifImg.Width, gifImg.Height);
                    Graphics.FromImage(frame).DrawImage(gifImg, Point.Empty);

                    ushort startingUniverseNumber = 1;
                    var universes = MapBitmapToUniverses(frame, startingUniverseNumber);

                    _logger.LogDebug("here 9");
                    // send the frames
                    foreach (var universe in universes)
                    {
                        sendClient.SendUnicast(destinationHostIP, universe.Number, universe.ChannelData);
                    }

                    // wait for the next frame
                    Thread.Sleep((int)(1000 / fps));
                }
            }

            _logger.LogInformation($"Finished playing GIF {gifBaseNameWithoutExtension}");
        }

        public List<E131Universe> MapBitmapToUniverses(Bitmap bitmap, ushort startingUniverseNumber)
        {
            var universes = new List<E131Universe>();

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    var color = bitmap.GetPixel(x, y);

                    var subPixelInfo = _channelMapper.MapPixel(color, x, y, bitmap.Width, bitmap.Height);
                    InsertSubPixelIntoUniverses(universes, subPixelInfo, startingUniverseNumber);
                }
            }

            return universes;
        }

        public void InsertSubPixelIntoUniverses(List<E131Universe> universes, IEnumerable<E131SubPixelInfo> subPixelInfo, ushort startingUniverseNumber)
        {
            foreach (var subPixel in subPixelInfo)
            {
                var universeNumber = (ushort)(subPixel.Universe + startingUniverseNumber);

                var universe = universes.FirstOrDefault(u => u.Number == universeNumber);
                if (universe == null)
                {
                    universe = new E131Universe(universeNumber);
                    universes.Add(universe);
                }

                universe.ChannelData[subPixel.Channel] = subPixel.Value;
            }
        }
    }
}
