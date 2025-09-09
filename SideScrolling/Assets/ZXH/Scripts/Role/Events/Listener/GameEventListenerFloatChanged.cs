using UnityEngine;
using UnityEngine.Events;

public class GameEventListenerFloatChanged : MonoBehaviour
{
    public GameEventFloatChanged gameEvent;
    public UnityEvent<FloatChangedPayload> response;

    void OnEnable() { gameEvent?.Register(OnRaised); }
    void OnDisable() { gameEvent?.Unregister(OnRaised); }
    void OnRaised(FloatChangedPayload p) => response?.Invoke(p);
}