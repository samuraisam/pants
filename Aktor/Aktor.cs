using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Aktor
{
    public enum CoroutineState
    {
        Ready,
        Sleeping,
        Stopped
    }

    public delegate CoroutineState CoroutineNext();

    public static class CoroutineStateFactory
    {
        public static readonly CoroutineNext Ready = () => { return CoroutineState.Ready; };
        public static readonly CoroutineNext Stopped = () => { return CoroutineState.Stopped; };
    }

    public delegate IEnumerable<CoroutineNext> CoroutineStart();

    public interface ICoroutine
    {
        void Start();
        CoroutineState LastKnownState { get; set; }
        IEnumerator<CoroutineNext> Executor { get; }
        CoroutineNext SuspendedContinuation { get; set; }
        void Join();
        void DidStop();
    }

    public interface IPool
    {
        void Start();
        void Run();
        void Add(ICoroutine co);
        void Remove(ICoroutine co);
    }

    public class RoundRobinPool : IPool
    {
        private Queue<ICoroutine> _queue = new Queue<ICoroutine>();
        private object _lock = new object();
        private bool _isShutdownRequested = false;
        private bool _isInterruptionRequested = false;

        public void Start()
        {
            Run();
        }

        public void Run()
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
                // do some sort of cleanup
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

    public class Coroutine : ICoroutine
    {
        private CoroutineState _lastKnownState = CoroutineState.Stopped;
        private IEnumerator<CoroutineNext> _executor;
        private CoroutineStart _start;
        private ManualResetEvent _completedWaitEvent = new ManualResetEvent(false);
        private CoroutineNext _suspendedContinuation;

        public CoroutineState LastKnownState
        {
            get { return _lastKnownState; }
            set { _lastKnownState = value; }
        }

        public IEnumerator<CoroutineNext> Executor
        {
            get { return _executor; }
        }

        public CoroutineNext SuspendedContinuation
        {
            get { return _suspendedContinuation; }
            set { _suspendedContinuation = value; }
        }

        public Coroutine(CoroutineStart start)
        {
            _start = start;
        }

        public void Start()
        {
            _executor = _start().GetEnumerator();
        }

        public void Join()
        {
            _completedWaitEvent.Set();
        }

        public void DidStop()
        {
            _completedWaitEvent.Set();
        }
    }
}
