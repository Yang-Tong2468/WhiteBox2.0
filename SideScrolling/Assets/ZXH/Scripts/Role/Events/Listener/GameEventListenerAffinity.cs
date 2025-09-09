using UnityEngine;
using UnityEngine.Events;

public class GameEventListenerAffinity : MonoBehaviour
{
    public GameEventAffinityChanged gameEvent;
    public UnityEvent<AffinityChangedPayload> response;

    void OnEnable() { gameEvent?.Register(OnRaised); }
    void OnDisable() { gameEvent?.Unregister(OnRaised); }
    void OnRaised(AffinityChangedPayload p) => response?.Invoke(p);
}