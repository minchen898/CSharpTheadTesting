using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MonitorEnterTest
{
    class Program
    {
        static Processor processor = new Processor();
        static private List<WaitHandle> waitHandle;

        static void Main(string[] args)
        {
            waitHandle = new List<WaitHandle>();
            processor.ShowMessage += Console.WriteLine;

            for (int i = 0; i < 50; i++)
            {
                int x = i;
                AutoResetEvent resetEvent = new AutoResetEvent(false);
                waitHandle.Add(resetEvent);
                Task.Factory.StartNew(() => DoWork(x, resetEvent));
            }
            Thread.Sleep(100);

            WaitHandle.WaitAll(waitHandle.ToArray());
            Console.WriteLine("Completed");

            waitHandle.Clear();
            for (int i = 0; i < 50; i++)
            {
                int x = i;
                AutoResetEvent resetEvent = new AutoResetEvent(false);
                waitHandle.Add(resetEvent);
                Task.Factory.StartNew(() => TryDoWork(x,resetEvent));
            }
            Thread.Sleep(100);

            WaitHandle.WaitAll(waitHandle.ToArray());
            Console.WriteLine("Completed");
            Console.ReadLine();
        }

        static void DoWork(int i, AutoResetEvent resetEvent)
        {
            try
            {
                Monitor.Enter(processor);
                processor.Process(i);
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("ID: {0} Exception: {1}", i, ex.Message));
            }
            finally
            {
                Monitor.Exit(processor);
            }
            resetEvent.Set();
        }

        static void TryDoWork(int i, AutoResetEvent resetEvent)
        {
            if (Monitor.TryEnter(processor, 3000))
            {
                try
                {
                    processor.Process(i);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(string.Format("ID: {0} Exception: {1}", i, ex.Message));
                }
                finally
                {
                    Monitor.Exit(processor);
                }
            }
            resetEvent.Set();
        }
    }
	
    public class Processor
    {
        private Random random;
        private int[] numTable;
        public Action<string> ShowMessage;

        public Processor()
        {
            numTable = new int[51];
            for (int i = 0; i <= 50; i++)
            {
                numTable[i] = i;
            }
            random = new Random(DateTime.Now.Millisecond);
        }

        public void Process(int i)
        {
            int waitTime = random.Next(250, 750);

            if (ShowMessage != null)
            {
                ShowMessage(string.Format("ID: {0} Number: {1} Time: {2} Wait {3}ms", i, numTable[i], DateTime.Now.ToString("HH:mm:ss.fff"), waitTime));
            }

            Thread.Sleep(waitTime);
        }
    }
}
