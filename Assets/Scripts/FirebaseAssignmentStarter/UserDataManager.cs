using Firebase.Database;
using Newtonsoft.Json;
using PimDeWitte.UnityMainThreadDispatcher;
using System;
using System.Collections.Generic;
using UnityEngine;

public class UserDataManager : MonoBehaviour
{
    public static UserDataManager Instance;

    [Header("Firebase")]
    [SerializeField]
    string databaseUrl =
        "https://shingutest-68112-default-rtdb.asia-southeast1.firebasedatabase.app/";

    FirebaseDatabase database;
    DatabaseReference reference;
    UnityMainThreadDispatcher dispatcher;

    public UserData CurrentUserData { get; private set; }

    string userKey;
    public bool IsLoaded { get; private set; }

    public event Action OnDataLoaded;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        database = FirebaseDatabase.GetInstance(databaseUrl);
        reference = database.RootReference;
        dispatcher = UnityMainThreadDispatcher.Instance();

        userKey = PlayerPrefs.GetString("UserKey");

        if (string.IsNullOrEmpty(userKey))
        {
            Debug.LogError("UserKey가 없습니다.");
            return;
        }

        LoadUserData();
    }

    public void LoadUserData()
    {
        reference
            .Child("UserInfo")
            .Child(userKey)
            .GetValueAsync()
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("유저 데이터 로드 실패");
                    return;
                }

                DataSnapshot snapshot = task.Result;

                UserData userData = new UserData();

                userData.NickName = snapshot.Child("NickName").Value.ToString();

                userData.Coin = int.Parse(snapshot.Child("Coin").Value.ToString());

                userData.Score = int.Parse(snapshot.Child("Score").Value.ToString());

                userData.UnitList = snapshot.Child("UnitList").Value.ToString();


                userData.Inventory =  new Dictionary<string, int>();

                DataSnapshot inventorySnapshot = snapshot.Child("Inventory");

                foreach (DataSnapshot item in inventorySnapshot.Children)
                {
                    userData.Inventory[item.Key] =
                        int.Parse(item.Value.ToString());
                }

                CurrentUserData = userData;
                IsLoaded = true;

                dispatcher.Enqueue(() =>
                {
                    Debug.Log("유저 데이터 로드 완료");
                    OnDataLoaded?.Invoke();
                });
            });
    }

    public void SaveUserData()
    {
        if (CurrentUserData == null)
            return;

        Dictionary<string, object> updateData =
            new Dictionary<string, object>();

        updateData["Coin"] = CurrentUserData.Coin;
        updateData["Score"] = CurrentUserData.Score;
        updateData["UnitList"] = CurrentUserData.UnitList;

        // 인벤토리는 아이템 별로 따로 저장
        foreach (var item in CurrentUserData.Inventory)
        {
            updateData[$"Inventory/{item.Key}"] =
                item.Value;
        }

        reference
            .Child("UserInfo")
            .Child(userKey)
            .UpdateChildrenAsync(updateData)
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("유저 데이터 저장 실패");
                    return;
                }

                dispatcher.Enqueue(() =>
                {
                    Debug.Log("유저 데이터 저장 완료");
                });
            });
    }
}