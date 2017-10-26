using System;

namespace Walterlv.Threading
{
    public class CustomAwaitable : IAwaitable<CustomAwaiter>
    {
        public CustomAwaiter GetAwaiter()
        {
            return new CustomAwaiter();
        }
    }

    public class CustomAwaiter : IAwaiter
    {
        public bool IsCompleted { get; }

        public void GetResult()
        {
        }
        public void OnCompleted(Action continuation)
        {
        }
    }
}
