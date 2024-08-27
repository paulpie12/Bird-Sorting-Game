using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.UI;

public class ShopButton : MonoBehaviour
{

    public CategoryItem Item { get => item; }

    [SerializeField] private Button button;
    [SerializeField] private Text titleText;
    [SerializeField] private Text costText;
    [SerializeField] private Image iconImage;

    [Space]
    [SerializeField] private CanvasGroup purchasedOverlay;

    [ShowInInspector, ReadOnly] private CategoryItem item;

    private Action<ShopButton> onClicked;

    private void OnEnable()
    {
        button.onClick.AddListener(OnClicked);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(OnClicked);
    }

    public void Setup(CategoryItem item, Action<ShopButton> onClicked)
    {
        this.item = item;
        this.onClicked = onClicked;

        titleText.text = item.Name;
        costText.text = item.Cost.ToString();
        iconImage.sprite = item.Icon;

        Refresh();
    }

    public void Refresh()
    {
        bool isPurchased = item.IsPurchased;
        purchasedOverlay.alpha = isPurchased ? 1 : 0;
        purchasedOverlay.blocksRaycasts = isPurchased;
    }

    private void OnClicked()
    {
        onClicked?.Invoke(this);
    }
}
