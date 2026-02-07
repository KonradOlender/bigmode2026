using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour

{
    public Gamemanager gamemanager;
    public bool isStop;

     public AK.Wwise.Event checkpointEvent;

    public float timeGain = 15;
    public List<ParticleSystem> particles;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            if (isStop)
            {
                Stop();
            }
            else
            {
                Checkpoint();
            }
        }
    }

    private void Checkpoint()
    {
       checkpointEvent.Post(gameObject);
        gamemanager.AddTime(timeGain);
        foreach (var part in particles)
        {
            part.Play();
        }
    }
    private void Stop()
    {
        gamemanager.Win();
    }
}
