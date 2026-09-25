using Manager;
using UnityEngine;


public class LightingManager : MonoBehaviour
{
    // References
    [SerializeField] private Light directionalLight;
    [SerializeField] private LightingPreset currentPreset;
    // Variables
    // [SerializeField, Range(0, 24)] private float timeOfDay;

    private void Update()
    {
        if (currentPreset == null)
            return;

        UpdateLighting(GameManager.Instance.TimeOfDay / 24f);
        
    }

    private void UpdateLighting(float timePercent)
    {
        RenderSettings.ambientLight = currentPreset.ambientColor.Evaluate(timePercent);
        RenderSettings.fogColor = currentPreset.fogColor.Evaluate(timePercent);

        if (directionalLight != null)
        {
            directionalLight.color = currentPreset.directionalColor.Evaluate(timePercent);
            directionalLight.transform.localRotation = Quaternion.Euler(new Vector3((timePercent * 360f) - 90f, 170f, 0));
        }
    }
    
    
}
