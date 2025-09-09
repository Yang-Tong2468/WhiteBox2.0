using UnityEngine;

[CreateAssetMenu(menuName = "RPG/Events/AffinityChanged", fileName = "Evt_AffinityChanged")]
public class GameEventAffinityChanged : BaseGameEvent<AffinityChangedPayload>
{
    public void Raise(NpcDefinition npc, float before, float after)
        => Raise(new AffinityChangedPayload { npc = npc, before = before, after = after });
}