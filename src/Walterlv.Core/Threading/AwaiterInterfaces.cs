using System.Runtime.CompilerServices;

namespace Walterlv.Threading
{
    /// <summary>
    /// 表示一个可等待对象，如果一个方法返回此类型的实例，则此方法可以使用 `await` 异步等待。
    /// </summary>
    /// <typeparam name="TAwaiter">用于给 await 确定返回时机的 IAwaiter 的实例。</typeparam>
    public interface IAwaitable<out TAwaiter> where TAwaiter : IAwaiter
    {
        TAwaiter GetAwaiter();
    }

    /// <summary>
    /// 表示一个包含返回值的可等待对象，如果一个方法返回此类型的实例，则此方法可以使用 `await` 异步等待返回值。
    /// </summary>
    /// <typeparam name="TAwaiter">用于给 await 确定返回时机的 IAwaiter{<typeparamref name="TResult"/>} 的实例。</typeparam>
    /// <typeparam name="TResult">异步返回的返回值类型。</typeparam>
    public interface IAwaitable<out TAwaiter, out TResult> where TAwaiter : IAwaiter<TResult>
    {
        TAwaiter GetAwaiter();
    }

    /// <summary>
    /// 用于给 await 确定异步返回的时机。
    /// </summary>
    public interface IAwaiter : INotifyCompletion
    {
        bool IsCompleted { get; }

        void GetResult();
    }

    /// <summary>
    /// 当执行关键代码（此代码中的错误可能给应用程序中的其他状态造成负面影响）时，
    /// 用于给 await 确定异步返回的时机。
    /// </summary>
    public interface ICriticalAwaiter : IAwaiter, ICriticalNotifyCompletion
    {
    }

    /// <summary>
    /// 用于给 await 确定异步返回的时机，并获取到返回值。
    /// </summary>
    /// <typeparam name="TResult">异步返回的返回值类型。</typeparam>
    public interface IAwaiter<out TResult> : INotifyCompletion
    {
        bool IsCompleted { get; }

        TResult GetResult();
    }

    /// <summary>
    /// 当执行关键代码（此代码中的错误可能给应用程序中的其他状态造成负面影响）时，
    /// 用于给 await 确定异步返回的时机，并获取到返回值。
    /// </summary>
    /// <typeparam name="TResult">异步返回的返回值类型。</typeparam>
    public interface ICriticalAwaiter<out TResult> : IAwaiter<TResult>, ICriticalNotifyCompletion
    {
    }
}
