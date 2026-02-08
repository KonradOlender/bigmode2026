using UnityEngine;

public class Pingwin : MonoBehaviour
{
         public AK.Wwise.Event PingwinEvent;

    public Rigidbody rb;
    public float speed = 5;

    public Rigidbody playerRB;
    public float boostForce = 3;

    public GameObject model;
    public ParticleSystem particle;

    void Start()
    {
        //rb = GetComponent<Rigidbody>();
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
            Debug.Log("testtesttest");
            playerRB.AddForce((playerRB.linearVelocity.normalized * -1) * boostForce, ForceMode.Impulse);
            model.SetActive(false);
            particle.Play();
            Debug.Log("Hit");
            PingwinEvent.Post(gameObject);
            Destroy(this.gameObject);
            
        }
    }
}
