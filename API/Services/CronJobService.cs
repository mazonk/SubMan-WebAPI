using System.Net.Http;

namespace Subman.Services
{
    public class CronJobService
    {
        private readonly HttpClient _httpClient;
        private System.Timers.Timer? _timer;
        private int _durationMinutes = 60;
        private const int MINUTES_DELTA = 1;
        private readonly string _url = "https://subman-webapi.onrender.com/swagger/index.html";

        public CronJobService()
        {
            _httpClient = new HttpClient();
        }

        public void Start()
        {
            if (_timer != null && _timer.Enabled)
                return;

            // Set the interval to MINUTES_DELTA minutes in milliseconds
            _timer = new System.Timers.Timer(MINUTES_DELTA * 60 * 1000); 

            // Attach the Elapsed event handler
            _timer.Elapsed += (sender, e) => Task.Run(async () => await PingServer());

            _timer.AutoReset = true;  // Keep triggering periodically
            _timer.Start();
        }

        public void Stop()
        {
            _timer?.Stop();
            _timer = null;
        }

        private async Task PingServer()
        {
            try
            {
                Console.WriteLine($"Pinging {_url}");
                await _httpClient.GetAsync(_url);
                _durationMinutes -= MINUTES_DELTA;
                Console.WriteLine($"Minutes Left: {_durationMinutes}");

                if (_durationMinutes <= 0)
                {
                    Stop();
                    Console.WriteLine("Stopped the cron job due to inactivity");
                    _durationMinutes = 120;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ping failed: {ex.Message}");
            }
        }
    }
}
