using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class StateManager : BaseManager
{

    public UnityAction<IState> OnStateChanged;

    public bool IsEditing { get => currentState is EditingState; }
    public IState CurrentState { get => currentState; }
    public IState PreviousState { get; private set; }

    [ShowInInspector, ReadOnly, SerializeReference] private IState currentState;

    public override UniTask StartManager()
    {
        return UniTask.CompletedTask;
    }

    public override UniTask StopManager()
    {
        return UniTask.CompletedTask;
    }

    public async UniTask SetCurrentState(IState state)
    {
        if(currentState != null)
        {
            await currentState.Teardown();
        }
        PreviousState = currentState;
        Debug.Log("Setting current state: " + state.GetType().Name);
        currentState = state;
        await state.Setup();
        OnStateChanged?.Invoke(state);
    }

}
