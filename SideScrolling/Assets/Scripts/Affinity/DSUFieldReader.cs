using UnityEngine;
using PixelCrushers.DialogueSystem;

public class DSUFieldReader : MonoBehaviour
{
    void Start()
    {
        // 访问整个数据库
        DialogueDatabase db = DialogueManager.masterDatabase;

        if (db == null)
        {
            Debug.LogWarning("未找到 Dialogue Database！");
            return;
        }

        // 遍历所有 Actor
        foreach (Actor actor in db.actors)
        {
            Debug.Log($"=== Actor: {actor.Name} ===");

            // 遍历该 Actor 的所有字段
            foreach (Field field in actor.fields)
            {
                if (field != null)
                {
                    Debug.Log($"字段名: {field.title}, 值: {field.value}, 类型: {field.type}");
                }
            }
        }
    }
}
