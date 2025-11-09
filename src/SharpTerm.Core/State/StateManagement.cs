namespace SharpTerm.Core.State;

/// <summary>
/// Represents an action that can modify application state.
/// </summary>
public interface IAction
{
    string Type { get; }
}

/// <summary>
/// Reducer function that takes current state and an action, returns new state.
/// </summary>
public delegate TState Reducer<TState>(TState state, IAction action) where TState : class;

/// <summary>
/// Middleware function that can intercept actions before they reach reducers.
/// </summary>
public delegate Func<IAction, IAction> Middleware<TState>(Store<TState> store, Func<IAction, IAction> next) where TState : class;

/// <summary>
/// Centralized state container with Redux-like pattern.
/// </summary>
public class Store<TState> where TState : class
{
    private TState _state;
    private readonly Reducer<TState> _reducer;
    private readonly List<Action<TState>> _subscribers = new();
    private readonly List<Middleware<TState>> _middlewares = new();
    private Func<IAction, IAction>? _dispatchChain;

    public TState State => _state;

    public event EventHandler<StateChangedEventArgs<TState>>? StateChanged;

    public Store(TState initialState, Reducer<TState> reducer)
    {
        _state = initialState ?? throw new ArgumentNullException(nameof(initialState));
        _reducer = reducer ?? throw new ArgumentNullException(nameof(reducer));
        BuildDispatchChain();
    }

    /// <summary>
    /// Adds middleware to the store.
    /// </summary>
    public void AddMiddleware(Middleware<TState> middleware)
    {
        _middlewares.Add(middleware ?? throw new ArgumentNullException(nameof(middleware)));
        BuildDispatchChain();
    }

    /// <summary>
    /// Dispatches an action to modify the state.
    /// </summary>
    public void Dispatch(IAction action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        // Run through middleware chain
        var finalAction = _dispatchChain?.Invoke(action) ?? action;

        // Apply reducer
        var newState = _reducer(_state, finalAction);

        if (!ReferenceEquals(_state, newState))
        {
            var oldState = _state;
            _state = newState;

            // Notify subscribers
            NotifySubscribers(oldState, newState, finalAction);
        }
    }

    /// <summary>
    /// Subscribes to state changes.
    /// </summary>
    public IDisposable Subscribe(Action<TState> callback)
    {
        if (callback == null)
            throw new ArgumentNullException(nameof(callback));

        _subscribers.Add(callback);
        return new Unsubscriber(() => _subscribers.Remove(callback));
    }

    /// <summary>
    /// Gets a selector function result from the current state.
    /// </summary>
    public TResult Select<TResult>(Func<TState, TResult> selector)
    {
        return selector(_state);
    }

    private void BuildDispatchChain()
    {
        Func<IAction, IAction> chain = action => action;

        // Build middleware chain in reverse order
        for (int i = _middlewares.Count - 1; i >= 0; i--)
        {
            var middleware = _middlewares[i];
            var next = chain;
            chain = middleware(this, next);
        }

        _dispatchChain = chain;
    }

    private void NotifySubscribers(TState oldState, TState newState, IAction action)
    {
        StateChanged?.Invoke(this, new StateChangedEventArgs<TState>(oldState, newState, action));

        foreach (var subscriber in _subscribers.ToList())
        {
            subscriber(newState);
        }
    }

    private class Unsubscriber : IDisposable
    {
        private readonly Action _unsubscribe;

        public Unsubscriber(Action unsubscribe)
        {
            _unsubscribe = unsubscribe;
        }

        public void Dispose()
        {
            _unsubscribe();
        }
    }
}

/// <summary>
/// Event arguments for state changes.
/// </summary>
public class StateChangedEventArgs<TState> : EventArgs where TState : class
{
    public TState OldState { get; }
    public TState NewState { get; }
    public IAction Action { get; }

    public StateChangedEventArgs(TState oldState, TState newState, IAction action)
    {
        OldState = oldState;
        NewState = newState;
        Action = action;
    }
}

