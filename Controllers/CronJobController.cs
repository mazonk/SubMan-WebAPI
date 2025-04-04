using Microsoft.AspNetCore.Mvc;
using Subman.Services;

namespace Subman.Controllers
{
    [ApiController]
    [Route("cron-job")]
    public class CronJobController : ControllerBase
    {
        private readonly CronJobService _cronJobService;

        public CronJobController(CronJobService cronJobService)
        {
            _cronJobService = cronJobService;
        }

        [HttpPost("start")]
        public IActionResult StartCron()
        {
            _cronJobService.Start();
            return Ok("Started cron-job from active route call");
        }

        [HttpPost("stop")]
        public IActionResult StopCron()
        {
            _cronJobService.Stop();
            return Ok("Stopped cron-job from active route call");
        }
    }
}
