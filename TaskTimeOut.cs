using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace TaskTimeOut
{
    class Program
    {
        static void Main(string[] args)
        {
            int n = 0;
            NumQueue queue = new NumQueue(5);
            queue.Message += (message) => { Console.WriteLine(message); };

            do
            {
                n++;

                #region Start async task to fetch data and send to server

                Task.Factory.StartNew(() =>
                {
                    int id = n;
                    //Console.WriteLine(id + " start");

                   // CancellationTokenSource tokenSource = new CancellationTokenSource();

                    Task task = new Task(
                        //state =>
                        ()=>
                        {
                            //var token = (CancellationToken)state;

                            int? r = queue.Get(id);

                            //if (!token.IsCancellationRequested)
                            if (r != null)
                            {
                                if (id % 3 == 0)
                                    Thread.Sleep(5000);
                                else
                                    Thread.Sleep(800);
                                queue.Return(r.Value);
                                //Console.WriteLine(id + ": End @ " + DateTime.Now.ToLongTimeString());
                            }
                        });
                        //}, tokenSource.Token, tokenSource.Token);

                    //Console.WriteLine(id + ": Count = " + queue.Count);
                    task.Start();
                    bool hasResult = task.Wait(8000);//, tokenSource.Token);
                    if (!hasResult)
                    {
                        //Console.WriteLine(id + ": Canceled");
                        //tokenSource.Cancel();
                    }
                    //Console.WriteLine(id + " result: " + hasResult);
                }).ContinueWith(t =>
                {
                    AggregateException ae = t.Exception;
                    foreach (Exception ex in ae.InnerExceptions)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }, TaskContinuationOptions.OnlyOnFaulted);

                #endregion

                Thread.Sleep(500);
            }
            while (true);
        }
    }
	
    public class NumQueue
    {
        private readonly ConcurrentQueue<int> _numQueue;
        private readonly AutoResetEvent autoEvent = new AutoResetEvent(false);

        public Action<string> Message;

        public int Count { get { return _numQueue.Count; } }

        public NumQueue(int count)
        {
            _numQueue = new ConcurrentQueue<int>();
            for (int i = 1; i <= count; i++)
                _numQueue.Enqueue(i);
        }

        public int? Get(int id)
        {
            int result = 0;
            while (result == 0)
            {
                if (!_numQueue.TryDequeue(out result))
                {
                    if (Message != null)
                        Message(string.Format("{0} waiting", id));
                    autoEvent.Reset();
                    if (!WaitHandle.WaitAll(new WaitHandle[] { autoEvent }, 500))
                    {
                        Message(string.Format("{0} stop waiting", id));
                        return null;
                    }
                }
            }

            if (Message != null)
                Message(string.Format("{0} get {1}", id, result));

            return result;
        }

        public void Return(int i)
        {
            _numQueue.Enqueue(i);
            autoEvent.Set();
        }
    }
}
