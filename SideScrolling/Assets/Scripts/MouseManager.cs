using UnityEngine;

public class MouseManager : MonoBehaviour
{
    private static MouseManager instance;
    private static bool forceCursorVisible = true;
    private static CursorLockMode targetLockMode = CursorLockMode.None;
    
    void Awake()
    {
        // 单例模式，确保只有一个MouseManager
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        // 立即设置鼠标状态
        ApplyCursorSettings();
    }
    
    void LateUpdate()
    {
        // 在LateUpdate中确保我们的设置最后生效
        ApplyCursorSettings();
    }
    
    private void ApplyCursorSettings()
    {
        if (Cursor.visible != forceCursorVisible)
        {
            Cursor.visible = forceCursorVisible;
        }
        
        if (Cursor.lockState != targetLockMode)
        {
            Cursor.lockState = targetLockMode;
        }
    }
    
    public static void SetCursorState(bool visible, CursorLockMode lockMode)
    {
        forceCursorVisible = visible;
        targetLockMode = lockMode;
        
        // 立即应用设置
        Cursor.visible = visible;
        Cursor.lockState = lockMode;
    }
    
    public static void ForceCursorVisible()
    {
        SetCursorState(true, CursorLockMode.None);
    }
}