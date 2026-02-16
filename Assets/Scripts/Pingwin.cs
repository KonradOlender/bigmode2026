using UnityEngine;

public class Pingwin : MonoBehaviour
{
         public AK.Wwise.Event PenguinEvent;
         public AK.Wwise.Event PenguinTalkEvent;

    public Rigidbody rb;
    public float speed = 5;

    public Rigidbody playerRB;
    public float boostForce = 3;

    public GameObject model;
    public ParticleSystem particle;

    public Gamemanager gamemanager;

    void Start()
    {
        //rb = GetComponent<Rigidbody>();
        PenguinTalkEvent.Post(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        //rb.linearVelocity = Vector3.forward * speed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            playerRB.AddForce((playerRB.linearVelocity.normalized * -1) * boostForce, ForceMode.Impulse);
            model.SetActive(false);
            particle.Play();
            gamemanager.AddPingwinKilled();
            PenguinEvent.Post(gameObject);
        }
    }
}
