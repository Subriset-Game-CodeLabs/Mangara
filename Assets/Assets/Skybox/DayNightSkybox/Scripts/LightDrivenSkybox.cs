using System.Collections.Generic;
using UnityEngine;

namespace MGeLabs.DayNightSystem
{
    /// <summary>
    /// Skrip kustom untuk memperbarui Skybox, Ambient, dan Fog 
    /// secara murni berdasarkan rotasi manual/otomatis dari Directional Light.
    /// </summary>
    public class LightDrivenSkybox : MonoBehaviour
    {
        [Header("Setup Utama")]
        [Tooltip("Masukkan Directional Light yang dirotasi oleh skrip teman Anda.")]
        [SerializeField] private Light directionalLight;
        [Tooltip("Masukkan file DayNightVisualConfig (ScriptableObject) Anda.")]
        [SerializeField] private DayNightVisualConfig visualConfig;

        [Header("Modul Tambahan (Opsional)")]
        [Tooltip("Masukkan modul tambahan seperti Particle System jika ada.")]
        [SerializeField] private List<DayNightVisualModule> visualModules;

        private Material skyMaterialInstance;

        private void Start()
        {
            if (visualConfig == null || visualConfig.skyMaterial == null)
            {
                Debug.LogError("Visual Config atau Sky Material belum dimasukkan di Inspector!");
                return;
            }

            // Membuat instance material baru di runtime agar tidak merusak file asli di Project
            skyMaterialInstance = new Material(visualConfig.skyMaterial);
            RenderSettings.skybox = skyMaterialInstance;
        }

        private void Update()
        {
            if (directionalLight == null || visualConfig == null || skyMaterialInstance == null) return;

            // 1. HITUNG PERSENTASE WAKTU (0 - 1) BERDASARKAN ROTASI X LAMPU
            float xRotation = directionalLight.transform.localEulerAngles.x;

            // Normalisasi sudut: Kita buat sudut 270 derajat (lampu lurus ke atas / Tengah Malam) sebagai titik 0
            float normalizedAngle = (xRotation + 90f) % 360f;
            float timePercentage = normalizedAngle / 360f;

            // 2. UPDATE TRANSISI TEXTURE SKYBOX
            if (visualConfig.skyMaterialTransitionValues != null && visualConfig.skyMaterialTransitionValues.Count > 0)
            {
                float transitionVal = LerpList(visualConfig.skyMaterialTransitionValues, timePercentage);
                skyMaterialInstance.SetFloat("_Transition", transitionVal);
            }

            // 3. UPDATE WARNA AMBIENT & DIRECTIONAL LIGHT
            if (visualConfig.enableColors)
            {
                RenderSettings.ambientLight = visualConfig.ambientLightColor.Evaluate(timePercentage);
                directionalLight.color = visualConfig.directionalLightColor.Evaluate(timePercentage);
            }

            // 4. UPDATE UNITY FOG (KABUT GAME)
            if (visualConfig.enableFog)
            {
                RenderSettings.fog = true;
                RenderSettings.fogColor = visualConfig.fogColor.Evaluate(timePercentage);

                if (RenderSettings.fogMode == FogMode.Linear && visualConfig.fogDistances != null && visualConfig.fogDistances.Count > 0)
                {
                    RenderSettings.fogEndDistance = LerpList(visualConfig.fogDistances, timePercentage);
                }
            }

            // 5. UPDATE SKYBOX HORIZON FOG (KABUT SHADER)
            if (visualConfig.enableSkyFog)
            {
                skyMaterialInstance.SetColor("_FogColor", visualConfig.skyMaterialFogColor.Evaluate(timePercentage));
                if (visualConfig.skyFogDensities != null && visualConfig.skyFogDensities.Count > 0)
                {
                    skyMaterialInstance.SetFloat("_FogDensity", LerpList(visualConfig.skyFogDensities, timePercentage));
                }
            }

            // 6. UPDATE MODUL TAMBAHAN (KUNANG-KUNANG / PARTIKEL)
            if (visualModules != null)
            {
                foreach (var module in visualModules)
                {
                    if (module != null) module.UpdateVisual(timePercentage);
                }
            }
        }

        // Fungsi pembantu interpolasi list, menyalin logika asli dari DayNightVisualizer
        private float LerpList(List<float> list, float t)
        {
            if (list == null || list.Count == 0) return 0f;
            if (list.Count == 1) return list[0];

            int startIndex = (int)(t * (list.Count - 1));
            int endIndex = startIndex + 1;

            if (endIndex >= list.Count) return list[list.Count - 1];

            float startValue = list[startIndex];
            float endValue = list[endIndex];

            float fraction = (t - (float)startIndex / (list.Count - 1)) * (list.Count - 1);
            return Mathf.Lerp(startValue, endValue, fraction);
        }

        private void OnDestroy()
        {
            if (skyMaterialInstance != null)
            {
                Destroy(skyMaterialInstance);
            }
        }
    }
}