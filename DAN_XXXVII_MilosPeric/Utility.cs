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
        private static Semaphore _pool2 = new Semaphore(1, 10);
        private static int counter = 0;

        /// <summary>
        /// Writes random set of numbers 1 to 5000 to textual file.
        /// After file is generated, method signals to other threads using same locking object by using Pulse().
        /// </summary>
        /// <param name="amount">Amount of numbers to be generated</param>
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

        /// <summary>
        /// Method selects lowest numbers divisible by 3 from list generated from Textual file.
        /// Waits until list is filled maximum of 3 seconds. 
        /// Adds 10 numbers under mentioned condition to new list in ascending order.
        /// Signals after finished working via Pulse().
        /// </summary>
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
                Console.WriteLine("Best pathways aquired by manager. Sending coordinates to drivers...");
                Monitor.Pulse(objLock);
            }
        }

        /// <summary>
        /// Creates 10 threads and starts them, calls Worker2 method. Each thread is named by one of the optimal destination routes.
        /// </summary>
        public void StartTrucks()
        {
            for (int i = 1; i <= 10; i++)
            {
                Thread t = new Thread(new ParameterizedThreadStart(Worker2));
                t.Name = bestPaths[i - 1].ToString();
                t.Start(i);
            }
        }

        /// <summary>
        /// After list of optimal routes is filled, Creates and Starts 10 threads. Each thread is named by one
        /// of the optimal routes from the list.
        /// </summary>
        public void LoadTrucks()
        {
            Console.WriteLine("Best pathways recieved by drivers. Started loading of the trucks...");
            lock (objLock)
            {
                while (bestPaths.Count != 10)
                {
                    Monitor.Wait(objLock);
                }
                for (int i = 1; i <= 10; i++)
                {
                    Thread tTruck = new Thread(new ParameterizedThreadStart(Worker));
                    tTruck.Name = bestPaths[i - 1].ToString();
                    tTruck.Start(i);
                }
                Monitor.Pulse(objLock);
            }
        }

        /// <summary>
        /// Method is called after truck either arrives at the destination or fails to rech it.
        /// Unloading time is 1.5 times faster than Loading times.
        /// </summary>
        /// <param name="truckNumber">Number of truck that gets unloaded.</param>
        public void UnloadTrucks(int truckNumber)
        {
            Console.WriteLine("Truck {0} starts unloading.", truckNumber);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            double unloadTime = loadTimes.ElementAt(truckNumber - 1) * 0.5;
            Thread.Sleep((int)unloadTime);
            Console.WriteLine("Truck {0} finished unloading. Unload time: {1:N2} seconds.", 
                truckNumber, stopwatch.Elapsed.TotalSeconds);
        }

        /// <summary>
        /// Method contains logic of random trip times and subsequent actions depending on random delay time.
        /// </summary>
        /// <param name="num">Number of thread currently executing method.</param>
        public void Worker2(object num)
        {
            Stopwatch stopwatch = new Stopwatch();
            Console.WriteLine("Truck {0} starts trip to destination via route: {1}.", num, Thread.CurrentThread.Name);
            _pool2.WaitOne();
            stopwatch.Start();
            _taskDelay = random.Next(500, 5001);
            _pool2.Release();
            if (_taskDelay > 3000)
            {
                Thread.Sleep(_taskDelay);
                Console.WriteLine("Truck {0} could not reach destination via route: {1} in time {2:N2} seconds. Returning truck.", 
                    num, Thread.CurrentThread.Name, stopwatch.Elapsed.TotalSeconds);
                Thread.Sleep(_taskDelay);
                Console.WriteLine("Truck {0} returned from failed trip - destination route: {2}. Trip time: {1:N2} seconds.", 
                    num, stopwatch.Elapsed.TotalSeconds, Thread.CurrentThread.Name);
                UnloadTrucks((int)num);
                stopwatch.Reset();
            }
            else
            {
                Thread.Sleep(_taskDelay);
                Console.WriteLine("Truck {0} finishes trip to destination via route: {1}. Trip time: {2:N2} seconds.",
                    num, Thread.CurrentThread.Name, stopwatch.Elapsed.TotalSeconds);
                UnloadTrucks((int)num);
                stopwatch.Reset();
            }
        }

        /// <summary>
        /// Simulates truck loading by generating random delay times.
        /// Uses semaphores and counter to synchronize threads in such way that only two threads can be active 
        /// at the same time.
        /// </summary>
        /// <param name="num">Number of thread currently executing method.</param>
        public void Worker(object num)
        {
            Stopwatch stopwatch = new Stopwatch();
            _pool.WaitOne();           
            stopwatch.Start();
            Console.WriteLine("Truck {0} has started loading.", num);
            _taskDelay = random.Next(500, 5001);
            loadTimes.Add(_taskDelay);
            Thread.Sleep(_taskDelay);
            counter++;
            Console.WriteLine("Truck {0} has finished loading. Loading time: {1:N2} seconds.", 
                num, stopwatch.Elapsed.TotalSeconds);
            stopwatch.Reset();
            if ((int)num == 10)
            {
                lock (objLock)
                {
                    Console.WriteLine("Loading finished.");
                    Console.WriteLine("Trucks starting path to destination...");
                    //Thread.Sleep(_taskDelay);
                    StartTrucks();
                }
            }
            if (counter > 1)
            {
                counter = 0;
                _pool.Release(2);
            }
            
        }

        /// <summary>
        /// Reads generated pathing values from file into List<int>.
        /// </summary>
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

