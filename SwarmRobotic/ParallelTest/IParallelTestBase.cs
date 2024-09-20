using System;
namespace ParallelTest
{
    interface IParallelTestBase
    {
        void AddThread();
        event Action<string> ErrorThrown;
        event Action GoLast;
        bool isRunning { get; }
        void Pause();
        void RemoveThread();
        int RequireThread { get; }
        void Resume();
        int RunningThread { get; }
        void Start(int threads);
        void Stop();
        event Action<int> TaskCreated;
        event Action<int> ThreadsChanged;
        int TimeStep { get; set; }
    }
}
