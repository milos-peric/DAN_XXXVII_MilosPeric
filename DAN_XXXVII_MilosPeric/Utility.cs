using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DAN_XXXVII_MilosPeric
{
    class Utility
    {
        private const string pathingFileName = @"..\..\PathingValues.txt";
        private static List<int> listOfPathingValues = new List<int>();
        private List<int> bestPaths = new List<int>();
        private List<int> loadTimes = new List<int>();
        private static Semaphore _pool = new Semaphore(2, 2);
        private static int _taskDelay;
        private static Random random = new Random();
        private readonly object objLock = new object();
        private List<Thread> threadList = new List<Thread>();
        private static Semaphore _pool2 = new Semaphore(1, 10);

        public void WriteRandomNumbersToFile(object amount)
        {
            try
            {
                lock (objLock)
                {
                    while (!File.Exists(pathingFileName))
                    {
                        Monitor.Wait(objLock);
                    }
                    Console.WriteLine("Writing numbers to file....");
                    using (StreamWriter streamWriter = new StreamWriter(pathingFileName))
                    {
                        Random random = new Random();
                        for (int i = 0; i < (int)amount; i++)
                        {
                            streamWriter.WriteLine(random.Next(1, 5001));
                        }
                    }
                    if (File.Exists(pathingFileName))
                    {
                        Console.WriteLine("Numbers written to file succesfully.");
                    }
                    Monitor.Pulse(objLock);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Write to file is not possible.");
                Console.WriteLine(e.Message);
            }
        }

        public void ManagerMethod()
        {
            lock (objLock)
            {
                while (listOfPathingValues.Count != 1000)
                {
                    Monitor.Wait(objLock, 3000);
                }
                List<int> pathingList = new List<int>();
                foreach (var item in listOfPathingValues)
                {
                    if (item % 3 == 0)
                    {
                        pathingList.Add(item);
                    }
                }
                pathingList = pathingList.OrderBy(x => x).Distinct().ToList();
                for (int i = 0; i < 10; i++)
                {
                    bestPaths.Add(pathingList.ElementAt(i));
                }
                Console.WriteLine("Best paths aquired.");
                Monitor.Pulse(objLock);
            }
        }

        public void StartTrucks()
        {            
            for (int i = 1; i <= 10; i++)
            {
                Thread t = new Thread(new ParameterizedThreadStart(Worker2));
                t.Name = bestPaths[i - 1].ToString();
                t.Start(i);
            }
        }

        public void LoadTrucks()
        {
            Console.WriteLine("Starting loading of the trucks.");
            lock (objLock)
            {
                while (bestPaths.Count != 10)
                {
                    Monitor.Wait(objLock);
                }
                for (int i = 1; i <= 10; i++)
                {
                    Thread tTruck = new Thread(new ParameterizedThreadStart(Worker));
                    threadList.Add(tTruck);
                    tTruck.Start(i);
                }
                Monitor.Pulse(objLock);
            }
        }

        public void UnloadTrucks(int truckNumber)
        {
            Console.WriteLine("Truck {0} starts unloading.", truckNumber);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            double unloadTime = loadTimes.ElementAt(truckNumber - 1) * 0.5;
            Thread.Sleep((int)unloadTime);
            Console.WriteLine("Truck {0} finished unloading. Unload time: {1:N2} seconds.", truckNumber, stopwatch.Elapsed.TotalSeconds);
        }

        public void Worker2(object num)
        {
            Stopwatch stopwatch = new Stopwatch();
            Console.WriteLine("Truck {0} starts trip to destination code: {1}.", num, Thread.CurrentThread.Name);
            
            _pool2.WaitOne();
            stopwatch.Start();
            _taskDelay = random.Next(500, 5001);           
            //Console.WriteLine("Task delay {0} T:{1}", _taskDelay, num);
            _pool2.Release();
            if (_taskDelay > 3000)
            {
                Thread.Sleep(_taskDelay);
                Console.WriteLine("Truck {0} could not reach destination code: {1} in time. Returning truck.", num, Thread.CurrentThread.Name);
                Thread.Sleep(_taskDelay);
                Console.WriteLine("Truck {0} returned from failed trip destination code: {2}. Trip time: {1:N2} seconds.", num, stopwatch.Elapsed.TotalSeconds, Thread.CurrentThread.Name);
                stopwatch.Reset();
            }
            else
            {
                Thread.Sleep(_taskDelay);
                Console.WriteLine("Truck {0} finishes trip to destination code: {1}. Trip time: {2:N2} seconds."
                    , num, Thread.CurrentThread.Name, stopwatch.Elapsed.TotalSeconds);               
                UnloadTrucks((int)num);
                stopwatch.Reset();
            }              
        }

    public void Worker(object num)
    {
        Stopwatch stopwatch = new Stopwatch();
        _pool.WaitOne();
        stopwatch.Start();
        Console.WriteLine("Truck {0} has started loading.", num);
        _taskDelay = random.Next(500, 5001);
        loadTimes.Add(_taskDelay);
        Thread.Sleep(_taskDelay);
        Console.WriteLine("Truck {0} has finished loading. Loading time: {1:N2} seconds.", num, stopwatch.Elapsed.TotalSeconds);
        stopwatch.Reset();
        _pool.Release();
        if ((int)num == 10)
        {
            lock (objLock)
            {
                Console.WriteLine("Loading finished.");
                Console.WriteLine("Trucks starting path to destination...");
                Thread.Sleep(_taskDelay);                
                StartTrucks();
            }
        }
    }

    public void ReadPathingValues()
    {
        try
        {
            lock (objLock)
            {
                using (StreamReader streamReader = new StreamReader(pathingFileName))
                {
                    string line;
                    int number;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        number = Convert.ToInt32(line);
                        listOfPathingValues.Add(number);
                    }
                }
                Monitor.Pulse(objLock);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Can't read from {0} file or file doesn't exist.", pathingFileName);
            Console.WriteLine(e.Message);
        }
    }
}
}
