

namespace Redux
{
    /// <summary>
    /// Contains effect methods for the redux store's state    
    /// </summary>
    /// <typeparam name="T">type of store's state</typeparam>
    public interface IEffect<in T> where T: new() {}
}