using UnityEngine;
using UnityEngine.UI;

public class MarketItemView : MonoBehaviour
{
    [SerializeField] Text sellerText;
    [SerializeField] Text itemText;
    [SerializeField] Text priceText;

    string listingKey;
    MarketItemData marketData;

    public void Initialize(string key,MarketItemData data)
    {
        listingKey = key;
        marketData = data;

        sellerText.text = $"판매자 : {data.SellerNickName}";

        itemText.text = $"아이템 - {data.ItemName}";

        priceText.text = "구매 " + data.Price.ToString() + "C";
    }

    public void OnClickBuy()
    {
        MarketManager.Instance.BuyItem(
            listingKey,
            marketData);
    }
}