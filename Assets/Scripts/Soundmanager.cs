using UnityEngine;
using UnityEngine.UI;

public class Soundmanager : MonoBehaviour
{
    public static Soundmanager Instance;
         public AK.Wwise.Event checkpointEvent;


    public AudioSource musicSound;
    public AudioSource rideSound;
    public AudioSource rideNoice;
    public AudioSource rideEnd;
    public AudioSource boost;
    public AudioSource meta;
    public AudioSource lose;
    public AudioSource pingwinHit;

    public Slider musicVolume;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //checkpointEvent.Post(gameObject);
        musicSound.Play();
    }

    // Update is called once per frame


    public void RideSoundPlay()
    {
        rideSound.Play();
        rideNoice.Play();
    }
    public void RideSoundStop()
    {
        rideSound.Stop();
        rideNoice.Stop();
    }

    public void RideEndPlay()
    {
        rideEnd.Play();
    }





}
