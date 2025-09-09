using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "RPG/Events/TierChanged", fileName = "Evt_TierChanged")]
public class GameEventAffinityTierChanged : BaseGameEvent<TierChangedPayload>
{
    public void Raise(NpcDefinition npc, string fromTier, string toTier)
        => Raise(new TierChangedPayload { npc = npc, fromTier = fromTier, toTier = toTier });
}
