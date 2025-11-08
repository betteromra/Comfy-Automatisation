using UnityEngine;
using System;

public class DayNightCycleManager : MonoBehaviour
{
    [Header("Cycle Timing (in seconds)")]
    [SerializeField] private float dayDuration = 20f; // DEBUG: 20 seconds
    [SerializeField] private float sunsetDuration = 5f; // DEBUG: 5 seconds
    [SerializeField] private float nightDuration = 20f; // DEBUG: 20 seconds
    [SerializeField] private float sunriseDuration = 5f; // DEBUG: 5 seconds

    [ContextMenu("Set Debug Speed (Fast)")]
    private void SetDebugSpeed()
    {
        dayDuration = 20f;
        sunsetDuration = 5f;
        nightDuration = 20f;
        sunriseDuration = 5f;
        totalCycleDuration = dayDuration + sunsetDuration + nightDuration + sunriseDuration;
        //Debug.Log("Day/Night cycle set to debug speed (50 seconds total) with corrected angles");
    }

    [ContextMenu("Set Normal Speed")]
    private void SetNormalSpeed()
    {
        dayDuration = 360f;
        sunsetDuration = 120f;
        nightDuration = 360f;
        sunriseDuration = 120f;
        totalCycleDuration = dayDuration + sunsetDuration + nightDuration + sunriseDuration;
        //Debug.Log("Day/Night cycle set to normal speed (16 minutes total)");
    }

    [Header("Lighting")]
    [SerializeField] private Light directionalLight;
    [SerializeField] private Color dayLightColor = new Color(1f, 0.96f, 0.84f);
    [SerializeField] private Color sunsetLightColor = new Color(1f, 0.6f, 0.4f);
    [SerializeField] private Color nightLightColor = new Color(0.5f, 0.6f, 0.8f);
    [SerializeField] private Color sunriseLightColor = new Color(1f, 0.7f, 0.5f);

    [Header("Light Intensity")]
    [SerializeField] private float dayLightIntensity = 1.2f;
    [SerializeField] private float sunsetLightIntensity = 0.8f;
    [SerializeField] private float nightLightIntensity = 0.4f;
    [SerializeField] private float sunriseLightIntensity = 0.7f;

    [Header("Ambient Lighting")]
    [SerializeField] private Color dayAmbientColor = new Color(0.5f, 0.5f, 0.5f);
    [SerializeField] private Color nightAmbientColor = new Color(0.2f, 0.2f, 0.3f);

    // Current cycle state
    private float currentTime = 0f;
    private float totalCycleDuration;
    private TimeOfDay currentTimeOfDay = TimeOfDay.Day;
    public TimeOfDay CurrentTimeOfDay { get => currentTimeOfDay; }
    Timer updateLightTimer;
    Timer dayTimer;
    Timer sunsetTimer;
    Timer nightTimer;
    Timer sunriseTimer;

    // Events
    public event Action OnDayStart;
    public event Action OnSunsetStart;
    public event Action OnNightStart;
    public event Action OnSunriseStart;

    public enum TimeOfDay
    {
        Day,
        Sunset,
        Night,
        Sunrise
    }

    private void Start()
    {
        totalCycleDuration = dayDuration + sunsetDuration + nightDuration + sunriseDuration;

        updateLightTimer = new Timer(totalCycleDuration * 0.0075f);
        updateLightTimer.Restart();

        dayTimer = new(dayDuration);
        sunsetTimer = new(sunsetDuration);
        nightTimer = new(nightDuration);
        sunriseTimer = new(sunriseDuration);

        // Start at day
        currentTime = -.001f;
        UpdateCycle();
    }

    private void Update()
    {
        currentTime += Time.deltaTime;

        // Loop the cycle
        if (currentTime >= totalCycleDuration)
        {
            currentTime -= totalCycleDuration;
        }

        UpdateCycle();
    }

    private void UpdateCycle()
    {
        float dayEnd = dayDuration;
        float sunsetEnd = dayEnd + sunsetDuration;
        float nightEnd = sunsetEnd + nightDuration;
        float sunriseEnd = nightEnd + sunriseDuration;

        TimeOfDay previousTimeOfDay = currentTimeOfDay;

        // Determine current time of day
        if (currentTime < dayEnd)
        {
            currentTimeOfDay = TimeOfDay.Day;
            if (previousTimeOfDay != currentTimeOfDay)
            {
                OnDayStart?.Invoke();
                dayTimer.Restart();
                directionalLight.color = dayLightColor;
                directionalLight.intensity = dayLightIntensity;
                RenderSettings.ambientLight = dayAmbientColor;
            }
            UpdateDay();
        }
        else if (currentTime < sunsetEnd)
        {
            currentTimeOfDay = TimeOfDay.Sunset;
            if (previousTimeOfDay != currentTimeOfDay)
            {
                OnSunsetStart?.Invoke();
                sunsetTimer.Restart();
            }
            UpdateSunset();
        }
        else if (currentTime < nightEnd)
        {
            currentTimeOfDay = TimeOfDay.Night;
            if (previousTimeOfDay != currentTimeOfDay)
            {
                OnNightStart?.Invoke();
                nightTimer.Restart();
            }
            UpdateNight();
        }
        else
        {
            currentTimeOfDay = TimeOfDay.Sunrise;
            if (previousTimeOfDay != currentTimeOfDay)
            {
                OnSunriseStart?.Invoke();
                sunriseTimer.Restart();
            }
            UpdateSunrise();
        }
    }

