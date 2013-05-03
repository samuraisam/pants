using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Pants;

/**
 * An example of using the Coroutine package.
 */
namespace Pants.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            BasicCoroutineDemo();
            JoinedCoroutineDemo();
        }

        #region basiccoroutine

        static void BasicCoroutineDemo()
        {
            Debug.WriteLine("----------------------------------------------------------------------------");
            Debug.WriteLine("creating pool thread");
            Coroutine.IPool pool = new Coroutine.RoundRobinPool();
            Thread thread = new Thread(pool.Start);
            thread.Start();

            pool.Add(new Coroutine.Coroutine(Number1));
            pool.Add(new Coroutine.Coroutine(Number2));

            Debug.WriteLine("shutting down pool and waiting for it to finish");

            pool.Join(true);

            Debug.WriteLine("finished shutting down pool");

            thread.Join();

            Debug.WriteLine("all done, scheduler thread exited");
            Debug.WriteLine("----------------------------------------------------------------------------");
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

        #endregion

        #region joinedcoroutine

        static void JoinedCoroutineDemo()
        {
            Debug.WriteLine("----------------------------------------------------------------------------");
            Debug.WriteLine("creating pool thread");
            Coroutine.IPool pool = new Coroutine.RoundRobinPool();
            Thread thread = new Thread(pool.Start);
            thread.Start();

            Coroutine.ICoroutine co = new Coroutine.Coroutine(() => new ObjectCoroutine());

            pool.Add(co);

            Debug.WriteLine("joining coroutine");
            co.Join();
            Debug.WriteLine("coroutine has been joined");

            Debug.WriteLine("Joining pool");

            pool.Join(true);

            Debug.WriteLine("finished shutting down pool");

            thread.Join();

            Debug.WriteLine("all done, scheduler thread exited");
            Debug.WriteLine("----------------------------------------------------------------------------");
        }

        class ObjectCoroutine : IEnumerable<Coroutine.CoroutineNext>
        {
            public IEnumerator<Coroutine.CoroutineNext> GetEnumerator()
            {
                Debug.WriteLine("ObjCo began");
                yield return Coroutine.CoroutineStateFactory.Ready;
                Debug.WriteLine("ObjCo is finishing");
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                // Lets call the generic version here
                return this.GetEnumerator();
            }
        }

        #endregion

        //class MyActor : Actor
        //{
        //    public void Receive(Matcher m)
        //    {
        //        m.Match(typeof(Message<IMessageType>), (Message<IMessageType> message) => {
        //        });
        //    }
        //}
    }
}
