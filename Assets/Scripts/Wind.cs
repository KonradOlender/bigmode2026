using UnityEngine;
using AK.Wwise;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody))]
public class WwiseSpeedRTPC : MonoBehaviour
{
    [Header("Wwise")]
    public AK.Wwise.Event movementEvent; // Event used
    public RTPC speedRTPC; // RTPC used

    [Header("Settings")]
    public float maxSpeed = 15f;    // ustaw takie samo jak w BallCharacter
    public bool playOnStart = true;
    public bool normalizeTo100 = false; // jeśli RTPC w Wwise ma zakres 0–100

    private Rigidbody rb;
    private uint playingID; // ID odtwarzanego eventu

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        if (playOnStart && movementEvent != null)
        {
            // Odtwarzamy event i zapisujemy jego PlayingID
            playingID = movementEvent.Post(gameObject);
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

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Zatrzymujemy event przez AkSoundEngine.ExecuteActionOnEvent
        if (movementEvent != null)
        {
            AkSoundEngine.ExecuteActionOnEvent(
                movementEvent.Id,
                AkActionOnEventType.AkActionOnEventType_Stop,
                gameObject,
                0,
                AkCurveInterpolation.AkCurveInterpolation_Linear
            );
        }

        // Dodatkowo zatrzymujemy wszystkie dźwięki powiązane z tym gameObject
        AkSoundEngine.StopAll(gameObject);
    }
}