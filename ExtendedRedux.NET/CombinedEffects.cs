using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Redux
{
    
    public class CombinedEffects<TState> 
    {
        /// <summary>
        /// effects extracted from the effect classes through reflection
        /// </summary>
        private readonly Dictionary<Type, Func<IAction, IStore<TState>, IObservable<IAction>>> effects
            = new Dictionary<Type, Func<IAction, IStore<TState>,  IObservable<IAction>>>();

        /// <summary>
        /// creates combined effect midleware through reflection API.  Looks at reducers method 
        /// and analyzes the ones marked by the ReduceAction, ExtractState and UpdateState attributes. 
        /// </summary>
        /// <param name="effectsObjects"></param>
        public  CombinedEffects(IEnumerable<object> effectsObjects)
        {
            foreach (var effect in effectsObjects)
            {
                foreach (
                    var method in
                    effect.GetType()
                        .GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public))
                {
                    var parameters = method.GetParameters();

                    if (parameters.Length < 1 || parameters.Length > 2) continue;

                    // check if the method signature is of the reducer and skip if not                    
                    if (!typeof(IObservable<IAction>).IsAssignableFrom(method.ReturnType)) continue;                    
                    var actionType = parameters[0].ParameterType;
                    if (!typeof(IAction).IsAssignableFrom(actionType)) continue;
                    if(parameters.Length == 2 && !typeof(IStore<TState>).IsAssignableFrom(parameters[1].ParameterType)) continue;


                    if (effects.ContainsKey(actionType))
                    {
                        var intermediate = effects[actionType];
                        effects[actionType] 
                            = (action,store) => 
                            intermediate(action,store)
                            .Merge((IObservable<IAction>) (
                                parameters.Length == 2 
                                ? method.Invoke(effect, new object[] {action, store}) 
                                : method.Invoke(effect, new object[] { action })));                                                    
                    }
                    else
                    {
                        effects.Add(actionType,(action,store) => (IObservable<IAction>)(
                                parameters.Length == 2
                                ? method.Invoke(effect, new object[] { action, store })
                                : method.Invoke(effect, new object[] { action })));
                    }
                }
            }
        }

        public Func<Dispatcher, Dispatcher> Middleware(IStore<TState> store)
        {
            return dispatcher => action =>
            {
                IAction nextAction = dispatcher(action);
                Func<IAction, IStore<TState>, IObservable<IAction>> effect;
                if (effects.TryGetValue(nextAction.GetType(), out effect))
                {
                    effect(nextAction, store)
                        .ObserveOnDispatcher()
                        .Subscribe(_ => store.Dispatch(_));
                }
                return nextAction;
            };
        }
    }
}
