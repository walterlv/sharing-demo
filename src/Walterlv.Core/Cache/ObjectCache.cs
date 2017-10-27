using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Walterlv.Cache
{
    public class ObjectCache<TKey, TValue> where TValue : class
    {
        private static readonly Dictionary<TKey, WeakReference<TValue>> Cache = new Dictionary<TKey, WeakReference<TValue>>();

        public Func<TKey, TValue> Converter { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private TValue Convert(TKey key)
        {
            return Converter?.Invoke(key);
        }

        public TValue this[TKey key]
        {
            get
            {
                if (Cache.TryGetValue(key, out var reference))
                {
                    if (reference.TryGetTarget(out var value))
                    {
                        return value;
                    }
                }
                var newValue = Convert(key);
                reference = new WeakReference<TValue>(newValue);
                Cache[key] = reference;
                return newValue;
            }
        }
    }
}
