using UnityEngine;
using MyGame.Time;

public class TimeBackgroundManager : MonoBehaviour
{
    public GameObject morningBg;
    public GameObject middayBg;
    public GameObject eveningBg;
    public GameObject nightBg;

    private APTimeManager apTimeManager;

    void Start()
    {
        apTimeManager = FindObjectOfType<APTimeManager>();
        if (apTimeManager != null)
        {
            APTimeManager.OnDateTimeChangedAP += OnTimeChanged;
            UpdateBackground(apTimeManager.currentPhase);
        }
    }

    void OnDestroy()
    {
        if (apTimeManager != null)
            APTimeManager.OnDateTimeChangedAP -= OnTimeChanged;
    }

    private void OnTimeChanged(Slax.Schedule.DateTime dt)
    {
        UpdateBackground(apTimeManager.currentPhase);
    }

    private void UpdateBackground(APTimeManager.Phase phase)
    {
        morningBg.SetActive(phase == APTimeManager.Phase.Morning);
        middayBg.SetActive(phase == APTimeManager.Phase.Midday);
        eveningBg.SetActive(phase == APTimeManager.Phase.Evening);
        nightBg.SetActive(phase == APTimeManager.Phase.Night);
    }
}