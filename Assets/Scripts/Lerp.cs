using UnityEngine;

public class Lerp : MonoBehaviour
{
    public Transform positionA;
    public Transform positionB;

    public float lerpSpeed = 2f;
    private bool moveToB;

    void Update()
    {
        Transform target = moveToB ? positionB : positionA;
        transform.position = Vector3.Lerp(
            transform.position,
            target.position,
            Time.deltaTime * lerpSpeed
        );
    }

    public void ToggleCameraPosition()
    {
        moveToB = !moveToB;
    }
}
