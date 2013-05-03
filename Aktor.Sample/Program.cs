using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            Console.WriteLine("creating pool thread");
            Coroutine.IPool pool = new Coroutine.RoundRobinPool();
            Thread thread = new Thread(pool.Start);
            thread.Start();

            pool.Add(new Coroutine.Coroutine(Number1));
            pool.Add(new Coroutine.Coroutine(Number2));
        }

        static IEnumerable<Coroutine.CoroutineNext> Number1()
        {
            Console.WriteLine("coroutine 1: starting");
            for (var i = 0; i < 100; i++) {
                Console.WriteLine("coroutine 1: {0}", i);
                yield return Coroutine.CoroutineStateFactory.Ready;
            }
            Console.WriteLine("coroutine 1: finished");
        }

        static IEnumerable<Coroutine.CoroutineNext> Number2()
        {
            Console.WriteLine("coroutine 2: starting");
            bool didSleep = false;
            for (int i = 0; i < 100; i++) {
                Console.WriteLine("coroutine 2: {0}", i);
                yield return new Coroutine.CoroutineNext(() => {
                    if (i % 2 == 0) {
                        Console.WriteLine("coroutine 2: got even number");
                        return Coroutine.CoroutineState.Ready;
                    } else {
                        if (!didSleep) {
                            didSleep = true;
                            Console.WriteLine("coroutine 2: got odd number... going to sleep");
                            return Coroutine.CoroutineState.Sleeping;
                        } else {
                            didSleep = false;
                            Console.WriteLine("coroutine 2: done sleeping");
                            return Coroutine.CoroutineState.Ready;
                        }
                    }
                });
            }
            Console.WriteLine("coroutine 2: finished");
        }
    }
}
