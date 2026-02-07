using UnityEngine;

public class Soundmanager : MonoBehaviour
{
    public static Soundmanager Instance;

    public AudioSource rideSound;
    public AudioSource rideNoice;
    public AudioSource rideEnd;
    public AudioSource checkPoint;
    public AudioSource boost;
    public AudioSource meta;
    public AudioSource lose;

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

    }

    // Update is called once per frame
    void Update()
    {
        
    }

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

    public void CheckPointPlay()
    {
        checkPoint.Play();
    }

    public void BoostPlay()
    {
        boost.Play();
    }
    public void metaPlay()
    {
        meta.Play();
    }

    public void LosePlay()
    {
        lose.Play();
    }


}
