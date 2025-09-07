/*
实施点击时镜头缩放和拉远的效果
*/
using UnityEngine;

public class ZoomByClicking : MonoBehaviour
{
    public CameraController cameraController; // 引用相机控制脚本

    private void OnMouseDown()
    {
        if (cameraController != null)
        {
            cameraController.FocusOnNPC(transform); // 将 NPC 的 Transform 传递给相机控制脚本
        }
    }
}