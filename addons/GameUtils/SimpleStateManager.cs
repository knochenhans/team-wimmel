using System;

#nullable enable

public class SimpleStateManager<T>(T? initialState = default) where T : Enum
{
    public delegate void StateChangedHandler(T oldState, T newState, bool silent = false);
    public event StateChangedHandler? StateChanged;

    private T? currentState = initialState;
    public T? CurrentState
    {
        get => currentState;
        set
        {
            if (Equals(currentState, value))
                return;

            var oldState = currentState;
            currentState = value;
            OnStateChanged(oldState, value);
        }
    }

    public void SetState(T state, bool silent = false)
    {
        if (Equals(currentState, state))
            return;

        var oldState = currentState;
        currentState = state;
        OnStateChanged(oldState, state, silent);
    }

    protected virtual void OnStateChanged(T? oldState, T? newState, bool silent = false)
    {
        if (oldState is null || newState is null)
            return;
        StateChanged?.Invoke(oldState, newState, silent);
    }

    public override string ToString() => currentState?.ToString() ?? "null";
}
