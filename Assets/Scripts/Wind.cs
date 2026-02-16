using UnityEngine;
using AK.Wwise;

[RequireComponent(typeof(Rigidbody))]
public class WwiseSpeedRTPC : MonoBehaviour
{
    [Header("Wwise")]
    public AK.Wwise.Event movementEvent;     // <-- TU wklejasz Event
    public RTPC speedRTPC;          // <-- TU wklejasz RTPC "Speed"

    [Header("Settings")]
    public float maxSpeed = 15f;    // ustaw takie samo jak w BallCharacter
    public bool playOnStart = true;
    public bool normalizeTo100 = false; // zaznacz jeśli RTPC w Wwise ma zakres 0–100

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        if (playOnStart && movementEvent != null)
        {
            movementEvent.Post(gameObject);
        }
    }

    private void Update()
    {
        if (speedRTPC == null) return;

        float currentSpeed = rb.linearVelocity.magnitude;

        if (normalizeTo100)
        {
            float normalized = Mathf.Clamp((currentSpeed / maxSpeed) * 100f, 0f, 100f);
            speedRTPC.SetValue(gameObject, normalized);
        }
        else
        {
            speedRTPC.SetValue(gameObject, currentSpeed);
        }
    }
}