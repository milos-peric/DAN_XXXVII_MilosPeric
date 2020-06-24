using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DAN_XXXVII_MilosPeric
{
    class Program
    {
        public static EventWaitHandle ready = new AutoResetEvent(false);
        public static EventWaitHandle go = new AutoResetEvent(false);
        static void Main(string[] args)
        {
            Utility utility = new Utility();
            Thread tGenerator = new Thread(new ParameterizedThreadStart(utility.WriteRandomNumbersToFile));
            tGenerator.Start(1000);
            ready.WaitOne();
            Thread tReader = new Thread(new ThreadStart(utility.ReadPathingValues));
            tReader.Start();
            go.WaitOne();
            Thread tManager = new Thread(new ThreadStart(utility.ManagerMethod));
            tManager.Start();
            ready.WaitOne();
            Thread tTruckDriver = new Thread(new ThreadStart(utility.LoadTrucks));
            tTruckDriver.Start();
            Thread tStartTrucks = new Thread(new ThreadStart(utility.StartTrucks));
            tStartTrucks.Start();
            Console.ReadKey();
        }
    }
}
