using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
    public NpcDefinition npcDefinition; // 与场景中 NPC 关联的 SO 数据
    public AffinityRule[] customAffinityRules; // NPC 特有的好感规则


    public float moveDistance = 5f;    // 向右移动的距离（单位：世界单位）
    public float duration = 2f;        // 总时长（秒）

    public CharacterStats stats;       // 角色属性（用于好感度计算）
    private SpriteRenderer spriteRenderer;
    private Vector3 startPos;
    private Vector3 targetPos;
    private Color startColor;


    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        startPos = transform.position;
        targetPos = startPos + Vector3.right * moveDistance;
        startColor = spriteRenderer.color;
        stats = FindAnyObjectByType<CharacterStats>();
    }

    /// <summary>
    /// 更新好感度
    /// </summary>
    public void UpdateAffinity()
    {
        foreach (var pair in customAffinityRules)
        {
            AffinitySystem.Instance.ApplyRule(stats, npcDefinition, pair);
        }
        Debug.Log($"NPC {npcDefinition.npcId} 好感度:{AffinitySystem.Instance.GetAffinity(npcDefinition)}");
    }

    public void BeginMoveAndFade()
    {
        StartCoroutine(MoveRightAndFade());
    }

    /// <summary>
    /// 移动加渐变协程
    /// </summary>
    /// <returns></returns>
    private IEnumerator MoveRightAndFade()
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // 同步移动向右
            transform.position = Vector3.Lerp(startPos, targetPos, t);

            // 同步渐变透明
            float newAlpha = Mathf.Lerp(startColor.a, 0f, t);
            spriteRenderer.color = new Color(
                startColor.r, startColor.g, startColor.b, newAlpha);

            yield return null;
        }

        // 确保完成最终状态
        transform.position = targetPos;
        spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, 0f);
    }
}
