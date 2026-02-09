using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System;

public class Gamemanager : MonoBehaviour
{
    public DataBase levelDataBase;
    public AK.Wwise.Event checkpointEvent;
     public AK.Wwise.Event SliderEvent;
     public AK.Wwise.Event SliderDropEvent;



    public BallCharacter ball;
    [Header("Timer Settings")]
    [SerializeField] private float timeLimit = 60f;

    [Header("GameState")]
    public bool isPreGame = true;
    public bool isFiled = false;

    [Header("Timer State")]
    public float currentTime;
    public bool isTimerRunning;

    [Header("UI")]
    public TMP_Text timerText;
    public GameObject uiElementWinScreen;
    public GameObject uiElementFailedScreen;
    public GameObject uiElementClock;
    public GameObject uiElementSpeedomater;
    public GameObject uimenu;
    public TMP_Text speedText;
    //public Slider speedSlider;
    public Slider pregameSlider;
    public float sliderSpeed;

    [Header("Stats")]
    public TMP_Text timeLeftText;
    public TMP_Text topSpeedText;
    public TMP_Text killedText;
    private int currentKilled = 0;


    [Header("Events")]
    public UnityEvent onTimerStart;
    public UnityEvent onTimerEnd;
    public UnityEvent<float> onTimerUpdate;

    public UnityEvent<bool> onSetPlayerControll;

    [Header("Stats")]
    public float topSpeed = 0;

    [Header("Launch")]
    public float force_0_45;
    public float force_45_65;
    public float force_65_95;
    public float force_95_100;
    public GameObject preGameCamera;


    private bool sliderAdd = true;
    private bool sliderSubtract = true;

    private void Start()
    {
        Soundmanager.Instance.RideSoundStop();
       SliderEvent.Post(gameObject);

        uiElementWinScreen.SetActive(false);
        uiElementFailedScreen.SetActive(false);
        uiElementSpeedomater.SetActive(false);
        uiElementClock.SetActive(false);

        pregameSlider.gameObject.SetActive(true);
        currentTime = timeLimit;
        timerText.text = "00:00";

        //if (startOnAwake)
        //{
        //    StartTimer();
        //}
        onSetPlayerControll?.Invoke(false);
    }

    private void FixedUpdate()
    {
        if (isTimerRunning)
        {
            UpdateTimer();
        }
        else if (isPreGame)
        {
            SliderHandle();
        }
    }

    private void SliderHandle()
    {
        if (sliderAdd)
        {
            if(pregameSlider.value + sliderSpeed >= 1)
            {
                pregameSlider.value = 1;
                sliderAdd = false;
                sliderSubtract = true;
            }
            else
            {
                pregameSlider.value += sliderSpeed;
            }
        }
        else if (sliderSubtract)
        {
            if (pregameSlider.value - sliderSpeed <= 0)
            {
                pregameSlider.value = 0;
                sliderAdd = true;
                sliderSubtract = false;
            }
            else
            {
                pregameSlider.value -= sliderSpeed;
            }
        }
        
    }

    public void getCurrentSliderValue(InputAction.CallbackContext context)
    {
        if (isPreGame)
        {   
            float force = 0;
            if (context.started)
            {
                if (pregameSlider.value >= 0 && pregameSlider.value < 0.45)
                {
                    force = force_0_45;
                }
                else if (pregameSlider.value >= 0.45 && pregameSlider.value < 0.65)
                {
                    force = force_45_65;
                }
                else if (pregameSlider.value >= 0.65 && pregameSlider.value < 0.95)
                {
                    force = force_65_95;
                }
                else if (pregameSlider.value >= 0.95 && pregameSlider.value <= 100)
                {
                    force = force_95_100;
                }
                ball.AddForcePreGame(force);
                isPreGame = false;
                preGameCamera.SetActive(false);
                pregameSlider.gameObject.SetActive(false);
                SliderEvent.Stop(gameObject);
                SliderDropEvent.Post(gameObject);   
                StartTimer();
            }
        }
    }

    public void OpenConsole()
    {
        uimenu.SetActive(!uimenu.activeInHierarchy);
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
        uiElementSpeedomater.SetActive(true);
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
        currentTime += additionalTime;
    }

    public string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime);
        int milliseconds = Mathf.FloorToInt((currentTime - seconds) * 1000);
        return string.Format("{0:00}:{1:00}", seconds, milliseconds);
    }

    public void Win()
    {
        if (!isFiled)
        {
            Soundmanager.Instance.metaPlay();
            StopTimer();
            topSpeedText.text = topSpeedText.text + ((float)Math.Round(topSpeed, 2)).ToString();
            timeLeftText.text = timeLeftText.text + GetFormattedTime();
            killedText.text = killedText.text + currentKilled.ToString();
            uiElementClock.SetActive(false);
            uiElementSpeedomater.SetActive(false);
            uiElementWinScreen.SetActive(true);
        }
        
    }

    public void EndLevelBack()
    {
        Soundmanager.Instance.RideSoundStop();
        levelDataBase.SetLevelData(true, "S", currentTime, GetFormattedTime(), (float)Math.Round(topSpeed, 2));
        SceneManager.LoadScene(0);
    }
    public void EndLevelNext(int levelIndex)
    {
        Soundmanager.Instance.RideSoundStop();
        levelDataBase.SetLevelData(true, "S", currentTime, GetFormattedTime(), (float)Math.Round(topSpeed, 2));
        SceneManager.LoadScene(levelIndex);
    }
    public void Failed()
    {
        Soundmanager.Instance.LosePlay();
        isFiled = true;
        uiElementClock.SetActive(false);
        uiElementSpeedomater.SetActive(false);
        uiElementFailedScreen.SetActive(true);
            checkpointEvent.Post(gameObject);

    }

    public void setSpeedText(float value)
    {
        if(topSpeed < value)
        {
            topSpeed = value;
        }
        speedText.text = value.ToString("F0");
        //if(value / 100 < speedSlider.maxValue)
        //{
        //    speedSlider.value = value / 100;
        //}
        
    }

    public void LoadScene(int sceneIndex)
    {
        SliderEvent.Stop(gameObject);
        SceneManager.LoadScene(sceneIndex);
    }

    public void AddPingwinKilled()
    {
        currentKilled += 1;
        levelDataBase.AddPingwinKilled(1);
    }



}
