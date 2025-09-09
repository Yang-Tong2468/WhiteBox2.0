using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class NpcMovementController : MonoBehaviour
{
    [Tooltip("移动和渐变的总时长（秒）")]
    public float transitionDuration = 2f;

    private SpriteRenderer spriteRenderer;
    private Color startColor;
    private Vector3 startPos;
    private Vector3 targetPos;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            startColor = spriteRenderer.color;
        }
    }

    // 公共入口函数，执行完整的移动流程
    public void MoveTo(string sceneName, string markerName)
    {
        StartCoroutine(MoveRoutine(sceneName, markerName));
    }

    private IEnumerator MoveRoutine(string sceneName, string markerName)
    {
        // 1. 如果需要，先执行渐变消失动画
        yield return FadeOut();

        // 2. 判断是场景内移动还是跨场景移动
        if (string.IsNullOrEmpty(sceneName) || SceneManager.GetActiveScene().name == sceneName)
        {
            // 场景内移动
            GameObject targetMarker = GameObject.Find(markerName);
            if (targetMarker != null)
            {
                transform.position = targetMarker.transform.position;
                ResetState();
            }
            else
            {
                Debug.LogError($"在当前场景中找不到标记点 '{markerName}'！");
            }
        }
        else
        {
            // 跨场景移动
            yield return TeleportToSceneRoutine(sceneName, markerName);
        }
    }

    private IEnumerator FadeOut()
    {
        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / transitionDuration);
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.Lerp(startColor, Color.clear, t);
            }
            yield return null;
        }
        gameObject.SetActive(false);
    }

    private IEnumerator TeleportToSceneRoutine(string sceneName, string targetMarkerName)
    {
        DontDestroyOnLoad(gameObject);
        AsyncOperation sceneLoadOperation = SceneManager.LoadSceneAsync(sceneName);
        yield return sceneLoadOperation;

        GameObject targetMarker = GameObject.Find(targetMarkerName);
        if (targetMarker != null)
        {
            transform.position = targetMarker.transform.position;
        }
        else
        {
            Debug.LogError($"在场景 '{sceneName}' 中找不到标记点 '{targetMarkerName}'！");
            transform.position = Vector3.zero;
        }
        ResetState();
    }

    private void ResetState()
    {
        gameObject.SetActive(true);
        if (spriteRenderer != null)
        {
            spriteRenderer.color = startColor;
        }
    }
}