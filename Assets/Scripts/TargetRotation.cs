using UnityEngine;

public class TargetRotation : MonoBehaviour
{
    public float speed = 200f;

    void Update()
    {
        transform.Rotate(0, 0, speed * Time.deltaTime);
    }
}