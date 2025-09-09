using UnityEngine;
public class ParallaxLayer : MonoBehaviour
{
    public Transform cam;
    public float parallaxMultiplier = 0.2f; // 远层取小，近层取大
    Vector3 startPos;
    void Start() { if (!cam) cam = Camera.main.transform; startPos = transform.position; }
    void LateUpdate()
    {
        var delta = cam.position - startPos;
        transform.position = startPos + new Vector3(delta.x * parallaxMultiplier, 0, 0);
    }
}
