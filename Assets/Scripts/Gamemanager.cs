using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class Gamemanager : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] private float timeLimit = 60f;
    [SerializeField] private bool startOnAwake = true;

    [Header("Timer State")]
    public float currentTime;
    public bool isTimerRunning;

    [Header("UI")]
    public TMP_Text timerText;
    public GameObject uiElementWinScreen;
    public GameObject uiElementFailedScreen;
    public GameObject uiElementClock;


    [Header("Events")]
    public UnityEvent onTimerStart;
    public UnityEvent onTimerEnd;
    public UnityEvent<float> onTimerUpdate;

    private void Start()
    {
        uiElementWinScreen.SetActive(false);
        uiElementFailedScreen.SetActive(false);
        currentTime = timeLimit;
        timerText.text = "00:00";

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

        timerText.text = GetFormattedTime();

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            StopTimer();
            Failed();
        }
    }

    public void StartTimer()
    {
        uiElementClock.SetActive(true);
        isTimerRunning = true;
        onTimerStart?.Invoke();
    }

    public void StopTimer()
    {
        isTimerRunning = false;
        onTimerEnd?.Invoke();
    }

    public void ResetTimer()
    {
        currentTime = timeLimit;
        isTimerRunning = false;
    }

    public void AddTime(float additionalTime)
    {
        Debug.Log("AddTie");
        Debug.Log(currentTime);
        currentTime += additionalTime;
        Debug.Log(currentTime);
    }

    public string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void Win()
    {
        Debug.Log("Win");
        StopTimer();
        uiElementClock.SetActive(false);
        uiElementWinScreen.SetActive(true);
    }
    public void Failed()
    {
        Debug.Log("YouSuck!");
        uiElementClock.SetActive(false);
        uiElementFailedScreen.SetActive(true);
    }
}
