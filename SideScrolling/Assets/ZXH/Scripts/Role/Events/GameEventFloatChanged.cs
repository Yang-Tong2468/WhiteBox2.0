using UnityEngine;

[CreateAssetMenu(menuName = "RPG/Events/FloatChanged", fileName = "Evt_FloatChanged")]
public class GameEventFloatChanged : BaseGameEvent<FloatChangedPayload>
{
    public void Raise(string key, float before, float after)
        => Raise(new FloatChangedPayload { key = key, before = before, after = after });
}