using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;

namespace Walterlv.Exceptions
{
    public static class AggregateExceptionExtensions
    {
        public static void Append(this IAggregateException aex, Exception innerException)
        {
            aex.InnerExceptions?.Add(innerException);
        }

        public static void Append(this IAggregateException aex, IEnumerable<Exception> innerExceptions)
        {
            foreach (var ex in innerExceptions)
            {
                aex.InnerExceptions?.Add(ex);
            }
        }

        public static void Catch(this IAggregateException aex, Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                aex.Append(ex);
            }
        }

        public static void Rethrow(this IAggregateException aex)
        {
            if (aex == null) throw new ArgumentNullException(nameof(aex));
            if (!(aex is Exception))
            {
                throw new ArgumentException("Argument should always be an Exception.", nameof(aex));
            }
            var innerExceptions = aex.InnerExceptions;
            if (innerExceptions != null && innerExceptions.Any())
            {
                Rethrow(innerExceptions, exceptions => (Exception) aex);
            }
        }

        public static void Rethrow(this AggregateException aex)
        {
            if (aex == null) throw new ArgumentNullException(nameof(aex));
            Rethrow(aex.InnerExceptions, exceptions => aex);
        }

        public static void Rethrow(this ICollection<Exception> exceptions,
            Func<ICollection<Exception>, Exception> createAggregateException = null)
        {
            if (exceptions == null) throw new ArgumentNullException(nameof(exceptions));
            if (exceptions.Count == 1)
            {
                ExceptionDispatchInfo.Capture(exceptions.First()).Throw();
            }
            else if (exceptions.Count > 1)
            {
                throw createAggregateException == null
                    ? new AggregateException(exceptions)
                    : createAggregateException(exceptions);
            }
        }
    }
}
