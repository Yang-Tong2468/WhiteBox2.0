using UnityEngine;
using UnityEngine.Events;

public abstract class BaseGameEvent<T> : ScriptableObject
{
    private event UnityAction<T> _listeners;

    public void Raise(T payload) => _listeners?.Invoke(payload);
    public void Register(UnityAction<T> l) => _listeners += l;
    public void Unregister(UnityAction<T> l) => _listeners -= l;
}