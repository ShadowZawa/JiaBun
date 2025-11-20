using System.Collections.Generic;
using UnityEngine;



[System.Serializable]
public class GPSRecievedEvent
{
    public float latitude;
    public float longitude;
    public double altitude;
    public float horizontalAccuracy;
    public double timestamp;

    public GPSRecievedEvent(float lat, float lon, double alt = 0, float accuracy = 0, double time = 0)
    {
        latitude = lat;
        longitude = lon;
        altitude = alt;
        horizontalAccuracy = accuracy;
        timestamp = time;
    }
    public override string ToString()
    {
        return $"GPS位置: ({latitude:F6}, {longitude:F6}), 海拔: {altitude}, 精確度: {horizontalAccuracy}";
    }
}
public class showMessageBoxEvent
{
    public string message;
    public Color color=Color.red;
    public int time=3;
    public showMessageBoxEvent(string msg, Color? col = null, int? t = 3)
    {
        message = msg;
        color = col ?? Color.red;
        time = t ?? 3;
    }
}
public class RestaurantDataReceivedEvent
{
    public RequestPlacesModel places;
    public string message;
    public RestaurantDataReceivedEvent(RequestPlacesModel model, string msg)
    {
        places = model;
        message = msg;
    }
}
public class RestaurantConversationRecievedEvent
{
    public string oldMessage;
    public ConversationResponseModel conversation;
    public RestaurantConversationRecievedEvent(ConversationResponseModel model, string msg)
    {
        conversation = model;
        oldMessage = msg;
    }
}