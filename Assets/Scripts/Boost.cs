using UnityEngine;

public class Boost : MonoBehaviour
{
    public Gamemanager gamemanager;
    public string triggerTag = "Player";
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
        if(other.tag == "triggerTag")
        {
            gamemanager.AddTime(timeGain);
        }
    }
}
