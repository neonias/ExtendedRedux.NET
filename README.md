# Redux.NET extension

This extension improves the developer experience with the Redux.NET. It allows for usage of MEF with Redux, but is not dependent on it. 

## Usage:

### Defining the store: 

```c#
using Redux;
using Siemens.Prototyping.ReduxStore.States;

class ApplicationStore: Store<ApplicationState>
{
       
    public ApplicationStore(
        IEnumerable<IReducer<ApplicationState>> reducers, 
        IEnumerable<IEffect<ApplicationState>> effects) 
        : base(reducers.Combine(), new ApplicationState(), effects.Combine() )
    {}
}
```

or with MEF

```c#
using Redux;
using Siemens.Prototyping.ReduxStore.States;

namespace Siemens.Prototyping.ReduxStore
{
    [Export(typeof(IStore<ApplicationState>))]
    class ApplicationStore: Store<ApplicationState>
    {
        [ImportingConstructor]
        public ApplicationStore(
            [ImportMany(typeof(IReducer<ApplicationState>))]
                IEnumerable<IReducer<ApplicationState>> reducers,
            [ImportMany(typeof(IEffect<ApplicationState>))]
                IEnumerable<IEffect<ApplicationState>> effects) 
            : base(reducers.Combine(), new ApplicationState(), effects.Combine() )
        {}
    }
}
```

The state can be any Plain C# class, with public C# Properties. 

### Reducers

A reducer is a class decorated by the Reducer attribute. The parameter of the attribute constructor determines
the path to the state that is reduced. Each reducer method gets the input state as specified by this parameter. 
Reducer's methods that are public, has two arguments, and the second argument is derived from the Redux.IAction. 
The type of second argument determines which actions is dispatched to this reducer (inheritance is **not** taken 
into account) are combined into final global reducer. 

It is recommended that the reducer class implements IReducer<ApplicationState> for the purpose of MEF usage 
in Redux Store creation.

To simplify imutable updates, the extension Object.Assign is provided. It creates copy new instance of the state,
copies the original state and assign the specific value to properties indicated by the specified path. 

*Example:* Below reducer has two reducer methods that updates the _InteractionState_ property of the store state. 

```c#
[Reducer("Album.Pictures")]
internal class InteractionStateReducer: IReducer<ApplicationState>
{         
    public Pictures LoadImageReducer(Pictures state, LoadImageAction action)
    {
        return state.Assign(new object[]{"CurrentImageLoading"}, new object[]{true});            
    }
    
}
```

### Subscribing

You can use `Redux.IStore<>.Select` to get the of the partial state, e.g. 

```c#
IStore<ImageSource> imageStore  = store.Select(_ => _.Pictures.CurrentImage);
...
imageStore.Dispatch(new LoadImageAction(uri));
```

## Effects

Effects are simple classes that handles the side-effects of actions without causing side-effects in reducers. 
They are intended to execute async operations, and transforms one action into observable of another action(s)
You can use MEF to better employ effects. 

Each effect class can have one or more public effect methods with the nomenclature

```c#
    public IObservable Effect(SpecificAction action){...}
```

or 

```c#
    public IObservable Effect(SpecificAction action, IStore<AppState> store){...}
```

which will be executed after the action of the `SpecificAction` type is processed by reducers. 


```c#
    [Export(typeof(IEffect<ApplicationState>))]
    internal sealed class PlannerEffect: IEffect<ApplicationState>
    {
        private readonly IList<IIntentHeuristic> heuristics;
        
        [ImportingConstructor]
        public PlannerEffect( 
            [ImportMany] IEnumerable<IIntentHeuristic> heuristics)
        {
            this.heuristics = heuristics.ToList();            
        }

        public IObservable<IAction> AddSegmentationEffect(AddSegmentationAction action, ApplicationStore store)
        {
            var context = new Context
            {
                Gesture = store.GetState().InteractionState.CurrentGesture,
                Volume = store.GetState().CurrentDataset.Volume,
                SliceIndex = store.GetState().CurrentDataset.CurrentSliceIndex
            };

            var intent = new HeuristicIntent
            {
                Value = heuristics
                    .Select(heuristic => heuristic.ComputePlausibility(context, action.Segmentation))
                    .Aggregate(HeuristicUnit.Combine),
                Segmentation = action.Segmentation
            };
            return Observable.Return(new AddIntentAction(intent));
        }

       
    }
```
