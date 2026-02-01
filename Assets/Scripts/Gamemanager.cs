using UnityEngine;
using UnityEngine.Events;

public class Gamemanager : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] private float timeLimit = 60f;
    [SerializeField] private bool startOnAwake = true;

    [Header("Timer State")]
    public float currentTime;
    public bool isTimerRunning;

    [Header("Events")]
    public UnityEvent onTimerStart;
    public UnityEvent onTimerEnd;
    public UnityEvent<float> onTimerUpdate;

    private void Start()
    {
        currentTime = timeLimit;

        if (startOnAwake)
        {
            StartTimer();
        }
    }

    private void Update()
    {
        if (isTimerRunning)
        {
            UpdateTimer();
        }
    }

    private void UpdateTimer()
    {
        currentTime -= Time.deltaTime;

        onTimerUpdate?.Invoke(currentTime);

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            StopTimer();
            onTimerEnd?.Invoke();
        }
    }

    public void StartTimer()
    {
        isTimerRunning = true;
        onTimerStart?.Invoke();
    }

    public void StopTimer()
    {
        isTimerRunning = false;
    }

    public void ResetTimer()
    {
        currentTime = timeLimit;
        isTimerRunning = false;
    }

    public void AddTime(float additionalTime)
    {
        currentTime += additionalTime;
    }

    public string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
