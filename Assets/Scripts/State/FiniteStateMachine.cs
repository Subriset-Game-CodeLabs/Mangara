public class FiniteStateMachine<T> where T : IState
{
    private T _currentState;
    private T _previousState;

    public T CurrentState => _currentState;
    public T PreviousState => _previousState;

    public FiniteStateMachine(T entry)
    {
        _currentState = entry;
        _currentState.OnEnter();
    }

    public void ChangeState(T newState)
    {
        if (ReferenceEquals(newState, _currentState))
        {
            return;
        }
        _previousState = _currentState;
        _previousState.OnExit();
        _currentState = newState;
        _currentState.OnEnter();
    }

    public void OnUpdate()
    {
        _currentState.OnUpdate();
    }
}
