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
        static void Main(string[] args)
        {
            Utility utility = new Utility();
            Thread tGenerator = new Thread(new ParameterizedThreadStart(utility.WriteRandomNumbersToFile));
            tGenerator.Start(1000);
            Thread tReader = new Thread(new ThreadStart(utility.ReadPathingValues));
            tReader.Start();
            Thread tManager = new Thread(new ThreadStart(utility.ManagerMethod));
            tManager.Start();
            tManager.Join();            
            Thread tTruckLoader = new Thread(new ThreadStart(utility.LoadTrucks));
            tTruckLoader.Start();
            //Thread tTruckStarter = new Thread(new ThreadStart(utility.StartTrucks));
            //tTruckStarter.Start();
            //Console.WriteLine();
            Console.ReadKey();
        }
    }
}
