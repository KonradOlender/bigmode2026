using System.Collections;
using UnityEngine;

public class PingwinSpawner : MonoBehaviour
{
    public GameObject spawningObject;
    public Transform spawnPoint;
    public float spawnInterval;

    private int animIndex = 0;

    private bool canSpawn = true;
    void Start()
    {
        
    }

    void Update()
    {
        if (canSpawn)
        {
            var spawnedObject = Instantiate(spawningObject, spawnPoint.position, spawnPoint.rotation);
            var anim = spawnedObject.GetComponent<Animator>();
            var script = spawnedObject.GetComponent<Destroy>();
            script.enabledValue = true;
            if(animIndex % 2 == 0)
            {
                anim.Play("Idle");
            }
            else
            {
                anim.Play("Atack");
            }
            animIndex++;
            StartCoroutine(spawnCooldown());
        }
    }

    public IEnumerator spawnCooldown()
    {
        canSpawn = false;
        yield return new WaitForSeconds(spawnInterval);
        canSpawn = true;
    }
}
