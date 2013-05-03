namespace Pants.Coroutine
{
    public interface IPool
    {
        /// <summary>
        /// Starts the scheduler. This is a blocking call! It will run forever, or until Finish(bool) is called.
        /// </summary>
        void Start();

        /// <summary>
        /// Schedules a coroutine.
        /// </summary>
        /// <param name="co">The coroutine to schedule.</param>
        void Add(ICoroutine co);

        /// <summary>
        /// Unschedule a coroutine. WARNING: this is currently an unreasonably expensive operation.
        /// </summary>
        /// <param name="co">The coroutine to unschedule.</param>
        void Remove(ICoroutine co);

        /// <summary>
        /// Exits the scheduling loop and exhausts the pool. This may method may never exit because a coroutine 
        /// may continually schedule itself or another coroutine.
        /// </summary>
        /// <param name="waitUntilDone">If this is true then we'll block until the scheduler is exhausted.</param>
        void Join(bool waitUntilDone);
    }
}