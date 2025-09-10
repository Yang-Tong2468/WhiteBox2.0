// using UnityEngine;
// using PixelCrushers.DialogueSystem;

// public class DSUFieldReader : MonoBehaviour
// {
//     void Start()
//     {
//         // 访问整个数据库
//         DialogueDatabase db = DialogueManager.masterDatabase;

//         if (db == null)
//         {
//             Debug.LogWarning("未找到 Dialogue Database！");
//             return;
//         }

//         // 遍历所有 Actor
//         foreach (Actor actor in db.actors)
//         {
//             Debug.Log($"=== Actor: {actor.Name} ===");

//             // 遍历该 Actor 的所有字段
//             foreach (Field field in actor.fields)
//             {
//                 if (field != null)
//                 {
//                     Debug.Log($"字段名: {field.title}, 值: {field.value}, 类型: {field.type}");
//                 }
//             }
//         }
//     }
// }

// using UnityEngine;
// using PixelCrushers.DialogueSystem;
// using TMPro;

// public class DSUFieldReader : MonoBehaviour
// {
//     // 你需要的字段名列表
//     public string[] targetFieldNames;
//     public TextMeshProUGUI[] fieldTexts;

//     void Start()
//     {
//         DialogueDatabase db = DialogueManager.masterDatabase;

//         if (db == null)
//         {
//             Debug.LogWarning("未找到 Dialogue Database！");
//             return;
//         }

//         // 这里只取第一个Actor为例
//         Actor actor = db.actors.Count > 0 ? db.actors[0] : null;
//         if (actor == null)
//         {
//             Debug.LogWarning("未找到 Actor！");
//             return;
//         }

//         // foreach (Actor actor in db.actors)
//         // {
//         //     Debug.Log($"=== Actor: {actor.Name} ===");

//         //     foreach (string fieldName in targetFieldNames)
//         //     {
//         //         Field field = actor.fields.Find(f => f.title == fieldName);
//         //         if (field != null)
//         //         {
//         //             Debug.Log($"字段名: {field.title}, 值: {field.value}, 类型: {field.type}");
//         //         }
//         //         else
//         //         {
//         //             Debug.LogWarning($"未找到字段: {fieldName}");
//         //         }
//         //     }
//         // }

//         for (int i = 0; i < targetFieldNames.Length && i < fieldTexts.Length; i++)
//         {
//             Field field = actor.fields.Find(f => f.title == targetFieldNames[i]);
//             if (field != null)
//             {
//                 fieldTexts[i].text = field.value;
//             }
//             else
//             {
//                 fieldTexts[i].text = "未找到";
//             }
//         }
//     }
// } 

using UnityEngine;
using PixelCrushers.DialogueSystem;
using TMPro;

public class DSUFieldReader : MonoBehaviour
{
    public string[] targetFieldNames;
    public TextMeshProUGUI[] fieldTexts;

    // 段位对应可见条数
    private int GetVisibleCountByTier(string tier)
    {
        switch (tier)
        {
            case "陌生": return 1;
            case "冷漠": return 2;
            case "友好": return 3;
            case "亲密": return 5;
            case "挚友": return targetFieldNames.Length;
            default: return 1;
        }
    }

    // 添加一个方法来更新指定Actor的信息
    public void UpdateInfoPanel(string actorName)
    {
        DialogueDatabase db = DialogueManager.masterDatabase;
        if (db == null)
        {
            Debug.LogWarning("未找到 Dialogue Database！");
            return;
        }

        // 根据名字查找对应的Actor
        Actor actor = db.actors.Find(a => a.Name == actorName);
        if (actor == null)
        {
            Debug.LogWarning($"未找到Actor: {actorName}");
            return;
        }

        // // 更新面板信息
        // for (int i = 0; i < targetFieldNames.Length && i < fieldTexts.Length; i++)
        // {
        //     Field field = actor.fields.Find(f => f.title == targetFieldNames[i]);
        //     if (field != null)
        //     {
        //         fieldTexts[i].text = field.value;
        //     }
        //     else
        //     {
        //         fieldTexts[i].text = "未找到";
        //     }
        // }

        // 获取好感度段位
        string tier = "陌生";
        var affinitySystem = AffinitySystem.Instance;
        if (affinitySystem != null)
        {
            // 如果actorName和npcId不一致，这里需要转换
            string npcId = ConvertActorNameToNpcId(actorName); // 添加一个转换方法
            var npcDef = affinitySystem.allNPC.GetNpcById(npcId);
            if (npcDef != null)
            {
                tier = affinitySystem.GetTier(npcDef);
            }
        }

        int visibleCount = GetVisibleCountByTier(tier);

        // 按段位显示信息
        for (int i = 0; i < targetFieldNames.Length && i < fieldTexts.Length; i++)
        {
            if (i < visibleCount)
            {
                Field field = actor.fields.Find(f => f.title == targetFieldNames[i]);
                fieldTexts[i].text = field != null ? field.value : "not found";
            }
            else
            {
                fieldTexts[i].text = "secret";
            }
        }
    }

    // 添加转换方法（根据你的命名规则调整）
    private string ConvertActorNameToNpcId(string actorName)
    {
        // 如果actorName就是npcId，直接返回
        return actorName;

        // 或者根据你的规则转换，比如：
        // return "NPC_" + actorName;
    }
}