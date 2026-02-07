using System.Collections;
using UnityEngine;

public class Destroy : MonoBehaviour
{
    public float delay;
    public bool enabledValue = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(DestroyWithDelay());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator DestroyWithDelay()
    {
        yield return new WaitForSeconds(delay);
        if (enabledValue)
        {
            Destroy(this.gameObject);
        }
    }
}
