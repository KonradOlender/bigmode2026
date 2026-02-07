using UnityEngine;

public class Boost : MonoBehaviour
{
    public Gamemanager gamemanager;
    public string triggerTag = "Player";
    public float boostForce = 15;

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
            Soundmanager.Instance.BoostPlay();
            ball = other.GetComponent<BallCharacter>();
            ball.AddBoost(boostForce);
        }
    }
}
