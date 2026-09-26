using UnityEngine;

public class CeilingLampManager : MonoBehaviour
{
    public static CeilingLampManager instance;

    [Header("Ceiling Lamp Light Settings")]
    public Color lampColor = new Color(1.0f, 0.88f, 0.72f); // Warm incandescent tungsten
    public float lampIntensity = 5.0f;
    public float lampRange = 20f;
    public LightShadows shadowType = LightShadows.Soft;

    [Header("Ambient Mood Balance")]
    public bool balanceDirectionalLight = true;
    public float targetSunIntensity = 0.55f; // Softens harsh daylight but keeps room visible

    [Header("Atmosphere")]
    public bool enableSubtleHumFlicker = true;
    public float flickerMinMultiplier = 0.96f;
    public float flickerMaxMultiplier = 1.03f;

    private Light activeLampLight;
    private float baseIntensity;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        ScanAndSetupCeilingLamps();
        BalanceRoomLighting();
    }

    void Update()
    {
        // If no lamp light has been found yet, scan periodically
        if (activeLampLight == null && Time.frameCount % 60 == 0)
        {
            ScanAndSetupCeilingLamps();
        }

        // Very subtle realistic electrical micro-flicker for psychological horror ambiance
        if (enableSubtleHumFlicker && activeLampLight != null && activeLampLight.enabled)
        {
            float noise = Mathf.PerlinNoise(Time.time * 6f, 0f);
            activeLampLight.intensity = baseIntensity * Mathf.Lerp(flickerMinMultiplier, flickerMaxMultiplier, noise);
        }
    }

    public void BalanceRoomLighting()
    {
        if (!balanceDirectionalLight) return;

        // Find the harsh directional sunlight and soften it so the chandelier becomes the focal light source
        Light[] allLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (var l in allLights)
        {
            if (l.type == LightType.Directional)
            {
                l.intensity = targetSunIntensity;
                l.color = new Color(0.90f, 0.85f, 0.78f); // Warm dim ambient light
                Debug.Log($"CeilingLampManager: Balanced Directional Light to {targetSunIntensity} for atmospheric interior mood.");
            }
        }
    }

    public void ScanAndSetupCeilingLamps()
    {
        // Find the ROOT antique_light parent (not a child sub-mesh like antique_light.002)
        GameObject bestMatch = null;
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsInactive.Include);
        foreach (var go in allObjects)
        {
            string n = go.name.ToLower();
            if (n.Contains("antique_light") || n.Contains("chandelier") || n.Contains("ceiling_light") || 
                n.Contains("hanging_light") || n.Contains("light_fixture"))
            {
                // Prefer the parent (has children) over individual sub-meshes
                if (go.transform.childCount > 0)
                {
                    bestMatch = go;
                    break; // Root found, use it
                }
                else if (bestMatch == null)
                {
                    // Pick the highest parent that matches
                    Transform root = go.transform;
                    while (root.parent != null && root.parent.name.ToLower().Contains("antique_light"))
                    {
                        root = root.parent;
                    }
                    bestMatch = root.gameObject;
                }
            }
        }

        if (bestMatch != null)
        {
            SetupLamp(bestMatch);
            return;
        }

        // Fallback: attach light just below ceiling_big
        GameObject ceiling = GameObject.Find("ceiling_big");
        if (ceiling != null)
        {
            SetupLamp(ceiling);
        }
    }

    public void SetupLamp(GameObject lampObject)
    {
        if (lampObject == null) return;

        Light lampLight = lampObject.GetComponentInChildren<Light>();
        if (lampLight == null)
        {
            // Calculate the center of all child renderers for accurate light placement
            GameObject lightChild = new GameObject("CeilingLampLightSource");
            lightChild.transform.SetParent(lampObject.transform, false);
            
            Renderer[] childRends = lampObject.GetComponentsInChildren<Renderer>();
            if (childRends.Length > 0 && !lampObject.name.Contains("ceiling"))
            {
                // Average all child bounds to find true center of the chandelier
                Bounds combined = childRends[0].bounds;
                for (int i = 1; i < childRends.Length; i++)
                {
                    combined.Encapsulate(childRends[i].bounds);
                }
                // Place the light at the center of the combined bounds
                lightChild.transform.position = combined.center;
            }
            else if (lampObject.name.Contains("ceiling"))
            {
                lightChild.transform.localPosition = new Vector3(0f, -1.2f, 0f);
            }
            else
            {
                lightChild.transform.localPosition = new Vector3(0f, -0.25f, 0f);
            }

            lampLight = lightChild.AddComponent<Light>();
        }

        // Configure light properties for warm, rich interior lighting
        lampLight.type = LightType.Point;
        lampLight.color = lampColor;
        lampLight.intensity = lampIntensity;
        lampLight.range = lampRange;
        lampLight.shadows = shadowType;
        lampLight.shadowStrength = 0.85f;
        lampLight.enabled = true;

        activeLampLight = lampLight;
        baseIntensity = lampIntensity;

        // Apply emissive warm glow to ALL child renderers so every bulb/arm glows
        Renderer[] rends = lampObject.GetComponentsInChildren<Renderer>();
        foreach (var r in rends)
        {
            foreach (var mat in r.materials)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", lampColor * 2.0f);
            }
        }

        Debug.Log($"CeilingLampManager: Configured working light on '{lampObject.name}' with {rends.Length} glowing parts (Intensity: {lampIntensity}, Range: {lampRange})");
    }

    public void SetLightState(bool isOn)
    {
        if (activeLampLight != null)
        {
            activeLampLight.enabled = isOn;
        }
    }
}
