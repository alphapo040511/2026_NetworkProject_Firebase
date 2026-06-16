using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("UI")]
    [SerializeField] Text DrinkCountText;
    [SerializeField] Text CookieCountText;
    [SerializeField] Text JellyCountText;
    [SerializeField] Text MessageText;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (UserDataManager.Instance.IsLoaded)
        {
            RefreshUI();
        }
        else
        {
            UserDataManager.Instance.OnDataLoaded += RefreshUI;
        }
    }

    private void OnDestroy()
    {
        if (UserDataManager.Instance != null)
        {
            UserDataManager.Instance.OnDataLoaded -= RefreshUI;
        }
    }

    public void RefreshUI()
    {
        DrinkCountText.text =
            "Drink : " + GetItemCount("Drink");

        CookieCountText.text =
            "Cookie : " + GetItemCount("Cookie");

        JellyCountText.text =
            "Jelly : " + GetItemCount("Jelly");
    }

    int GetItemCount(string itemName)
    {
        if (UserDataManager.Instance.CurrentUserData.Inventory
            .ContainsKey(itemName))
        {
            return UserDataManager.Instance
                .CurrentUserData
                .Inventory[itemName];
        }

        return 0;
    }

    public void OnClickUseDrink()
    {
        UseItem("Drink", "시원한 음료를 마셨습니다.");
    }

    public void OnClickUseCookie()
    {
        UseItem("Cookie", "바삭한 쿠키는 맛있습니다!");
    }

    public void OnClickUseJelly()
    {
        UseItem("Jelly", "젤리를 먹었습니다!");
    }

    void UseItem(string itemName, string usingMessage)
    {
        var inventory =
            UserDataManager.Instance
            .CurrentUserData
            .Inventory;

        if (!inventory.ContainsKey(itemName) ||
            inventory[itemName] <= 0)
        {
            MessageText.text =
                itemName + " 개수가 부족합니다.";

            return;
        }

        inventory[itemName]--;

        UserDataManager.Instance.SaveUserData();

        RefreshUI();

        MessageText.text =
            $"[{itemName} 사용 완료] {usingMessage}";
    }
}