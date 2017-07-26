using System;

namespace Redux
{
    /// <summary>
    /// Contains reducer methods for the redux store's state
    /// </summary>
    /// <typeparam name="T">type of store's state</typeparam>
    public interface IReducer<in T> where T: new() {}
}