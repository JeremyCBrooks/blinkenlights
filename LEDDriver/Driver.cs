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
        private static readonly string fifo = "blink.fifo";
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
            var processes = Process.GetProcesses();
            foreach (var process in processes)
            {
                if (process.ProcessName == driver)
                {
                    driverProcess = process;
                    break;
                }
            }

            if (driverProcess == null)
            {
                driverProcess = new Process();
                driverProcess.StartInfo = new ProcessStartInfo(driver)
                {
                    UseShellExecute = false,
                };
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
            return writeValue("x");
        }

        public bool EnableLED(LED led)
        {
            return writeValue(led.ToString());
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

            using (var fs = File.OpenWrite(fifo))
            using (var sr = new StreamWriter(fs))
            {
                sr.Write(value);
            }

            return true;
        }
    }
}
