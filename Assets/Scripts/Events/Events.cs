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