

using System.Collections.Generic;

[System.Serializable]
public class RequestPlacesModel
{
    List<PlaceModel> places;
}
[System.Serializable]
public class PlaceModel
{
    public string displayName;
    public float rating;
    public string googleMapsUri;
    public bool openNow;
    public float startPrice;
    public float endPrice;
    public string currencyCode;
}