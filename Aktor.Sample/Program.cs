using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Aktor;

/**
 * An example of using the Coroutine package.
 */
namespace Aktor.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            BasicCoroutineDemo();
        }

        // -----------------------------------------------------------------------------------------
        
        static void BasicCoroutineDemo()
        {
            Debug.WriteLine("creating pool thread");
            Coroutine.IPool pool = new Coroutine.RoundRobinPool();
            Thread thread = new Thread(pool.Start);
            thread.Start();

            pool.Add(new Coroutine.Coroutine(Number1));
            pool.Add(new Coroutine.Coroutine(Number2));

            Debug.WriteLine("shutting down pool and waiting for it to finish");

            pool.Finish(true);

            Debug.WriteLine("finished shutting down pool");

            thread.Join();

            Debug.WriteLine("all done, scheduler thread exited");
        }

        static IEnumerable<Coroutine.CoroutineNext> Number1()
        {
            Debug.WriteLine("coroutine 1: starting");
            for (var i = 0; i < 100; i++) {
                Debug.WriteLine("coroutine 1: {0}", i);
                yield return Coroutine.CoroutineStateFactory.Ready;
            }
            Debug.WriteLine("coroutine 1: finished");
        }

        static IEnumerable<Coroutine.CoroutineNext> Number2()
        {
            Debug.WriteLine("coroutine 2: starting");
            bool didSleep = false;
            for (int i = 0; i < 100; i++) {
                Debug.WriteLine("coroutine 2: {0}", i);
                yield return new Coroutine.CoroutineNext(() => {
                    if (i % 2 == 0) {
                        Debug.WriteLine("coroutine 2: got even number");
                        return Coroutine.CoroutineState.Ready;
                    } else {
                        if (!didSleep) {
                            didSleep = true;
                            Debug.WriteLine("coroutine 2: got odd number... going to sleep");
                            return Coroutine.CoroutineState.Sleeping;
                        } else {
                            didSleep = false;
                            Debug.WriteLine("coroutine 2: done sleeping");
                            return Coroutine.CoroutineState.Ready;
                        }
                    }
                });
            }
            Debug.WriteLine("coroutine 2: finished");
        }
    }
}
