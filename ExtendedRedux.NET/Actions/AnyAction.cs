using System;
using System.Runtime.Caching;
using System.Runtime.InteropServices;
using Redux;

namespace ExtendedRedux.NET.Actions
{
    /// <summary>
    /// If this type is used in a Reducer or an Effect, 
    /// it will be triggered if any action is passed to the Store.
    /// </summary>
    public class AnyAction : IAction
    {
    }

    public class MyStore<T> : Store<T>
    {
        public MyStore(Reducer<T> reducer, T initialState = default(T), params Middleware<T>[] middlewares) 
            : base(reducer, initialState, middlewares)
        {
            
        }

        public new void Dispatch(IAction action)
        {
            base.Dispatch(action);
        }
    }
}
