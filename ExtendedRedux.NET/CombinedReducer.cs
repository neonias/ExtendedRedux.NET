using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ExtendedRedux.NET.Actions;


// ReSharper disable once CheckNamespace
namespace Redux
{
    /// <summary>
    /// Responsibilities:
    /// <para>Combines individual reducers into one reducer function used by the store</para>
    /// <typeparam name="T"> type of the redux store's state</typeparam>
    /// </summary>
    internal class CombinedReducer<T> where T : new()
    {
        /// <summary>
        /// reducers extracted from the reducer classes through reflection
        /// </summary>
        private readonly Dictionary<Type, Func<T, IAction, T>> reducers 
            = new Dictionary<Type, Func<T, IAction, T>>();

        /// <summary>
        /// Returns new state (new instance) based on current state and action being performed
        /// </summary>
        /// <param name="currentState">Current application state</param>
        /// <param name="action">Instance of action being performed - must derive from IAction</param>
        /// <returns>New instance of state, or the same instance, if a unknown action is performed.</returns>
        public T Execute(T currentState, IAction action)
        {
            Func<T, IAction, T> reducer;
            var intermediateState = reducers.TryGetValue(typeof(AnyAction), out reducer)
                ? reducer(currentState, action)
                : currentState;
            return reducers.TryGetValue(action.GetType(), out reducer) ? reducer(intermediateState, action) : intermediateState;
        }

        /// <summary>
        /// creates combined reducer through reflection API.  Looks at reducers method 
        /// and analyzes the ones marked by the ReduceAction, ExtractState and UpdateState attributes. 
        /// </summary>
        /// <param name="reducers"></param>
        public CombinedReducer(IEnumerable<object> reducers)
        {
            foreach (var reducer in reducers)
            {
                var reducerAtt = reducer.GetType().GetCustomAttribute(typeof(ReducerAttribute)) as ReducerAttribute;
                if (reducerAtt == null)
                    throw new InvalidOperationException(
                        "The class '" + reducer.GetType().FullName
                        + "' is not a reducer -> use ReducerAttribute");

                string[] path = reducerAtt.Path;

                foreach (
                    var method in
                    reducer.GetType()
                        .GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public))
                {
                    var parameters = method.GetParameters();

                    // check if the method signature is of the reducer and skip if not
                    if (parameters.Length != 2) continue;

                    var actionType = parameters[1].ParameterType;

                    if (!typeof(IAction).IsAssignableFrom(actionType)) continue;

                    if (this.reducers.ContainsKey(actionType))
                    {
                        var intermediate = this.reducers[actionType];
                        this.reducers[actionType] = (state, action) =>
                        {
                            return (T) Assign(
                                intermediate(state, action),
                                path, _ => method.Invoke(reducer, new[] {_, action}));
                        };
                    }
                    else
                    {
                        this.reducers.Add(
                            actionType,
                            (state, action) => (T) Assign(state, path, _ => method.Invoke(reducer, new[] {_, action})));
                    }
                }
            }
        }


        /// <summary>
        /// creates shallow copy of the input state and replaces only the properties that are on the provided path. 
        /// This ensures that the input is not mutated and its properties are either copied or replaced if mutated
        /// </summary>
        /// <param name="state">the source object</param>
        /// <param name="path">the path to the mutated</param>
        /// <param name="reducer">the function that calculates new value for the mutated property</param>
        /// <returns></returns>
        private object Assign(object state, string[] path, Func<object,object> reducer  )
        {
            if (path.Length == 0)
            {
                return reducer(state);
            }
            var sourceType = state.GetType();
            object destination = Activator.CreateInstance(sourceType);

            foreach (
                var property in
                sourceType.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance))
            {
                object propertyValue = property.GetValue(state);
                if (property.Name.Equals(path[0]))
                {
                    propertyValue = Assign(propertyValue, path.Skip(1).ToArray(), reducer);
                }
                property.SetValue(destination, propertyValue);
            }
            return destination;
        }    
              
    }
}
