using UnityEngine;

public class Soundmanager : MonoBehaviour
{
    public static Soundmanager Instance;

    public AudioSource rideSound;
    public AudioSource rideEnd;

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
    }
    public void RideSoundStop()
    {
        rideSound.Stop();
    }

    public void RideEndPlay()
    {
        rideEnd.Play();
    }
    public void RideEndStop()
    {
        rideEnd.Stop();
    }


}
