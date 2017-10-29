﻿using System;
using System.Runtime.ExceptionServices;
using System.Windows.Threading;
using Walterlv.Annotations;
using Walterlv.Threading;

namespace Walterlv.Demo.Sharing.Utils.Threading
{
    public class DispatcherAsyncOperation<T> : DispatcherObject,
        IAwaitable<DispatcherAsyncOperation<T>, T>, IAwaiter<T>
    {
        private DispatcherAsyncOperation()
        {
        }

        public DispatcherAsyncOperation<T> GetAwaiter()
        {
            return this;
        }

        public bool IsCompleted { get; private set; }

        public T Result { get; private set; }

        public T GetResult()
        {
            if (_exception != null)
            {
                ExceptionDispatchInfo.Capture(_exception).Throw();
            }
            return Result;
        }

        public DispatcherAsyncOperation<T> ConfigurePriority(DispatcherPriority priority)
        {
            _priority = priority;
            return this;
        }

        public void OnCompleted(Action continuation)
        {
            if (IsCompleted)
            {
                _continuation.Invoke();
            }
            else
            {
                _continuation = continuation;
            }
        }

        private void ReportResult(T result, Exception ex)
        {
            Result = result;
            _exception = ex;
            IsCompleted = true;
            Dispatcher.InvokeAsync(_continuation, _priority);
        }

        private Action _continuation;
        private DispatcherPriority _priority = DispatcherPriority.Normal;
        private Exception _exception;

        public static DispatcherAsyncOperation<T> Create([NotNull] out Action<T, Exception> reportResult)
        {
            var asyncOperation = new DispatcherAsyncOperation<T>();
            reportResult = asyncOperation.ReportResult;
            return asyncOperation;
        }
    }
}
