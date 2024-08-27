using Cysharp.Threading.Tasks;
using UnityEngine;

public struct PauseState : IState
{
    public UniTask Setup()
    {
        ManagerProvider.PauseManager.Pause();
        return UniTask.CompletedTask;
    }

    public UniTask Teardown()
    {
        ManagerProvider.PauseManager.Resume();
        return UniTask.CompletedTask;
    }
}
