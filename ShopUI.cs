using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Cysharp.Threading.Tasks;

public class ShopUI : MonoBehaviour
{

    [SerializeField] private Button back;

    [Header("Shop Tabs")]
    [SerializeField] private Button background;
    [SerializeField] private Button bird;
    [SerializeField] private Button branch;

    [Header("Shop Pages")]
    [SerializeField] private GameObject backgroundPage;
    [SerializeField] private GameObject birdPage;
    [SerializeField] private GameObject branchPage;



    private void Awake()
    {

        back.onClick.AddListener(BackOnClick);
        background.onClick.AddListener(BackgroundOnClick);
        bird.onClick.AddListener(BirdOnClick);
        branch.onClick.AddListener(BranchOnClick);

    }
    public void BranchOnClick()
    {
        if (birdPage != null)
        {
            birdPage.SetActive(false);
        }
        if (backgroundPage != null)
        {
            backgroundPage.SetActive(false);
        }
        branchPage.SetActive(true);
    }
    public void BirdOnClick()
    {
        if (backgroundPage != null)
        {
            backgroundPage.SetActive(false);
        }
        if (branchPage != null)
        {
            branchPage.SetActive(false);
        }
        birdPage.SetActive(true);

    }
    public void BackgroundOnClick()
    {
        if (branchPage != null)
        {
            branchPage.SetActive(false);
        }
        if (branchPage != null)
        {
            branchPage.SetActive(false);
        }
        backgroundPage.SetActive(true);
    }

    public void BackOnClick()
    {
        if (ManagerProvider.StateManager.PreviousState is GameplayState)
        {
            ManagerProvider.StateManager.SetCurrentState(new GameplayState(false)).Forget();
        }
        else
        {
            ManagerProvider.StateManager.SetCurrentState(ManagerProvider.StateManager.PreviousState).Forget();
        }
        Debug.Log("[Back Button] clicked");
    }

   
}
