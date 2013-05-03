using System.Collections.Generic;
using System.Threading;

namespace Pants.Coroutine
{
    public class RoundRobinPool : IPool
    {
        private Queue<ICoroutine> _queue = new Queue<ICoroutine>();
        private object _lock = new object();
        private bool _isShutdownRequested = false;
        private bool _isInterruptionRequested = false;
        private ManualResetEvent _quitEvent = new ManualResetEvent(false);

        public void Start()
        {
            Run();
        }

        private void Run()
        {
            try {
                bool exitRequested = false;

                while (!exitRequested) {
                    ICoroutine co = null;

                    while (co == null && !_isInterruptionRequested) {
                        lock (_lock) {
                            if (_queue.Count == 0) {
                                if (_isShutdownRequested) {
                                    break;
                                } else {
                                    Monitor.Wait(_lock, 250);
                                }
                            } else {
                                co = _queue.Dequeue();
                            }
                        }
                    }

                    if (co == null) {
                        exitRequested = true;
                        continue;
                    }

                    if (co.LastKnownState == CoroutineState.Sleeping) {
                        co.LastKnownState = co.SuspendedContinuation();
                        if (co.LastKnownState == CoroutineState.Stopped) {
                            co.DidStop();
                            continue;
                        }
                    }

                    if (co.LastKnownState != CoroutineState.Sleeping) {
                        if (co.Executor.MoveNext()) {
                            co.SuspendedContinuation = co.Executor.Current;
                            co.LastKnownState = co.SuspendedContinuation();
                        } else {
                            co.SuspendedContinuation = CoroutineStateFactory.Stopped;
                            co.LastKnownState = CoroutineState.Stopped;
                        }
                        if (co.LastKnownState == CoroutineState.Stopped) {
                            co.DidStop();
                            continue;
                        }
                    }

                    lock (_lock) {
                        _queue.Enqueue(co);
                    }
                }
            } finally {
                _quitEvent.Set();
            }
        }

        public void Finish(bool waitUntilDone)
        {
            lock (_lock) {
                _isShutdownRequested = true;
            }
            if (waitUntilDone) {
                _quitEvent.WaitOne();
            }
        }

        public void Add(ICoroutine co)
        {
            if (co.LastKnownState == CoroutineState.Stopped) {
                co.Start();
            }
            lock (_lock) {
                _queue.Enqueue(co);
                Monitor.PulseAll(_lock);
            }
        }

        public void Remove(ICoroutine co)
        {
            lock (_lock) {
                Queue<ICoroutine> copy = new Queue<ICoroutine>(_queue.Count * 2);
                while (_queue.Count != 0) {
                    ICoroutine checkCo = _queue.Dequeue();
                    if (!checkCo.Equals(co)) {
                        copy.Enqueue(co);
                    }
                }
                _queue = copy;
            }
        }
    }
}