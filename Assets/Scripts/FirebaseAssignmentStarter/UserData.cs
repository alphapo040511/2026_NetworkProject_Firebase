using System.Collections.Generic;
using Newtonsoft.Json;

[System.Serializable]
public class UserData
{
    public string NickName;
    public int Coin;
    public int Score;

    public string UnitList;

    public Dictionary<string, int> Inventory;

    public UserData()
    {
    }

    public UserData(string nickName)
    {
        NickName = nickName;
        Coin = 500;
        Score = 0;

        Dictionary<string, bool> unitList =
            new Dictionary<string, bool>();

        unitList["Unit1"] = true;

        for (int i = 2; i <= 6; i++)
        {
            unitList["Unit" + i] = false;
        }

        Inventory =
            new Dictionary<string, int>();

        Inventory["Drink"] = 0;
        Inventory["Cookie"] = 0;
        Inventory["Jelly"] = 0;

        UnitList = JsonConvert.SerializeObject(unitList);
    }
}
