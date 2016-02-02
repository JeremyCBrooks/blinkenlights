using System.Diagnostics;
using System.IO;

namespace LEDDriver
{
    public enum LED
    {
        Red = 1,
        Green = 2,
        Blue = 3
    }

    public class Driver
    {
        private static readonly string fifo = "/tmp/blink.fifo";
        private static readonly string driver = "blink.py";

        private Process driverProcess;

        public void Start()
        {
            if(driverProcess != null)
            {
                //already running, call Stop() first
                return;
            }

            //look for already running process
			try{
				var pgrep = new Process();
				pgrep.StartInfo = new ProcessStartInfo("pgrep", "-lf python")
				{
					UseShellExecute = false,
					RedirectStandardOutput = true,
					CreateNoWindow = true
				};
				pgrep.Start();
				while (!pgrep.StandardOutput.EndOfStream) {
					try{
						string line = pgrep.StandardOutput.ReadLine ();
						string[] parts = line.Split (new []{ ' ' });
						if (parts [1] == driver) {
							int processId = int.Parse (parts [0]);
							driverProcess = Process.GetProcessById (processId);
							break;
						}
					}catch{ }
				}
			}catch{ }

            if (driverProcess == null)
            {
                driverProcess = new Process();
                driverProcess.StartInfo = new ProcessStartInfo(driver)
                {
                    UseShellExecute = false,
					CreateNoWindow = true
                };
				driverProcess.EnableRaisingEvents = true;
                driverProcess.Exited += DriverProcess_Exited;
                driverProcess.Start();
            }
        }

        private void DriverProcess_Exited(object sender, System.EventArgs e)
        {
            driverProcess = null;
        }

        public bool IsRunning
        {
            get { return driverProcess != null; }
        }

        public bool Stop()
        {
			return writeValue ("x");
        }

        public bool EnableLED(LED led)
        {
			return writeValue(((int)led).ToString());
        }

        public bool DisableAllLEDs()
        {
            return writeValue("0");
        }

        private bool writeValue(string value)
        {
            if(driverProcess == null)
            {
                //driver not running
                return false;
            }

			byte[] buffer = System.Text.Encoding.UTF8.GetBytes (value);
			using (var fs = new FileStream(fifo, FileMode.Open, FileAccess.Write, FileShare.Write))
            {
				fs.Write(buffer, 0, buffer.Length);
				fs.Flush ();
            }

            return true;
        }
    }
}
