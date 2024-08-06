using System;
using UnityEngine.Purchasing;

public enum ProductType
{
    Consumable = 0,
    NonConsumable = 1,
    Subscription = 2
}

public enum IAPOperationStatus
{
    Success,
    Fail
}

[Serializable]
public class StoreProduct
{
    public string productName;
    public ProductType productType;
    public string idGooglePlay;
    public string idAmazon;
    public string idIOS;
    public string idMac;
    public string idWindows;
    public int value;
    public string localizedPriceString = "-";
    public int price;
    public string isoCurrencyCode;
    public string receipt;
    internal bool active;
    internal string localizedDescription;
    internal string localizedTitle;
    internal SubscriptionInfo subscriptionInfo;


    public StoreProduct(string productName, ProductType productType, int value, string idGooglePlay, string idIOS,
        string idAmazon, string idMac, string idWindows)
    {
        this.productName = productName;
        this.productType = productType;
        this.value = value;
        this.idGooglePlay = idGooglePlay;
        this.idIOS = idIOS;
        this.idAmazon = idAmazon;
        this.idMac = idMac;
        this.idWindows = idWindows;
    }

    public StoreProduct()
    {
        productName = "";
        idGooglePlay = "";
        idIOS = "";
        idAmazon = "";
        idMac = "";
        idWindows = "";
        productType = ProductType.Consumable;
    }

    internal UnityEngine.Purchasing.ProductType GetProductType()
    {
        return (UnityEngine.Purchasing.ProductType)(int)productType;
    }

    internal string GetStoreID()
    {
#if IAPMacOS
        return idMac;
#elif IAPiOS
        return idIOS;
#elif IAPGooglePlay
        return idGooglePlay;
#elif IAPAmazon
        return idAmazon;
#elif IAPWindows
        return idWindows;
#else
        return "";
#endif
    }
}