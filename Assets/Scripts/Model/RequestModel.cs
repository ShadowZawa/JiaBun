using System.Collections.Generic;

/// <summary>
/// 餐廳清單回應資料模型
/// </summary>
[System.Serializable]
public class RequestPlacesModel
{
    public List<PlaceModel> places;
}

/// <summary>
/// 單一餐廳資料模型
/// </summary>
[System.Serializable]
public class PlaceModel
{
    public string displayName;
    public float? rating;  // nullable，因為可能為 null
    public string googleMapsUri;
    public bool? openNow;  // nullable，因為可能為 null
    public string startPrice;  // 改為 string，因為 API 回傳的是字串格式
    public string endPrice;    // 改為 string，因為 API 回傳的是字串格式
    public string currencyCode;

    /// <summary>
    /// 取得開始價格（數字格式）
    /// </summary>
    public float GetStartPriceAsFloat()
    {
        if (string.IsNullOrEmpty(startPrice))
            return 0f;
        
        float.TryParse(startPrice, out float result);
        return result;
    }

    /// <summary>
    /// 取得結束價格（數字格式）
    /// </summary>
    public float GetEndPriceAsFloat()
    {
        if (string.IsNullOrEmpty(endPrice))
            return 0f;
        
        float.TryParse(endPrice, out float result);
        return result;
    }

    /// <summary>
    /// 取得價格範圍文字
    /// </summary>
    public string GetPriceRangeText()
    {
        if (string.IsNullOrEmpty(startPrice) && string.IsNullOrEmpty(endPrice))
            return "價格未提供";
        
        if (!string.IsNullOrEmpty(currencyCode))
            return $"{currencyCode} {startPrice} - {endPrice}";
        
        return $"{startPrice} - {endPrice}";
    }

    /// <summary>
    /// 檢查餐廳是否營業中
    /// </summary>
    public bool IsOpen()
    {
        return openNow.HasValue && openNow.Value;
    }

    /// <summary>
    /// 取得評分文字
    /// </summary>
    public string GetRatingText()
    {
        if (rating.HasValue)
            return rating.Value.ToString("F1");
        
        return "無評分";
    }
}

/// <summary>
/// AI 對話請求的資料模型
/// </summary>
[System.Serializable]
public class ConversationRequestModel
{
    public string PreviousData;
    public List<ConversationChoice> choice;
}

/// <summary>
/// AI 對話請求中的單一選項
/// </summary>
[System.Serializable]
public class ConversationChoice
{
    public string ID;
    public string displayName;
    public float? rating;
    public string priceLevel;
    public bool? openNow;

    /// <summary>
    /// 從 PlaceModel 建立 ConversationChoice
    /// </summary>
    public static ConversationChoice FromPlaceModel(PlaceModel place, int id)
    {
        return new ConversationChoice
        {
            ID = id.ToString(),
            displayName = place.displayName,
            rating = place.rating,
            priceLevel = place.GetPriceRangeText(),
            openNow = place.openNow
        };
    }
}

/// <summary>
/// AI 對話回應的資料模型
/// </summary>
[System.Serializable]
public class ConversationResponseModel
{
    public string newData;
    public string resultIndex;
    public string resultConversation;
}