using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Branch : MonoBehaviour, IBaseWorldObject, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    public int BranchId { get => branchData.BranchId; }
    public int BranchType { get => branchType; }
    private bool IsLeft { get => transform.localScale.x < 0; }
    public BranchData BranchData { get => branchData; }
    public bool Selected { get => selected; }
    public int SelectedBirdCount { get => spawnedBirds.Count(x => x.Selected); }
    public bool BranchComplete { get => SpawnedBirdCount <= 0; }
    public IEnumerable<Bird> SpawnedBirds { get => spawnedBirds.Where(x => x.gameObject.activeInHierarchy); }
    public int SpawnedBirdCount { get => SpawnedBirds.Count(); }


    [Header("Variables")]
    [Tooltip("Used to identify which prefab this belongs to from the scriptable object")]
    [SerializeField] private int branchType;

    [SerializeField] private CanvasGroup selectionBGCanvasGroup;
    [SerializeField] private Image selectionBG;
    [SerializeField] private RectTransform branchObject;
    [SerializeField] private LayoutElement layoutElement;
    [SerializeField] private PlaceAlongPoints spawnAlongPoints;

    [Header("Runtime Info")]
    [ShowInInspector, ReadOnly] private BranchData branchData;
    [ShowInInspector, ReadOnly] private List<Bird> spawnedBirds = new List<Bird>();
    [ShowInInspector, ReadOnly] private bool selected;

    private CancellationTokenSource cts;
    private bool isDisabling; // Tells us when the branch is being hidden and animated out

    public int GetBranchIdForBird(GameObject birdObject)
    {
        Branch birdBranch = birdObject.GetComponentInParent<Branch>();
        if (birdBranch != null)
        {
            return birdBranch.BranchId;
        }
        else
        {
            Debug.LogError("Bird object is not on a branch.");
            return -1;
        }
    }

    private void OnEnable()
    {
        selectionBGCanvasGroup.DOKill();
        selectionBGCanvasGroup.alpha = 0;

        layoutElement.DOKill();
        branchObject.DOKill();

        isDisabling = false;
    }
    
    private void OnValidate()
    {
        GetComponent<RectTransform>().pivot = Vector2.up;
    }

    public void InitializeBranch(BranchData branchData)
    {
        if (cts != null)
        {
            cts.Cancel();
            cts.Dispose();
        }
        cts = CancellationTokenSource.CreateLinkedTokenSource(gameObject.GetCancellationTokenOnDestroy());
        RefreshBranch(branchData, cts.Token).Forget();
    }

    /// <summary>
    /// Applies the BranchData onto the object
    /// </summary>
    /// <param name="branchData"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async UniTask RefreshBranch(BranchData branchData, CancellationToken ct)
    {
        if (ct.IsCancellationRequested) return;
        this.branchData = branchData;

        // Hide birds missing
        List<UniTask> tasks = new List<UniTask>();
        bool hasBird = false;
        for (int x = 0; x < spawnedBirds.Count; x++)
        {
            if (spawnedBirds[x].gameObject.activeInHierarchy == false) continue;
            hasBird = false;
            for (int i = 0; i < branchData.BirdData.Count; i++)
            {
                if (spawnedBirds[x].BirdId == branchData.BirdData[i].BirdId && spawnedBirds[x].BirdType == branchData.BirdData[i].BirdType)
                {
                    hasBird = true;
                    break;
                }
            }
            if (!hasBird)
            {
                GameObject birdToToggle = spawnedBirds[x].gameObject;
                tasks.Add(spawnedBirds[x].MoveBirdOut().ContinueWith(() => birdToToggle.SetActive(false)));
            }
        }
        await UniTask.WhenAll(tasks).AttachExternalCancellation(ct).SuppressCancellationThrow();
        if (ct.IsCancellationRequested) return;

        for (int i = 0; i < branchData.BirdData.Count; i++)
        {
            SpawnOrRefreshBird(branchData.BirdData[i]);
        }
        RefreshBirdSorting();
    }

    private void SpawnOrRefreshBird(BirdData birdData)
    {
        // Check if the bird exists and is spawned already
        Bird bird = spawnedBirds.FirstOrDefault(x =>
        {
            return x.gameObject.activeInHierarchy &&
                x.BirdId == birdData.BirdId &&
                x.BirdType == birdData.BirdType;
        });
        if (bird != null)
        {
            bird.gameObject.SetActive(true);
            spawnAlongPoints.MoveObjectToIndex(bird.gameObject, birdData.Index);
            bird.RefreshBird(birdData);
            return;
        }

        // Check if the bird type exists already
        bird = spawnedBirds.FirstOrDefault(x =>
        {
            return x.gameObject.activeInHierarchy == false &&
                x.BirdData.BirdType == birdData.BirdType;
        });
        if (bird != null)
        {
            bird.gameObject.SetActive(true);
            bird.RefreshBird(birdData);
            spawnAlongPoints.MoveObjectToIndex(bird.gameObject, birdData.Index);
            bird.MoveBirdIn().Forget();
            return;
        }

        // Spawn bird depending on its bird type
        GameObject newObj = Instantiate(ScriptableObjectProvider.BirdsScriptableObject.ActiveBirdCategory.Birds[birdData.BirdType], Vector3.zero, Quaternion.identity, transform);
        bird = newObj.GetComponent<Bird>();
        spawnedBirds.Add(bird);
        bird.Initialize(birdData.BirdId, this);
        bird.MoveBirdIn().Forget();
        spawnAlongPoints.MoveObjectToIndex(newObj, birdData.Index);
    }

    private void RefreshBirdSorting()
    {
        List<Bird> smallList = spawnedBirds.Where(x => x.gameObject.activeInHierarchy).OrderBy(x => x.BirdData.Index).ToList();

        int index = 0;
        for (int i = 0; i < smallList.Count; i++)
        {
            smallList[i].transform.SetSiblingIndex(index);
            index++;
        }
    }
    public async UniTask AnimateIn(CancellationToken ct)
    {
        isDisabling = false;

        // Animate in the branch
        Vector2 newPos = Vector2.left * branchObject.rect.width;
        newPos.y = branchObject.anchoredPosition.y;

        branchObject.DOKill();
        branchObject.localEulerAngles = Vector3.zero;
        branchObject.anchoredPosition = newPos;
        branchObject.DOAnchorPosX(0, 0.4f).SetDelay(0.1f);

        layoutElement.DOKill();
        layoutElement.DOPreferredSize(new Vector2(0, 0), 0.3f);

        branchObject.DOPunchRotation(Vector3.forward * Random.Range(4, 10f), 0.3f).OnComplete(() =>
        {
            layoutElement.DOKill();
            layoutElement.DOPreferredSize(new Vector2(0, 2000), 0.3f);
        });

        // Done animating the branch in, now animate the birds in
        await UniTask.Delay(300, true, PlayerLoopTiming.Update, ct).SuppressCancellationThrow();
        if (ct.IsCancellationRequested) return;

        List<UniTask> tasks = new List<UniTask>();
        for (int i = 0; i < spawnedBirds.Count; i++)
        {
            if (spawnedBirds[i].gameObject.activeInHierarchy == false)
                continue;

            tasks.Add(spawnedBirds[i].MoveBirdIn());
        }
        await UniTask.WhenAll(tasks).AttachExternalCancellation(ct).SuppressCancellationThrow();
        if (ct.IsCancellationRequested) return;

        // Branch and Birds Animated in
    }

    public async UniTask AnimateOut(CancellationToken ct)
    {
        isDisabling = true;

        // Animate out the birds
        List<UniTask> tasks = new List<UniTask>();
        for (int i = 0; i < spawnedBirds.Count; i++)
        {
            if (spawnedBirds[i].gameObject.activeInHierarchy == false)
                continue;

            tasks.Add(spawnedBirds[i].MoveBirdOut());
        }
        await UniTask.WhenAll(tasks).AttachExternalCancellation(ct).SuppressCancellationThrow();
        if (ct.IsCancellationRequested) return;

        // Animating out birds done, animate branch
        branchObject.DOKill();
        float newPos = -branchObject.rect.width;
        branchObject.DOAnchorPosX(newPos, 0.4f).SetDelay(0.1f);
        await branchObject.DOPunchRotation(Vector3.forward * Random.Range(4, 10f), 0.3f).AsyncWaitForCompletion();
        if (ct.IsCancellationRequested) return;
        layoutElement.DOKill();
        await layoutElement.DOPreferredSize(new Vector2(0, 0), 0.3f).AsyncWaitForCompletion();
        if (ct.IsCancellationRequested) return;

        // Branch is animated out
        gameObject.SetActive(false);
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        SelectBranch(eventData);
    }

    public int GetSelectableBirdCount()
    {
        return GetSelectableBirds().Length;
    }

    private Bird[] GetSelectableBirds()
    {
        List<Bird> birds = new List<Bird>();
        if (spawnedBirds.Count > 0)
        {
            //sorts birds on branch and flips for right/left side
            int firstBirdType = -1;
            if (!IsLeft)
                spawnedBirds = spawnedBirds.OrderBy(x => x.transform.position.x).ToList();
            else
                spawnedBirds = spawnedBirds.OrderByDescending(x => x.transform.position.x).ToList();
            for (int i = spawnedBirds.Count - 1; i >= 0; i--)
            {
                // Ignore pooled birds
                if (spawnedBirds[i].isActiveAndEnabled == false)
                    continue;
                // Find the first active bird on the branch
                if (firstBirdType == -1)
                    firstBirdType = spawnedBirds[i].BirdType;
                // Stop when finding a bird that is not the same type as the first
                if (spawnedBirds[i].BirdType != firstBirdType)
                    break;
                // Bird is the same as the first bird type, add it to the list
                birds.Add(spawnedBirds[i]);
            }
        }
        return birds.ToArray();
    }

    public void SelectBirds(bool val)
    {
        Bird[] selectableBirds = GetSelectableBirds();
        for (int i = 0; i < selectableBirds.Length; i++)
        {
            selectableBirds[i].SelectBird(val);
        }
    }

    public async UniTask MoveBranch(Branch branchToMoveTo)
    {
        Bird[] selectedBirds = GetSelectableBirds();

        // Deselect birds on branches
        branchToMoveTo.SelectBirds(false);
        SelectBirds(false);

        // Move the birds
        IEnumerable<UniTask> birdsMovingTask = selectedBirds.Select(x => MoveBirdToBranch(x, branchToMoveTo));
        await UniTask.WhenAll(birdsMovingTask);
    }

    private async UniTask MoveBirdToBranch(Bird bird, Branch branch)
    {
        // Move the birds in the level data
        branchData.MoveBird(bird, branch);

        // Move the bird objects on the branch's spawned list
        spawnedBirds.Remove(bird);
        branch.spawnedBirds.Add(bird);

        // Initialize bird data
        Vector3 prevPos = bird.transform.position;
        branch.spawnAlongPoints.MoveObjectToIndex(bird.gameObject, bird.BirdData.Index);
        bird.Initialize(bird.BirdId, branch);
        await bird.MoveBirdTo(prevPos, Vector3.zero);
    }

    public void SelectBranch(PointerEventData eventData)
    {
        if (isDisabling) return;
        
        // Handle logic while not editing
        if (ManagerProvider.StateManager.IsEditing == false)
        {
            ManagerProvider.GameplayManager.SelectBranch(this).Forget();
            return;
        }

        // Handle editing logic
        if (ManagerProvider.EditingManager.IsErasing)
        {
            if (branchData.BirdData.Count <= 0)
            {
                ManagerProvider.EditingManager.RemoveBranch(BranchId);
            }
            else
            {
                ManagerProvider.EditingManager.RemoveBird(BranchId);
            }
        }
        else
        {
            ManagerProvider.EditingManager.AddBird(BranchId);
        }
        selectionBGCanvasGroup.DOKill();
        selectionBGCanvasGroup.DOFade(0, 0.2f);
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        if (ManagerProvider.StateManager.IsEditing == false) return;
        if (isDisabling) return;

        selectionBGCanvasGroup.DOKill();
        selectionBGCanvasGroup.DOFade(0.2f, 0.2f);
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        if (ManagerProvider.StateManager.IsEditing == false) return;
        if (isDisabling) return;

        selectionBGCanvasGroup.DOKill();
        selectionBGCanvasGroup.DOFade(0, 0.2f);
    }

}


