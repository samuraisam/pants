using System.Collections.Generic;

namespace Aktor.Coroutine
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
}