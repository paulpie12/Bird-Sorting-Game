using Cysharp.Threading.Tasks;
using UnityEngine;

public class UIManager : BaseManager
{

    private static StateManager StateManager { get => ManagerProvider.StateManager; }
    private static LifecycleManager LifecycleManager { get => ManagerProvider.LifecycleManager; }

    [Header("Life Cycle UI")]
    [SerializeField] private LoaderUI loaderUI;
    [SerializeField] private LevelEditorUI levelEditorUI;
    [SerializeField] private MainMenuUI mainMenuUI;
    [SerializeField] private GameplayUI gameplayUI;
    [SerializeField] private CompletionUI completionUI;
    [SerializeField] private RewardUI rewardUI;
    [SerializeField] private PauseUI pauseUI;
    [SerializeField] private ShopUI shopUI;


    [Header("Other")]
    [SerializeField] private DebugUI debugUI;

    public override UniTask StartManager()
    {
        StateManager.OnStateChanged += OnStateChanged;
        LifecycleManager.OnDebugToggled += OnToggledDebugMode;

        pauseUI.gameObject.SetActive(false);
        debugUI.gameObject.SetActive(false);
        rewardUI.gameObject.SetActive(false);
        levelEditorUI.gameObject.SetActive(false);
        mainMenuUI.gameObject.SetActive(false);
        gameplayUI.gameObject.SetActive(false);
        completionUI.gameObject.SetActive(false);
        shopUI.gameObject.SetActive(false);

        return UniTask.CompletedTask;
    }

    public override UniTask StopManager()
    {
        StateManager.OnStateChanged -= OnStateChanged;
        LifecycleManager.OnDebugToggled -= OnToggledDebugMode;
        
        return UniTask.CompletedTask;
    }

    private void OnStateChanged(IState state)
    {
        // TODO: Move this so each state manages its own UI
        levelEditorUI.gameObject.SetActive(state is EditingState);
        mainMenuUI.gameObject.SetActive(state is MainMenuState);
        gameplayUI.gameObject.SetActive(state is GameplayState || state is PauseState);
        completionUI.gameObject.SetActive(state is CompletedState);
        pauseUI.gameObject.SetActive(state is PauseState);
        shopUI.gameObject.SetActive(state is ShopState);
    }

    private void OnToggledDebugMode(bool val)
    {
        debugUI.gameObject.SetActive(val);
    }

    public void ToggleLoader(bool val)
    {
        loaderUI.gameObject.SetActive(val);
    }

    public UniTask ShowRewardUI(InventorySlot inventorySlot)
    {
        rewardUI.gameObject.SetActive(true);
        rewardUI.ShowRewardUI(inventorySlot);
        return UniTask.WaitUntil(() => rewardUI.gameObject.activeInHierarchy == false);
    }

}
