using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    [Header("UI")]
    [SerializeField] Text CoinText;
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
        CoinText.text =
            "Coin : " +
            UserDataManager.Instance
            .CurrentUserData
            .Coin;

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.RefreshUI();
        }
    }

    public void OnClickBuyDrink()
    {
        BuyItem("Drink", 200);
    }

    public void OnClickBuyCookie()
    {
        BuyItem("Cookie", 300);
    }

    public void OnClickBuyJelly()
    {
        BuyItem("Jelly", 100);
    }

    void BuyItem(string itemName, int price)
    {
        UserData userData =
            UserDataManager.Instance.CurrentUserData;

        if (userData.Coin < price)
        {
            MessageText.text =
                $"[{itemName} 가격 : {price}] 코인이 부족합니다.";

            return;
        }

        userData.Coin -= price;

        if (userData.Inventory.ContainsKey(itemName))
        {
            userData.Inventory[itemName]++;
        }
        else
        {
            userData.Inventory[itemName] = 1;
        }

        UserDataManager.Instance.SaveUserData();

        RefreshUI();

        MessageText.text =
            itemName + " 구매 완료";
    }

    public void BuyUnit(string unitName)
    {
        int price = 100;

        UserData userData = UserDataManager.Instance.CurrentUserData;

        Dictionary<string, bool> unitList =
            JsonConvert.DeserializeObject
            <Dictionary<string, bool>>
            (userData.UnitList);

        if (unitList[unitName])
        {
            MessageText.text =
                "이미 보유한 유닛입니다.";

            return;
        }

        if (userData.Coin < price)
        {
            MessageText.text =
                "코인이 부족합니다.";

            return;
        }

        userData.Coin -= price;

        unitList[unitName] = true;

        userData.UnitList =
            JsonConvert.SerializeObject(unitList);

        UserDataManager.Instance.SaveUserData();

        RefreshUI();

        MessageText.text =
            $"{unitName} 구매 완료";
    }

    public void GameScene()
    {
        SceneManager.LoadScene("GameScene");
    }
}