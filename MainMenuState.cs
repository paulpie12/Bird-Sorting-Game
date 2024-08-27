using Cysharp.Threading.Tasks;

public struct MainMenuState : IState
{
    public async UniTask Setup()
    {
        await ManagerProvider.LevelManager.UnloadLevel();
    }

    public UniTask Teardown()
    {
        return UniTask.CompletedTask;
    }
}