    private void UpdateDay()
    {
        if (directionalLight == null) return;
        if (!updateLightTimer.IsOverLoop()) return;

        float progress = dayTimer.PercentTime();

        // Sun moves from low angle (sunrise) through high angle (noon) back to low (sunset)
        float rotationAngle = Mathf.Lerp(150, 30, progress);

        directionalLight.transform.rotation = Quaternion.Euler(rotationAngle, -20, 0f);
    }

    private void UpdateSunset()
    {
        if (directionalLight == null) return;
        if (!updateLightTimer.IsOverLoop()) return;
        float progress = sunsetTimer.PercentTime();

        // Sun goes from sunset angle down below horizon (negative angles)
        float rotationAngle = Mathf.Lerp(30, 10, progress);
        directionalLight.transform.rotation = Quaternion.Euler(rotationAngle, -20, 0f);

        // Interpolate colors
        directionalLight.color = Color.Lerp(dayLightColor, sunsetLightColor, progress);
        directionalLight.intensity = Mathf.Lerp(dayLightIntensity, sunsetLightIntensity, progress);

        RenderSettings.ambientLight = Color.Lerp(dayAmbientColor, nightAmbientColor, progress);
    }

    private void UpdateNight()
    {
        if (directionalLight == null) return;
        if (!updateLightTimer.IsOverLoop()) return;
        float progress = nightTimer.PercentTime();

        // Moon light - keep at moderate angle from above
        float rotationAngle;

        if (progress <= .2f)
        {
            directionalLight.color = Color.Lerp(sunsetLightColor, nightLightColor, progress * 5);
            directionalLight.intensity = Mathf.Lerp(sunsetLightIntensity, nightLightIntensity, progress * 5);
            rotationAngle = Mathf.Lerp(10, -20, progress * 5);
        }
        else if (progress >= .8f)
        {
            directionalLight.color = Color.Lerp(nightLightColor, sunriseLightColor, progress * 5 - 4);
            directionalLight.intensity = Mathf.Lerp(nightLightIntensity, sunriseLightIntensity, progress * 5 - 4);
            rotationAngle = Mathf.Lerp(-160, -190, progress * 5 - 4);
        }
        else
        {
            rotationAngle = Mathf.Lerp(190, -10, progress * 1.6667f - .3334f);
        }

        directionalLight.transform.rotation = Quaternion.Euler(rotationAngle, -20, 0f);
    }

    private void UpdateSunrise()
    {
        if (directionalLight == null) return;
        if (!updateLightTimer.IsOverLoop()) return;
        float progress = sunriseTimer.PercentTime();

        // Sun rises from below horizon to sunrise angle
        float rotationAngle = Mathf.Lerp(-190, -210, progress);
        directionalLight.transform.rotation = Quaternion.Euler(rotationAngle, -20, 0f);

        // Interpolate colors
        directionalLight.color = Color.Lerp(sunriseLightColor, dayLightColor, progress);
        directionalLight.intensity = Mathf.Lerp(sunriseLightIntensity, dayLightIntensity, progress);

        RenderSettings.ambientLight = Color.Lerp(nightAmbientColor, dayAmbientColor, progress);
    }

    // Public getters
    public TimeOfDay GetCurrentTimeOfDay() => currentTimeOfDay;
    public float GetCurrentTime() => currentTime;
    public float GetTotalCycleDuration() => totalCycleDuration;
    public float GetCycleProgress() => currentTime / totalCycleDuration;

    // Optional: Set time manually (for debugging or game features)
    public void SetTime(float time)
    {
        currentTime = Mathf.Clamp(time, 0f, totalCycleDuration);
        UpdateCycle();
    }

    public void SetTimeOfDay(TimeOfDay timeOfDay)
    {
        switch (timeOfDay)
        {
            case TimeOfDay.Day:
                currentTime = 0f;
                break;
            case TimeOfDay.Sunset:
                currentTime = dayDuration;
                break;
            case TimeOfDay.Night:
                currentTime = dayDuration + sunsetDuration;
                break;
            case TimeOfDay.Sunrise:
                currentTime = dayDuration + sunsetDuration + nightDuration;
                break;
        }
        UpdateCycle();
    }
}
