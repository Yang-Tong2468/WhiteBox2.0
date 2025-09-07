// /*
// 点击player镜头拉近
// 点击Esc镜头还原
// 可设置拉近的距离与速度
// */
// using UnityEngine;

// public class CameraController : MonoBehaviour
// {
//     public Transform cameraTransform; // 相机的 Transform
//     public float zoomSpeed;      // 缩放速度
//     public float targetDistance; // 拉近距离
//     public float targetHeight; // NPC 的目标高度
//     public float maxZoomTime;    // 最大缩放时间（决定最大放大倍数）

//     private Transform targetNPC;      // 当前聚焦的 NPC
//     private Vector3 originalPosition; // 相机初始位置
//     private Quaternion originalRotation; // 相机初始旋转
//     private bool isZoomedIn = false;  // 是否已经拉近
//     private float zoomTimer = 0f;     // 缩放计时器
//     private bool isZoomFinished = false; // 是否已完成缩放

//     private void Start()
//     {
//         originalPosition = cameraTransform.position;
//         originalRotation = cameraTransform.rotation;
//     }

//     private void Update()
//     {
//         // 按下Esc恢复
//         if (isZoomedIn && Input.GetKeyDown(KeyCode.Escape))
//         {
//             ResetCamera();
//             return;
//         }

//         if (targetNPC != null && isZoomedIn && !isZoomFinished)
//         {
//             zoomTimer += Time.deltaTime * zoomSpeed;
//             float easeProgress = Mathf.Clamp01(zoomTimer / maxZoomTime);

//             // 计算目标位置和旋转
//             Vector3 focusPosition = targetNPC.position + new Vector3(0, targetHeight, 0);
//             Vector3 targetPosition = focusPosition - cameraTransform.forward * targetDistance;

//             cameraTransform.position = Vector3.Lerp(originalPosition, targetPosition, easeProgress);
//             cameraTransform.rotation = Quaternion.Slerp(originalRotation, Quaternion.LookRotation(focusPosition - targetPosition), easeProgress);

//             if (easeProgress >= 1f)
//             {
//                 isZoomFinished = true; // 缩放完成
//             }
//         }
//     }

//     public void FocusOnNPC(Transform npcTransform)
//     {
//         targetNPC = npcTransform;
//         isZoomedIn = true;
//         isZoomFinished = false;
//         zoomTimer = 0f; // 开始缩放
//     }

//     public void ResetCamera()
//     {
//         targetNPC = null;
//         isZoomedIn = false;
//         isZoomFinished = false;
//         zoomTimer = 0f;

//         cameraTransform.position = originalPosition;
//         cameraTransform.rotation = originalRotation;
//     }
// }

using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform cameraTransform;
    public Transform playerTransform; // 添加玩家引用
    public float zoomSpeed;
    public float targetDistance;
    public float targetHeight;
    public float maxZoomTime;
    public Vector3 offsetFromPlayer; // 相对玩家的偏移量

    private Transform targetNPC;
    private Vector3 currentOffset; // 当前相对玩家的偏移量
    private bool isZoomedIn = false;
    private float zoomTimer = 0f;
    private bool isZoomFinished = false;
    // private Vector3 originalPosition;
    // private Quaternion originalRotation;

    //[Header("UI强制激活")]
    //public GameObject gameplayPanel; // 拖拽你的 Gameplay Panel 进来


    private void Start()
    {
        // originalPosition = cameraTransform.position;
        // originalRotation = cameraTransform.rotation;
        currentOffset = offsetFromPlayer;
        UpdateCameraPosition(); // 初始化相机位置
        // 强制激活Gameplay Panel
        // if (gameplayPanel != null)
        //     gameplayPanel.SetActive(true);
    }

    private void LateUpdate() // 改用LateUpdate确保在玩家移动后更新相机
    {
        if (isZoomedIn)
        {
            HandleZoomMode();
        }
        else
        {
            // 正常跟随模式
            UpdateCameraPosition();
        }

        // 按下C恢复
        if (isZoomedIn && Input.GetKeyDown(KeyCode.C))
        {
            ResetCamera();
        }
    }

    private void HandleZoomMode()
    {
        if (!isZoomFinished && targetNPC != null)
        {
            zoomTimer += Time.deltaTime * zoomSpeed;
            float easeProgress = Mathf.Clamp01(zoomTimer / maxZoomTime);

            // 计算目标位置和旋转
            Vector3 focusPosition = targetNPC.position + new Vector3(0, targetHeight, 0);
            Vector3 targetPosition = focusPosition - cameraTransform.forward * targetDistance;

            // 从当前跟随位置插值到目标位置
            Vector3 currentFollowPosition = playerTransform.position + currentOffset;
            cameraTransform.position = Vector3.Lerp(currentFollowPosition, targetPosition, easeProgress);

            // 注视目标
            Quaternion targetRotation = Quaternion.LookRotation(focusPosition - targetPosition);
            cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation, targetRotation, easeProgress);

            if (easeProgress >= 1f)
            {
                isZoomFinished = true;
            }
        }
    }

    private void UpdateCameraPosition()
    {
        if (playerTransform != null)
        {
            // 更新相机位置，保持相对玩家的固定偏移
            cameraTransform.position = playerTransform.position + currentOffset;
            //cameraTransform.rotation = Quaternion.Euler(30, 0, 0); // 或其他你想要的固定角度
        }
    }

    public void FocusOnNPC(Transform npcTransform)
    {
        targetNPC = npcTransform;
        isZoomedIn = true;
        isZoomFinished = false;
        zoomTimer = 0f;
    }

    public void ResetCamera()
    {
        targetNPC = null;
        isZoomedIn = false;
        isZoomFinished = false;
        zoomTimer = 0f;
        currentOffset = offsetFromPlayer;
        // 让相机平滑返回到正常跟随位置，而不是直接跳回原始位置
        UpdateCameraPosition();

        // // 每次还原时都强制激活Gameplay Panel
        // if (gameplayPanel != null)
        //     gameplayPanel.SetActive(true);
    }
}