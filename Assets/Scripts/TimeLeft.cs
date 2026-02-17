using UnityEngine;
using AK.Wwise;

public class TimeLeft : MonoBehaviour
{
    [Header("Wwise Event / RTPC")]
    public AK.Wwise.Event timeLeftEvent; // Event to play the time left sound
    public RTPC timeLeftRTPC; // RTPC to control the remaining time parameter

    [Header("Dependencies")]
    public Gamemanager gameManager; // Reference to the GameManager

    private void Start()
    {
        if (timeLeftEvent != null)
        {
            timeLeftEvent.Post(gameObject);
        }
    }

    private void Update()
    {
        if (timeLeftEvent != null && gameManager != null)
        {
            // Dynamically adjust the event value based on current time
            float currentTime = gameManager.currentTime;
            timeLeftRTPC.SetValue(gameObject, currentTime);
        }
    }
}
