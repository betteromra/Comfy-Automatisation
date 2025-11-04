using UnityEngine;
using System;

public class DayNightCycleManager : MonoBehaviour
{
    [Header("Cycle Timing (in seconds)")]
    [SerializeField] private float dayDuration = 360f; // 6 minutes
    [SerializeField] private float sunsetDuration = 120f; // 2 minutes
    [SerializeField] private float nightDuration = 360f; // 6 minutes
    [SerializeField] private float sunriseDuration = 120f; // 2 minutes

    [Header("Lighting")]
    [SerializeField] private Light directionalLight;
    [SerializeField] private Color dayLightColor = new Color(1f, 0.96f, 0.84f);
    [SerializeField] private Color sunsetLightColor = new Color(1f, 0.6f, 0.4f);
    [SerializeField] private Color nightLightColor = new Color(0.4f, 0.5f, 0.7f);
    [SerializeField] private Color sunriseLightColor = new Color(1f, 0.7f, 0.5f);
    
    [Header("Light Intensity")]
    [SerializeField] private float dayLightIntensity = 1.2f;
    [SerializeField] private float sunsetLightIntensity = 0.8f;
    [SerializeField] private float nightLightIntensity = 0.3f;
    [SerializeField] private float sunriseLightIntensity = 0.7f;

    [Header("Sun Rotation")]
    [SerializeField] private float dayRotationStart = -30f; // Sunrise angle
    [SerializeField] private float dayRotationEnd = 210f; // Sunset angle
    
    [Header("Ambient Lighting")]
    [SerializeField] private Color dayAmbientColor = new Color(0.5f, 0.5f, 0.5f);
    [SerializeField] private Color nightAmbientColor = new Color(0.2f, 0.2f, 0.3f);

    // Current cycle state
    private float currentTime = 0f;
    private float totalCycleDuration;
    private TimeOfDay currentTimeOfDay = TimeOfDay.Day;

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
        
        if (directionalLight == null)
        {
            directionalLight = FindFirstObjectByType<Light>();
            if (directionalLight == null)
            {
                Debug.LogWarning("No directional light found for day/night cycle!");
            }
        }

        // Start at day
        currentTime = 0f;
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
            UpdateDay(currentTime / dayDuration);
        }
        else if (currentTime < sunsetEnd)
        {
            currentTimeOfDay = TimeOfDay.Sunset;
            float sunsetProgress = (currentTime - dayEnd) / sunsetDuration;
            UpdateSunset(sunsetProgress);
        }
        else if (currentTime < nightEnd)
        {
            currentTimeOfDay = TimeOfDay.Night;
            float nightProgress = (currentTime - sunsetEnd) / nightDuration;
            UpdateNight(nightProgress);
        }
        else
        {
            currentTimeOfDay = TimeOfDay.Sunrise;
            float sunriseProgress = (currentTime - nightEnd) / sunriseDuration;
            UpdateSunrise(sunriseProgress);
        }

        // Trigger events when time of day changes
        if (previousTimeOfDay != currentTimeOfDay)
        {
            switch (currentTimeOfDay)
            {
                case TimeOfDay.Day:
                    OnDayStart?.Invoke();
                    break;
                case TimeOfDay.Sunset:
                    OnSunsetStart?.Invoke();
                    break;
                case TimeOfDay.Night:
                    OnNightStart?.Invoke();
                    break;
                case TimeOfDay.Sunrise:
                    OnSunriseStart?.Invoke();
                    break;
            }
        }
    }

    private void UpdateDay(float progress)
    {
        if (directionalLight == null) return;

        // During day, sun moves from sunrise position to midday to sunset position
        float rotationAngle = Mathf.Lerp(dayRotationStart, dayRotationEnd * 0.5f, progress);
        directionalLight.transform.rotation = Quaternion.Euler(rotationAngle, 170f, 0f);

        directionalLight.color = dayLightColor;
        directionalLight.intensity = dayLightIntensity;
        RenderSettings.ambientLight = dayAmbientColor;
    }

    private void UpdateSunset(float progress)
    {
        if (directionalLight == null) return;

        // Sun continues rotating down during sunset
        float rotationAngle = Mathf.Lerp(dayRotationEnd * 0.5f, dayRotationEnd, progress);
        directionalLight.transform.rotation = Quaternion.Euler(rotationAngle, 170f, 0f);

        // Interpolate between day and sunset colors
        directionalLight.color = Color.Lerp(dayLightColor, sunsetLightColor, progress);
        directionalLight.intensity = Mathf.Lerp(dayLightIntensity, sunsetLightIntensity, progress);
        RenderSettings.ambientLight = Color.Lerp(dayAmbientColor, nightAmbientColor, progress);
    }

    private void UpdateNight(float progress)
    {
        if (directionalLight == null) return;

        // Keep sun below horizon during night
        float rotationAngle = Mathf.Lerp(dayRotationEnd, dayRotationEnd + 30f, progress);
        directionalLight.transform.rotation = Quaternion.Euler(rotationAngle, 170f, 0f);

        // Transition from sunset to full night color
        if (progress < 0.3f)
        {
            float transitionProgress = progress / 0.3f;
            directionalLight.color = Color.Lerp(sunsetLightColor, nightLightColor, transitionProgress);
            directionalLight.intensity = Mathf.Lerp(sunsetLightIntensity, nightLightIntensity, transitionProgress);
        }
        else
        {
            directionalLight.color = nightLightColor;
            directionalLight.intensity = nightLightIntensity;
        }
        
        RenderSettings.ambientLight = nightAmbientColor;
    }

    private void UpdateSunrise(float progress)
    {
        if (directionalLight == null) return;

        // Sun rises during sunrise
        float rotationAngle = Mathf.Lerp(dayRotationEnd + 30f, dayRotationStart, progress);
        directionalLight.transform.rotation = Quaternion.Euler(rotationAngle, 170f, 0f);

        // Interpolate between night and sunrise colors
        directionalLight.color = Color.Lerp(nightLightColor, sunriseLightColor, progress);
        directionalLight.intensity = Mathf.Lerp(nightLightIntensity, sunriseLightIntensity, progress);
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
