using Cysharp.Threading.Tasks;
using UnityEngine;

public struct ShopState : IState
{
    public UniTask Setup()
    {
        return UniTask.CompletedTask;
    }

    public UniTask Teardown()
    {
        return UniTask.CompletedTask;
    }
}
