using Firebase.Database;
using Newtonsoft.Json;
using PimDeWitte.UnityMainThreadDispatcher;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    FirebaseDatabase database;
    DatabaseReference reference;
    UnityMainThreadDispatcher dispatcher;

    [Header("Firebase")]
    [SerializeField] string databaseUrl = "https://shingutest-68112-default-rtdb.asia-southeast1.firebasedatabase.app/";

    [Header("UI")]
    [SerializeField] Text DrinkCountText;
    [SerializeField] Text CookieCountText;
    [SerializeField] Text JellyCountText;
    [SerializeField] Text MessageText;

    string userKey;
    public Dictionary<string, int> inventory = new Dictionary<string, int>();

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        database = FirebaseDatabase.GetInstance(databaseUrl);
        reference = database.RootReference;
        dispatcher = UnityMainThreadDispatcher.Instance();

        userKey = PlayerPrefs.GetString("UserKey");

        if (string.IsNullOrEmpty(userKey))
        {
            MessageText.text = "로그인 정보가 없습니다.";
            return;
        }

        LoadInventory();
    }

    void LoadInventory()
    {
        reference
            .Child("UserInfo")
            .Child(userKey)
            .Child("Inventory")
            .GetValueAsync()
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    dispatcher.Enqueue(() =>
                    {
                        MessageText.text = "인벤토리 불러오기 실패";
                    });
                    return;
                }

                if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;

                    if (snapshot.Value == null)
                    {
                        dispatcher.Enqueue(() =>
                        {
                            MessageText.text = "인벤토리 데이터가 없습니다.";
                        });
                        return;
                    }

                    string inventoryJson = snapshot.Value.ToString();
                    inventory = JsonConvert.DeserializeObject<Dictionary<string, int>>(inventoryJson);

                    dispatcher.Enqueue(() =>
                    {
                        RefreshUI();
                        MessageText.text = "인벤토리 불러오기 완료";
                    });
                }
            });
    }

    public void RefreshUI()
    {
        DrinkCountText.text = "Drink : " + GetItemCount("Drink");
        CookieCountText.text = "Cookie : " + GetItemCount("Cookie");
        JellyCountText.text = "Jelly : " + GetItemCount("Jelly");
    }

    int GetItemCount(string itemName)
    {
        if (inventory.ContainsKey(itemName))
        {
            return inventory[itemName];
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
        if (!inventory.ContainsKey(itemName) || inventory[itemName] <= 0)
        {
            MessageText.text = itemName + " 개수가 부족합니다.";
            return;
        }

        inventory[itemName]--;
        SaveInventory(itemName, usingMessage);
    }

    void SaveInventory(string usedItemName, string usingMessage)
    {
        string inventoryJson = JsonConvert.SerializeObject(inventory);

        reference
            .Child("UserInfo")
            .Child(userKey)
            .Child("Inventory")
            .SetValueAsync(inventoryJson)
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    dispatcher.Enqueue(() =>
                    {
                        MessageText.text = "인벤토리 저장 실패";
                    });
                    return;
                }

                dispatcher.Enqueue(() =>
                {
                    RefreshUI();
                    MessageText.text = $"[{usedItemName} 사용 완료] {usingMessage}";
                });
            });
    }
}
