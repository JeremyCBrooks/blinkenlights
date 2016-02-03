
namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var driver = new LEDDriver.Driver();

            driver.Start();
            System.Console.Out.WriteLine("Start");
            System.Threading.Thread.Sleep(1000);
            System.Console.Out.WriteLine(string.Format("Is running: {0}", driver.IsRunning));

            driver.EnableLED(LEDDriver.LED.Red);
            System.Console.Out.WriteLine("Set red");
            System.Threading.Thread.Sleep(3000);

            driver.EnableLED(LEDDriver.LED.Green);
            System.Console.Out.WriteLine("Set green");
            System.Threading.Thread.Sleep(3000);

            driver.EnableLED(LEDDriver.LED.Blue);
            System.Console.Out.WriteLine("Set blue");
            System.Threading.Thread.Sleep(3000);

            driver.DisableAllLEDs();
            System.Console.Out.WriteLine("All off");
            System.Threading.Thread.Sleep(1000);

            driver.Stop();
            System.Console.Out.WriteLine("Stop");
            System.Threading.Thread.Sleep(1000);

	    while (driver.IsRunning) {}
            System.Console.Out.WriteLine(string.Format("Is running: {0}", driver.IsRunning));
            System.Threading.Thread.Sleep(1000);
        }
    }
}
