using UnityEngine;
using AK.Wwise;

public class TimeLeft : MonoBehaviour
{
    [Header("Wwise Event / RTPC")]
    public AK.Wwise.Event timeLeftEvent; // Event to play the time left sound
    public RTPC timeLeftRTPC; // RTPC to control the remaining time parameter

    private void Start()
    {
        if (timeLeftEvent != null)
        {
            timeLeftEvent.Post(gameObject);
        }
    }

    private void Update()
    {
        if (timeLeftRTPC != null)
        {
            // Example RTPC value update logic
            float rtpcValue = Mathf.Clamp(Time.time, 0, 30); // Replace Time.time with actual value
            timeLeftRTPC.SetValue(gameObject, rtpcValue);
        }
    }
}
