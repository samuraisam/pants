using System.Collections.Generic;
using System.Threading;

namespace Aktor.Coroutine
{
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