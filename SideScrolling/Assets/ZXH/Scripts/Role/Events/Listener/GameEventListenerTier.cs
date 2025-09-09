using UnityEngine;
using UnityEngine.Events;

public class GameEventListenerTier : MonoBehaviour
{
    public GameEventAffinityTierChanged gameEvent;
    public UnityEvent<TierChangedPayload> response;

    void OnEnable() { gameEvent?.Register(OnRaised); }
    void OnDisable() { gameEvent?.Unregister(OnRaised); }
    void OnRaised(TierChangedPayload p) => response?.Invoke(p);
}
