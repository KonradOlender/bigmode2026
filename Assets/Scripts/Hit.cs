using UnityEngine;
using AK.Wwise;

public class CollisionSound : MonoBehaviour
{
    [Header("Wwise")]
    public AK.Wwise.Event collisionEvent;
    public AK.Wwise.RTPC speedRTPC;

    [Header("RTPC Settings")]
    public float maxSpeed = 15f;
    public bool normalizeTo100 = true;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();

            if (playerRb != null && speedRTPC != null)
            {
                float currentSpeed = playerRb.linearVelocity.magnitude;

                if (normalizeTo100)
                {
                    float normalized = Mathf.Clamp((currentSpeed / maxSpeed) * 100f, 0f, 100f);
                    speedRTPC.SetValue(gameObject, normalized);
                }
                else
                {
                    speedRTPC.SetValue(gameObject, currentSpeed);
                }
            }

            collisionEvent.Post(gameObject);
        }
    }
}