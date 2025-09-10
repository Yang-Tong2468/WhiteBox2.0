using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class PlayerMoverController : MonoBehaviour
{
    public float speed = 5f;
    public float zMin = -2f, zMax = 2f;

    Rigidbody rb;
    void Awake() { rb = GetComponent<Rigidbody>(); }

    void FixedUpdate()
    {
        float h = Input.GetAxisRaw("Horizontal"); 
        float v = Input.GetAxisRaw("Vertical");  
        Vector3 dir = new Vector3(h, 0, v).normalized;

        dir.z = Mathf.Clamp(dir.z, zMin, zMax);

        rb.MovePosition(rb.position + dir * speed * Time.fixedDeltaTime);
    }
}
