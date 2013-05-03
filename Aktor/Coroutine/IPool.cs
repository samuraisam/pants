namespace Aktor.Coroutine
{
    public interface IPool
    {
        void Start();
        void Run();
        void Add(ICoroutine co);
        void Remove(ICoroutine co);
    }
}