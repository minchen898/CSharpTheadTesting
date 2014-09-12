using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ManyThread
{
    class Program
    {
        static string _message;
        static void Main(string[] args)
        {
            _message = "";

            AutoResetEvent resetEvent = new AutoResetEvent(false);
            int total = 0;

            List<WaitHandle> waitHandle = new List<WaitHandle>();

            for (int i = 1; i <= 100; i++)
            {
                total++;
                int id = i;
                AutoResetEvent indivEvent = new AutoResetEvent(false);
                waitHandle.Add(indivEvent);
                Task.Factory.StartNew(() => doWork(id, ref total, resetEvent, indivEvent));
            }

            waitHandle.ForEach((x) => x.WaitOne());
            resetEvent.WaitOne();
        }

        static private void doWork(int workId, ref int totalWork, AutoResetEvent resetEvent, AutoResetEvent indivEvent)
        {
            try
            {
                _message += "ID: " + workId + " Total: " + totalWork + Environment.NewLine;
                Console.WriteLine("ID: " + workId + " Total: " + totalWork);
            }
            finally
            {
                if (Interlocked.Decrement(ref totalWork) == 0)
                    resetEvent.Set();
                indivEvent.Set();
            }
        }
    }
}
