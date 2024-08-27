using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopCategoryView : MonoBehaviour
{

    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private ShopButton buttonPrefab;
    
    [SerializeReference] private BaseCategorizedScriptableObject categoryScriptableObject;

    private List<ShopButton> spawnedButtons = new List<ShopButton>();

    private void Awake()
    {
        Initialize();
    }

    private void OnEnable()
    {
        Refresh();
    }

    [ContextMenu("Initialize")]
    private void Initialize()
    {
        // Clear existing buttons
        for(int i = 0; i < spawnedButtons.Count; i++)
        {
            Destroy(spawnedButtons[i].gameObject);
        }
        spawnedButtons.Clear();

        // Spawn buttons for each of the categories in the Scriptable Object
        CategoryItem[] categories = categoryScriptableObject.Categories;
        for(int i = 0; i < categories.Length; i++)
        {
            ShopButton newButton = Instantiate(buttonPrefab, scrollRect.content.transform, false);
            newButton.Setup(categories[i], OnClickedShopButton);
            spawnedButtons.Add(newButton);
        }
    }

    [ContextMenu("Refresh")]
    private void Refresh()
    {
        scrollRect.verticalNormalizedPosition = 1;

        for (int i = 0; i < spawnedButtons.Count; i++)
        {
            spawnedButtons[i].Refresh();
        }
    }

    private void OnClickedShopButton(ShopButton button)
    {
        Debug.Log("[ShopCategoryView] Clicked on button: " + button.Item.Id);
        // bool purchased = DataProvider.Inventory.PurchaseItem(button.Item.Id);
    }
}
