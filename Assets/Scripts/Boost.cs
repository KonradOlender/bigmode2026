using UnityEngine;

public class Boost : MonoBehaviour
{
    public Gamemanager gamemanager;
    public string triggerTag = "Player";
    public float boostForce = 15;
         public AK.Wwise.Event checkpointEvent;


    private BallCharacter ball;
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
        if(other.tag == triggerTag)
        {
            
            ball = other.GetComponent<BallCharacter>();
            ball.AddBoost(boostForce);
            checkpointEvent.Post(gameObject);

        }
    }
}
