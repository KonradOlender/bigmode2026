using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    public Gamemanager gamemanager;
    public float timeGain = 15;
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
            gamemanager.AddTime(timeGain);
        }
    }
}
