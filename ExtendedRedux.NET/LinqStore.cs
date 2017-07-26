using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace Redux
{
    /// <summary>
    /// internal state for enabling LINQ on IStore objects
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <typeparam name="TSubState"></typeparam>
    class LinqStore<TState, TSubState> : IStore<TSubState>
    {
        private readonly IStore<TState> parent;
        private readonly Func<TState, TSubState> select;

        public LinqStore(IStore<TState> parent, Func<TState, TSubState> select)
        {
            this.parent = parent;
            this.select = select;
        }

        public IAction Dispatch(IAction action)
        {
            return parent.Dispatch(action);
        }

        public TSubState GetState()
        {
            return select(parent.GetState());
        }

        /// <summary>
        /// the observer will be notified about the changes on the selected part of the store; 
        /// only changes on the selected part are propagate, duplicated notifications are filtered out
        /// 
        /// </summary>                
        public IDisposable Subscribe(IObserver<TSubState> observer)
        {
            return Observable.Select(parent,select).DistinctUntilChanged(new Comparer()).Subscribe(observer);
        }

        private class Comparer : IEqualityComparer<TSubState>
        {
            public bool Equals(TSubState x, TSubState y)
            {

                return x != null && x.Equals(y) || x ==null && y == null;
            }

            public int GetHashCode(TSubState obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}