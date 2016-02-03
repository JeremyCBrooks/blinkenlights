using System.Timers;
using Leeroy;
using System.Threading.Tasks;
using LEDDriver;
using System.Threading;

using Timer = System.Timers.Timer;

namespace Test
{
    public class JenkinsPoller
    {
        private Jenkins _jenkins;
        private string _jobName;
        private Driver _driver = new Driver();
        private Timer _timer;
        private AutoResetEvent _event = new AutoResetEvent(true);

        public JenkinsPoller(string hostAndPort, string jobName, double interval)
        {
            _jenkins = new Jenkins(hostAndPort);
            _jobName = jobName;

            _timer = new Timer(interval);
            _timer.Elapsed += async (s, e) => { await Poll(s, e); };
        }

        public async void Start()
        {
            await Poll(this, null);
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        private async Task Poll(object sender, ElapsedEventArgs args)
        {
            // If we are already in a poll call, wait until the next interval to try again.
            if (!_event.WaitOne(0))
            {
                return;
            }

            try
            {
                Jenkins.State state = await _jenkins.CheckState(_jobName);
                switch (state)
                {
                    case Jenkins.State.Succeeded:
                        _driver.EnableLED(LED.Green);
                        break;

                    case Jenkins.State.Failed:
                        _driver.EnableLED(LED.Red);
                        break;

                    case Jenkins.State.RebuildingAfterFailed:
                        _driver.EnableLED(LED.Blue);
                        break;
                }
            }
            finally
            {
                _event.Set();
            }
        }
    }
}
