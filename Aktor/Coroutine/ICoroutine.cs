using System.Collections.Generic;

namespace Aktor.Coroutine
{
    /// <summary>
    /// This is all you need to know to write Coroutines. It's a delegate that yields the current state of the
    /// computation. The ICoroutine interface (and any implementations) are likely not very useful to you since
    /// they exist just to support the yield interface to concurrency.
    /// 
    /// For example (pseudo-code):
    /// <code>
    ///     IEnumerable<Coroutine.CoroutineNext> MyCoroutine()
    ///     {
    ///         Buffer bytes = new Buffer();
    ///         string endOfMessage = "\r\n";
    ///         while (bytes.read(buffer, 1024) != EOF) {
    ///             if (buffer.Slice(-2) == endOfMessage) {
    ///                 break;
    ///             }
    ///             yield return Coroutine.CoroutineStateFactory.Ready;
    ///         }
    ///         DoSomethingWithMessage(bytes.StringValue())
    ///     }
    /// </code>
    /// 
    /// What the above will do is yield to the scheduler every read so that other coroutines may do work.
    /// 
    /// It is important to note that the object that you ultimately yield is itself a delegate. Calling it
    /// returns one of the below CoroutineStates. This can become handy if you need to check some state 
    /// asynchronoulsy the next time the coroutine comes up for scheduling.
    /// </summary>
    /// <returns>The return value is a side effect of using `yield`</returns>
    public delegate IEnumerable<CoroutineNext> CoroutineStart();

    /// <summary>
    /// Which state is a coroutine in. You return one of these from the `CoroutineNext` which is yielded
    /// to the scheduler in `yield return`.
    /// </summary>
    public enum CoroutineState
    {
        Ready,
        Sleeping,
        Stopped
    }

    /// <summary>
    /// The delegate which is yielded inside your coroutine which returns the current state. It is handy to
    /// just use CoroutineStateFactory
    /// </summary>
    /// <returns></returns>
    public delegate CoroutineState CoroutineNext();

    /// <summary>
    /// Pre-baked CoroutineNext delegates for your convenience.
    /// </summary>
    public static class CoroutineStateFactory
    {
        public static readonly CoroutineNext Ready = () => { return CoroutineState.Ready; };
        public static readonly CoroutineNext Stopped = () => { return CoroutineState.Stopped; };
    }

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