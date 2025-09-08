using UnityEngine;

public class CurrencyWallet : MonoBehaviour
{
    [Header("把金币定义拖进来（AttributeDefinition，id 建议 gold）")]
    public AttributeDefinition goldDef;
    public CharacterStats stats;

    public GameEventFloatChanged onGoldChanged;

    public float Gold => stats != null && goldDef != null ? stats.GetFinal(goldDef) : 0f;

    public void AddGold(float amount)
    {
        if (stats == null || goldDef == null) return;
        var before = Gold;
        stats.Add(goldDef, amount, invokeEvent: false);
        var after = Gold;
        onGoldChanged?.Raise(goldDef.id, before, after);
    }

    public bool SpendGold(float amount)
    {
        if (Gold < amount) return false;
        AddGold(-amount);
        return true;
    }
}
