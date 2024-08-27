using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using Spine.Unity;
using System.Linq;
using UnityEngine;

[ExecuteAlways]
public class Bird : MonoBehaviour
{

    public int BirdType => birdData.BirdType;
    public int BirdId => birdData.BirdId;
    public BirdData BirdData => birdData;
    public bool Selected => selected;
    private Vector3 TargetCornerPosition => branch?.BranchData?.BranchAlignment == BranchAlignments.Left ? ManagerProvider.LevelManager.Level.TopRighttPos : ManagerProvider.LevelManager.Level.TopLeftPos;

    private const string idleAnimation = "idle";
    private const string flyAnimation = "fly";
    [Header("Variables")]
    [SerializeField] private bool flippedByDefault;
    [SerializeField] private SkeletonAnimation skeletonAnimation;

    
    [Header("Runtime Info")]
    [ShowInInspector, ReadOnly] private Branch branch = default;
    [ShowInInspector, ReadOnly] private BirdData birdData = default;
    [ShowInInspector, ReadOnly] private float originalScale;

    [ShowInInspector, ReadOnly] private bool selected;

    private void Awake()
    {
        skeletonAnimation = GetComponentInChildren<SkeletonAnimation>();
        if (originalScale <= 0)
            originalScale = transform.GetChild(0).localScale.x;
    }

    public void Initialize(int birdId, Branch branch)
    {
        this.branch = branch;
        birdData = branch.BranchData.BirdData.FirstOrDefault(x => x.BirdId == birdId);
        RefreshBird(birdData);
    }

    /// <summary>
    /// Applies the Bird Data
    /// </summary>
    /// <param name="birdData"></param>
    public void RefreshBird(BirdData birdData)
    {
        this.birdData = birdData;
        transform.localScale = new Vector3(flippedByDefault ? -1 : 1, 1, 1);
    }

    /// <summary>
    /// Animates the bird being in selected mode or not
    /// </summary>
    /// <param name="val"></param>
    public void SelectBird(bool val)
    {
        selected = val;
        transform.localPosition = new Vector3(transform.localPosition.x, val ? (40.0f) : 0);
    }

    public async UniTask MoveBirdIn()
    {
        if (transform.childCount <= 0)
            Debug.LogError("Failed to find any children");
        Transform childRectTransform = transform.GetChild(0).transform;
        if (childRectTransform == null)
            Debug.LogErrorFormat("Failed to find any transform in child {0}", childRectTransform.childCount);
        childRectTransform.DOKill();
        childRectTransform.position = TargetCornerPosition;
        childRectTransform.localScale = Vector3.one * originalScale;
        childRectTransform.DOLocalMove(Vector2.zero, 0.6f);
        skeletonAnimation.AnimationState.SetAnimation(0, flyAnimation, true);
        await UniTask.Delay(600);
        skeletonAnimation.AnimationState.SetAnimation(0, idleAnimation, true);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawCube(TargetCornerPosition, Vector3.one * 3);
    }

    public async UniTask MoveBirdOut()
    {
        if (transform.childCount <= 0)
            Debug.LogError("Failed to find any children");
        Transform childRectTransform = transform.GetChild(0).transform;
        if (childRectTransform == null)
            Debug.LogError("Failed to find any transform in child");

        skeletonAnimation.AnimationState.SetAnimation(0, flyAnimation, true);
        childRectTransform.DOKill();
        childRectTransform.DOMove(TargetCornerPosition, 0.6f);
        await UniTask.Delay(600);
        if (originalScale <= 0)
            originalScale = transform.GetChild(0).localScale.x;
        childRectTransform.localScale = Vector3.zero;

        gameObject.SetActive(false);
    }

    public async UniTask MoveBirdTo(Vector3 prevPos, Vector3 targetPos)
    {
        if (transform.childCount <= 0)
            Debug.LogError("Failed to find any children");
        Transform childRectTransform = transform.GetChild(0).transform;
        if (childRectTransform == null)
            Debug.LogError("Failed to find any transform in child");
        childRectTransform.DOKill();
        childRectTransform.position = prevPos;
        bool isFacingLeft = prevPos.x > targetPos.x;
        int prevXDir = (int)childRectTransform.localScale.x;
        childRectTransform.localScale = new Vector3(isFacingLeft ? -prevXDir: -prevXDir, childRectTransform.localScale.y);
        childRectTransform.DOLocalMove(targetPos, 0.8f).SetEase(Ease.InOutCubic);
        skeletonAnimation.AnimationState.SetAnimation(0, flyAnimation, true);
        await UniTask.Delay(800);
        skeletonAnimation.AnimationState.SetAnimation(0, idleAnimation, true);
        childRectTransform.localScale = new Vector3(prevXDir, childRectTransform.localScale.y);
    }

}
