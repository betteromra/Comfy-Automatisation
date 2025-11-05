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
        sunriseAngle = 20f;
        noonAngle = 90f;
        sunsetAngle = 20f;
        totalCycleDuration = dayDuration + sunsetDuration + nightDuration + sunriseDuration;
        Debug.Log("Day/Night cycle set to debug speed (50 seconds total) with corrected angles");
    }

    [ContextMenu("Set Normal Speed")]
    private void SetNormalSpeed()
    {
        dayDuration = 360f;
        sunsetDuration = 120f;
        nightDuration = 360f;
        sunriseDuration = 120f;
        totalCycleDuration = dayDuration + sunsetDuration + nightDuration + sunriseDuration;
        Debug.Log("Day/Night cycle set to normal speed (16 minutes total)");
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

    [Header("Sun Rotation")]
    [SerializeField] private float sunriseAngle = 20f; // Low angle at sunrise (positive = from above)
    [SerializeField] private float noonAngle = 90f; // Directly overhead at noon
    [SerializeField] private float sunsetAngle = 20f; // Low angle at sunset (positive = from above)
    
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
        
        // Find directional light
        if (directionalLight == null)
        {
            Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            Debug.Log($"Found {lights.Length} lights in scene");
            
            foreach (Light light in lights)
            {
                Debug.Log($"Light: {light.gameObject.name}, Type: {light.type}, Enabled: {light.enabled}");
                if (light.type == LightType.Directional)
                {
                    directionalLight = light;
                    Debug.Log($"Assigned Directional Light: {light.gameObject.name}");
                    break;
                }
            }
        }
        
        if (directionalLight != null)
        {
            // Position light above the world center (-175, -20, -175)
            directionalLight.transform.position = new Vector3(-175f, 100f, -175f);
            Debug.Log($"Directional Light positioned at: {directionalLight.transform.position}");
        }
        else
        {
            Debug.LogWarning("No directional light found!");
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

        // Sun moves from low angle (sunrise) through high angle (noon) back to low (sunset)
        float rotationAngle = Mathf.Lerp(sunriseAngle, noonAngle, Mathf.Clamp01(progress * 2f));
        if (progress > 0.5f)
        {
            rotationAngle = Mathf.Lerp(noonAngle, sunsetAngle, (progress - 0.5f) * 2f);
        }
        
        directionalLight.transform.rotation = Quaternion.Euler(rotationAngle, 170f, 0f);
        directionalLight.color = dayLightColor;
        directionalLight.intensity = dayLightIntensity;
        
        if (Time.frameCount % 120 == 0) // Log every 2 seconds at 60fps
        {
            Debug.Log($"DAY - Progress: {progress:F2}, Angle: {rotationAngle:F1}°, Intensity: {directionalLight.intensity}, Rotation: {directionalLight.transform.rotation.eulerAngles}");
        }
        
        RenderSettings.ambientLight = dayAmbientColor;
    }

    private void UpdateSunset(float progress)
    {
        if (directionalLight == null) return;

        // Sun goes from sunset angle down below horizon (negative angles)
        float rotationAngle = Mathf.Lerp(sunsetAngle, -20f, progress);
        directionalLight.transform.rotation = Quaternion.Euler(rotationAngle, 170f, 0f);

        // Interpolate colors
        directionalLight.color = Color.Lerp(dayLightColor, sunsetLightColor, progress);
        directionalLight.intensity = Mathf.Lerp(dayLightIntensity, sunsetLightIntensity, progress);
        
        RenderSettings.ambientLight = Color.Lerp(dayAmbientColor, nightAmbientColor, progress);
    }

    private void UpdateNight(float progress)
    {
        if (directionalLight == null) return;

        // Moon light - keep at moderate angle from above
        float rotationAngle = Mathf.Lerp(30f, 50f, Mathf.Sin(progress * Mathf.PI));
        
        directionalLight.transform.rotation = Quaternion.Euler(rotationAngle, 170f, 0f);
        directionalLight.color = nightLightColor;
        directionalLight.intensity = nightLightIntensity;
        
        if (Time.frameCount % 120 == 0)
        {
            Debug.Log($"NIGHT - Progress: {progress:F2}, Angle: {rotationAngle:F1}°, Intensity: {directionalLight.intensity}");
        }
        
        RenderSettings.ambientLight = nightAmbientColor;
    }

    private void UpdateSunrise(float progress)
    {
        if (directionalLight == null) return;

        // Sun rises from below horizon to sunrise angle
        float rotationAngle = Mathf.Lerp(-20f, sunriseAngle, progress);
        directionalLight.transform.rotation = Quaternion.Euler(rotationAngle, 170f, 0f);

        // Interpolate colors
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
