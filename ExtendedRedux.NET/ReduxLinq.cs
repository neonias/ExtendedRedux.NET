using System;
using System.Collections.Generic;

namespace Redux
{
    public static class ReduxLinq
    {

        public static IStore<TResult> Select<TSource, TResult>(this IStore<TSource> source, Func<TSource, TResult> select)
        {
            return new LinqStore<TSource, TResult>(source, select);
        }

        public static Reducer<T> Combine<T>(this IEnumerable<IReducer<T>> reducers) where T: new()
        {
            return new CombinedReducer<T>(reducers).Execute;
        }

        public static Middleware<T> Combine<T>(this IEnumerable<IEffect<T>> effects) where T : new()
        {
            return new CombinedEffects<T>(effects).Middleware;
        }
    }
}
