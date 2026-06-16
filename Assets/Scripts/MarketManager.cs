using Firebase.Database;
using Newtonsoft.Json;
using PimDeWitte.UnityMainThreadDispatcher;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MarketManager : MonoBehaviour
{
    public static MarketManager Instance;

    FirebaseDatabase database;
    DatabaseReference reference;
    UnityMainThreadDispatcher dispatcher;

    [SerializeField]
    string databaseUrl =
        "https://shingutest-68112-default-rtdb.asia-southeast1.firebasedatabase.app/";

    [SerializeField] Text messageText;
    [SerializeField] Transform content;
    [SerializeField] MarketItemView itemPrefab;


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        database = FirebaseDatabase.GetInstance(databaseUrl);
        reference = database.RootReference;
        dispatcher = UnityMainThreadDispatcher.Instance();
    }

    public void OnClickSellDrink()
    {
        SellItem("Drink", 100);
    }

    public void OnClickSellCookie()
    {
        SellItem("Cookie", 200);
    }

    public void OnClickSellJelly()
    {
        SellItem("Jelly", 50);
    }


    public void SellItem(string itemName, int price)
    {
        UserData userData = UserDataManager.Instance.CurrentUserData;

        if (!userData.Inventory.ContainsKey(itemName) ||
            userData.Inventory[itemName] <= 0)
        {
            messageText.text = "아이템이 부족합니다.";
            return;
        }

        userData.Inventory[itemName]--;

        string userKey = PlayerPrefs.GetString("UserKey");
        string nickName = PlayerPrefs.GetString("UserNickName");

        DatabaseReference marketRef =
            reference.Child("Market").Push();

        string listingKey = marketRef.Key;

        Dictionary<string, object> updateData =
            new Dictionary<string, object>();

        // 인벤토리 감소
        updateData[$"UserInfo/{userKey}/Inventory/{itemName}"]
            = userData.Inventory[itemName];

        // 거래소 등록
        updateData[$"Market/{listingKey}/ListingKey"]
            = listingKey;

        updateData[$"Market/{listingKey}/SellerKey"]
            = userKey;

        updateData[$"Market/{listingKey}/SellerNickName"]
            = nickName;

        updateData[$"Market/{listingKey}/ItemName"]
            = itemName;

        updateData[$"Market/{listingKey}/Price"]
            = price;

        updateData[$"Market/{listingKey}/IsSold"]
            = false;

        updateData[$"Market/{listingKey}/ListedAt"]
            = ServerValue.Timestamp;

        messageText.text  = $"{itemName} 등록 완료";

        reference.UpdateChildrenAsync(updateData);

        RefreshMarket();
    }


    public void BuyItem(string listingKey, MarketItemData marketData)
    {
        string myKey = PlayerPrefs.GetString("UserKey");

        if (myKey == marketData.SellerKey)
        {
            messageText.text = "자신의 상품은 구매할 수 없습니다.";
            return;
        }

        UserData userData = UserDataManager.Instance.CurrentUserData;

        if (userData.Coin < marketData.Price)
        {
            messageText.text = "코인이 부족합니다.";
            return;
        }

        userData.Coin -= marketData.Price;

        if (userData.Inventory.ContainsKey(
            marketData.ItemName))
        {
            userData.Inventory[
                marketData.ItemName]++;
        }
        else
        {
            userData.Inventory[
                marketData.ItemName] = 1;
        }

        UserDataManager.Instance.SaveUserData();

        GiveCoinToSeller(
            marketData.SellerKey,
            marketData.Price);

        reference
            .Child("Market")
            .Child(listingKey)
            .RemoveValueAsync();

        RefreshMarket();
    }

    void GiveCoinToSeller(
     string sellerKey,
     int coin)
    {
        reference
            .Child("UserInfo")
            .Child(sellerKey)
            .Child("Coin")
            .GetValueAsync()
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    return;
                }

                int currentCoin =
                    int.Parse(
                        task.Result.Value.ToString());

                reference
                    .Child("UserInfo")
                    .Child(sellerKey)
                    .Child("Coin")
                    .SetValueAsync(
                        currentCoin + coin);
            });
    }

    public void RefreshMarket()
    {
        LoadMarket(OnMarketLoaded);

        InventoryManager.Instance.RefreshUI();

        ShopManager.Instance.RefreshUI();
    }

    void OnMarketLoaded(List<(string, MarketItemData)> marketItems)
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        foreach (var item in marketItems)
        {
            MarketItemView view = Instantiate(itemPrefab, content);

            view.Initialize(item.Item1,item.Item2);
        }
    }
    public void LoadMarket(System.Action<List<(string, MarketItemData)>> callback)
    {
        reference
            .Child("Market")
            .GetValueAsync()
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    return;
                }

                List<(string, MarketItemData)> result =
                    new();

                foreach (DataSnapshot item in task.Result.Children)
                {
                    MarketItemData data = new MarketItemData();

                    data.SellerKey =
                        item.Child("SellerKey").Value.ToString();

                    data.SellerNickName =
                        item.Child("SellerNickName").Value.ToString();

                    data.ItemName =
                        item.Child("ItemName").Value.ToString();

                    data.Price =
                        int.Parse(item.Child("Price").Value.ToString());

                    data.CreateTime =
                        long.Parse(item.Child("ListedAt").Value.ToString());

                    result.Add(
                        (item.Key, data));
                }

                dispatcher.Enqueue(() =>
                {
                    callback?.Invoke(result);
                });
            });
    }
}