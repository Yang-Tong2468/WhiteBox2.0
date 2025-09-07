/*
控制玩家移动
WSAD以及上下左右均可控制
*/
using UnityEngine;

public class PlayerMover : MonoBehaviour
{
    public float moveSpeed;
    public float Xmax;
    public float Xmin;
    public float Zmax;
    public float Zmin;

    void Update()
    {
        float h = Input.GetAxis("Horizontal"); // A/D 或 ←/→
        float v = Input.GetAxis("Vertical");   // W/S 或 ↑/↓

        Vector3 move = new Vector3(h, 0, v) * moveSpeed * Time.deltaTime;
        transform.Translate(move, Space.World);

        // 限制X轴和Z轴范围
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, Xmin, Xmax);
        pos.z = Mathf.Clamp(pos.z, Zmin, Zmax);
        transform.position = pos;
    }
}