/// <summary>
/// Helper for combining multiple reducers.
/// </summary>
public static class ReducerHelpers
{
    /// <summary>
    /// Combines multiple reducers into one.
    /// </summary>
    public static Reducer<TState> Combine<TState>(params Reducer<TState>[] reducers) where TState : class
    {
        return (state, action) =>
        {
            var currentState = state;
            foreach (var reducer in reducers)
            {
                currentState = reducer(currentState, action);
            }
            return currentState;
        };
    }
}

/// <summary>
/// Common middleware implementations.
/// </summary>
public static class CommonMiddleware
{
    /// <summary>
    /// Logs all actions and state changes.
    /// </summary>
    public static Middleware<TState> Logger<TState>(Action<string> log) where TState : class
    {
        return (store, next) => action =>
        {
            log($"Action: {action.Type}");
            log($"Previous State: {System.Text.Json.JsonSerializer.Serialize(store.State)}");

            var result = next(action);

            log($"New State: {System.Text.Json.JsonSerializer.Serialize(store.State)}");
            log("---");

            return result;
        };
    }

    /// <summary>
    /// Handles async actions.
    /// </summary>
    public static Middleware<TState> AsyncMiddleware<TState>() where TState : class
    {
        return (store, next) => action =>
        {
            if (action is IAsyncAction asyncAction)
            {
                asyncAction.ExecuteAsync(store.Dispatch);
                return action;
            }

            return next(action);
        };
    }
}

/// <summary>
/// Interface for async actions.
/// </summary>
public interface IAsyncAction : IAction
{
    Task ExecuteAsync(Action<IAction> dispatch);
}

/// <summary>
/// Example state class for demonstration.
/// </summary>
public class AppState
{
    public string CurrentView { get; init; } = "main";
    public Dictionary<string, object> Data { get; init; } = new();
    public bool IsLoading { get; init; }
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Creates a new state with modified properties.
    /// </summary>
    public AppState With(
        string? currentView = null,
        Dictionary<string, object>? data = null,
        bool? isLoading = null,
        string? errorMessage = null)
    {
        return new AppState
        {
            CurrentView = currentView ?? CurrentView,
            Data = data ?? new Dictionary<string, object>(Data),
            IsLoading = isLoading ?? IsLoading,
            ErrorMessage = errorMessage ?? ErrorMessage
        };
    }
}

/// <summary>
/// Example actions.
/// </summary>
public class NavigateAction : IAction
{
    public string Type => "NAVIGATE";
    public string View { get; }

    public NavigateAction(string view)
    {
        View = view;
    }
}

public class SetDataAction : IAction
{
    public string Type => "SET_DATA";
    public string Key { get; }
    public object Value { get; }

    public SetDataAction(string key, object value)
    {
        Key = key;
        Value = value;
    }
}

public class SetLoadingAction : IAction
{
    public string Type => "SET_LOADING";
    public bool IsLoading { get; }

    public SetLoadingAction(bool isLoading)
    {
        IsLoading = isLoading;
    }
}

public class SetErrorAction : IAction
{
    public string Type => "SET_ERROR";
    public string? ErrorMessage { get; }

    public SetErrorAction(string? errorMessage)
    {
        ErrorMessage = errorMessage;
    }
}

/// <summary>
/// Example reducer for AppState.
/// </summary>
public static class AppStateReducer
{
    public static AppState Reduce(AppState state, IAction action)
    {
        return action switch
        {
            NavigateAction nav => state.With(currentView: nav.View),
            SetDataAction setData => state.With(data: AddToData(state.Data, setData.Key, setData.Value)),
            SetLoadingAction loading => state.With(isLoading: loading.IsLoading),
            SetErrorAction error => state.With(errorMessage: error.ErrorMessage),
            _ => state
        };
    }

    private static Dictionary<string, object> AddToData(Dictionary<string, object> data, string key, object value)
    {
        var newData = new Dictionary<string, object>(data)
        {
            [key] = value
        };
        return newData;
    }
}
