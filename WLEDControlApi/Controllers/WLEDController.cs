using Microsoft.AspNetCore.Mvc;
using WLEDControlApi.Interfaces;

namespace WLEDControlApi.Controllers
{
    [ApiController]
    [Route("/")]
    public class WLEDController : ControllerBase
    {
        IWLEDService _wledService;
        IConfiguration _config;
        public WLEDController(IWLEDService wledService, IConfiguration config)
        {
            _wledService = wledService;
            _config = config;
        }

        [HttpGet]
        [Route("status")]
        public ActionResult Status()
        {
            return Ok("OK");
        }

        /// <summary>
        /// compatibility with old API: /play?gif={{ file }}&len={{ duration }}&fps={{ fps }}&host={{ host }}
        /// </summary>
        /// <param name="gif"></param>
        /// <param name="len"></param>
        /// <param name="fps"></param>
        /// <param name="host"></param>
        /// <returns></returns>
        [HttpGet]
        [HttpPost]
        [Route("play")]
        public ActionResult Play(string gif, double len, double fps, string host)
        {
            try
            {
                _wledService.PlayGifForSpecifiedTime(
                    gifName: gif,
                    durationInSeconds: len,
                    destinationHost: host,
                    fps: fps);
                return Ok();
            }
            catch (FileNotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
