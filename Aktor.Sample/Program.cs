using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Aktor.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("creating pool thread");
            Aktor.IPool pool = new Aktor.RoundRobinPool();
            Thread thread = new Thread(pool.Start);
            thread.Start();

            pool.Add(new Coroutine(Number1));
            pool.Add(new Coroutine(Number2));
        }

        static IEnumerable<Aktor.CoroutineNext> Number1()
        {
            Console.WriteLine("coroutine 1: starting");
            for (var i = 0; i < 100; i++) {
                Console.WriteLine("coroutine 1: {0}", i);
                yield return Aktor.CoroutineStateFactory.Ready;
            }
            Console.WriteLine("coroutine 1: finished");
        }

        static IEnumerable<Aktor.CoroutineNext> Number2()
        {
            Console.WriteLine("coroutine 2: starting");
            //bool sleeping = false;
            //for (var i = 0; i < 100; i++) {
            //    if (i % 2) {
            //    Console.WriteLine("coroutine 2: {0}", i);
            //    yield return Aktor.CoroutineStateFactory.Ready;
            //}
            bool didSleep = false;
            for (int i = 0; i < 100; i++) {
                Console.WriteLine("coroutine 2: {0}", i);
                yield return new Aktor.CoroutineNext(() => {
                    if (i % 2 == 0) {
                        Console.WriteLine("coroutine 2: got even number");
                        return Aktor.CoroutineState.Ready;
                    } else {
                        if (!didSleep) {
                            didSleep = true;
                            Console.WriteLine("coroutine 2: got odd number... going to sleep");
                            return Aktor.CoroutineState.Sleeping;
                        } else {
                            didSleep = false;
                            Console.WriteLine("coroutine 2: done sleeping");
                            return Aktor.CoroutineState.Ready;
                        }
                    }
                });
            }
            Console.WriteLine("coroutine 2: finished");
        }
    }
}
