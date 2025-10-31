using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boat : MonoBehaviour
{
    [Header("Cargo Prefabs")]
    public List<GameObject> cargoBoxes;
    public List<Cargo> cargo = new List<Cargo>();

    private List<Cargo> unlockedCargo = new List<Cargo>();
    private Port port;

    [Header("Boat Collisions Settings")]
    public Color crashedColor = Color.cyan;  // Color to when boat vehicles crash

    private bool hasCrashed = false;
    private float sinkDelay = 1f;
    private float sinkLength = 5f;  // distance the boat sinks down
    private float sinkDuration = 2f;  // time it takes to sink down to the desired length
    private float fadeDelay = 1f;  // time to wait before fading starts
    private float fadeDuration = 5f;  // how long to fully fade out

    [Header("Instance Settings")]
    private LineFollow vehicle;
    private List<Renderer> vehiclePartRenderers = new List<Renderer>();
    private float minX, maxX, minZ, maxZ;   // World bounds


    private void Start()
    {
        AssignCargo();

        vehicle = GetComponent<LineFollow>();
        //vehicleRenderer = GetComponent<Renderer>();
        foreach (Renderer vehiclePartRenderer in GetComponentsInChildren<Renderer>())
        {
            if (vehiclePartRenderer.CompareTag("Boat"))
            {
                vehiclePartRenderers.Add(vehiclePartRenderer);
            }
        }

        // cargo renderers
        foreach (GameObject box in cargoBoxes)
        {
            if (box.activeSelf)
            {
                Renderer rend = box.GetComponent<Renderer>();
                if (rend != null)
                {
                    vehiclePartRenderers.Add(rend);
                }
            }
        }

        // Teach the boat the world bounds so it can destroy itself
        GameObject terrain = GameObject.Find("TerrainGenerator");
        MeshFilter terrainMeshFilter = terrain.GetComponent<MeshFilter>();
        Bounds terrainMeshBounds = terrainMeshFilter.mesh.bounds;
        Vector3 terrainUnscaledSize = terrainMeshBounds.size;
        Vector3 terrainScaledSize = Vector3.Scale(terrainUnscaledSize, terrain.transform.localScale);

        minX = terrain.transform.position.x;
        maxX = minX + terrainScaledSize.x;
        minZ = terrain.transform.position.z;
        maxZ = minZ + terrainScaledSize.z;
    }

    private void Update()
    {
        CheckBounds();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Boat vehicle crash state:
        // Disappear off map after a few seconds (do NOT act as additional obstacles)
        if (other.CompareTag("Terrain") || other.CompareTag("Boat")) 
        {
            EnterCrashState();
        }
        if (other.CompareTag("Port")) {
            vehicle.SetAtPort(true);
            vehicle.DeleteLine();
            port = other.GetComponent<Port>();
            DeliverCargo();
            transform.Rotate(0f, 180f, 0f);
        }
    }

    public void AssignCargo()
    {
        List<CargoType> unlockedCargoTypes = CargoManager.Instance.GetUnlockedCargo();
        if (unlockedCargoTypes.Count ==  0)
        {
            return;
        }

        int cargoAmount = Random.Range(1, cargoBoxes.Count + 1);

        for (int i = 0; i < cargoBoxes.Count; i++)
        {
            if (i < cargoAmount)
            {
                cargoBoxes[i].SetActive(true);

                // Pick a random unlocked cargo type
                CargoType selectedCargoType = unlockedCargoTypes[Random.Range(0, unlockedCargoTypes.Count)];

                // Apply selected cargo color to the prefab
                Renderer rend = cargoBoxes[i].GetComponent<Renderer>();
                rend.material.color = selectedCargoType.color;

                // Create a new Cargo instance
                Cargo c = new Cargo
                {
                    type = selectedCargoType.cargoName,
                    price = selectedCargoType.basePrice,
                    amount = 1,
                    color = selectedCargoType.color,
                    spawnTime = Time.time,
                };

                cargo.Add(c);
            }
            else
            {
                cargoBoxes[i].SetActive(false);
            }
        }
    }

    void DeliverCargo()
    {
        if (cargo.Count > 0)
        {
            port.ReceiveCargo(cargo);
            cargo.Clear();

            foreach (var box in cargoBoxes)
            {
                box.SetActive(false);
            }
        }
    }

    public void EnterCrashState()
    {
        // Prevent multiple triggers
        if (hasCrashed)
        {
            return;
        }
        hasCrashed = true;

        LivesManager.Instance.LoseLife();

        vehicle.SetIsCrashed(true);

        if (vehiclePartRenderers.Count != 0)
        {
            foreach (Renderer vehiclePartRenderer in vehiclePartRenderers)
            {
                foreach (Material mat in vehiclePartRenderer.materials)
                {
                    mat.color = crashedColor;
                }
            }
        }

        StartCoroutine(SinkFadeOut());
    }

    // function to make boats sink, fade, then destroyed after crashing into another boat vehicle
    private IEnumerator SinkFadeOut()
    {
        // Wait before sinking
        yield return new WaitForSeconds(sinkDelay);

        // Sinking logic
        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(startPos.x, startPos.y - sinkLength, startPos.z);
        float time = 0f;
        while (time < sinkDuration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, time / sinkDuration);
            time += Time.deltaTime;
            yield return null;
        }
        transform.position = endPos;

        //TODO: Review Fadeout logic.
        //// Wait before fade
        yield return new WaitForSeconds(fadeDelay);

        //// Fade out logic
        if (vehiclePartRenderers.Count == 0)
        {
            yield break;
        }

        //// Cache original material colors for all parts of a ship
        List<Color[]> originalMatColors = new List<Color[]>();
        foreach (Renderer vehiclePartRenderer in vehiclePartRenderers)
        {
            Color[] colors = new Color[vehiclePartRenderer.materials.Length];
            for (int i = 0; i < vehiclePartRenderer.materials.Length; i++)
            {
                colors[i] = vehiclePartRenderer.materials[i].color;
                MakeMaterialTransparent(vehiclePartRenderer.materials[i]);
            }
            originalMatColors.Add(colors);
        }

        float fadeElapsed = 0f;

        //// Fade all materials over time
        while (fadeElapsed < fadeDuration)
        {
            float ratioFaded = fadeElapsed / fadeDuration;

            for (int r = 0; r < vehiclePartRenderers.Count; r++)
            {
                Renderer vehiclePartRenderer = vehiclePartRenderers[r];
                for (int i = 0; i < vehiclePartRenderer.materials.Length; ++i)
                {
                    // Ensure material supports transparency
                    //MakeMaterialTransparent(vehiclePartRenderer.materials[i]);

                    Color startColor = originalMatColors[r][i];
                    Color targetColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
                    
                    // Lerp using _BaseColor for URP Lit Mats
                    if (vehiclePartRenderer.materials[i].shader.name.Contains("Universal Render Pipeline/Lit"))
                    {
                        vehiclePartRenderer.materials[i].SetColor("_BaseColor", Color.Lerp(startColor, targetColor, ratioFaded));
                    }
                    else
                    {
                        vehiclePartRenderer.materials[i].color = Color.Lerp(startColor, targetColor, ratioFaded);
                    }
                }
            }

            fadeElapsed += Time.deltaTime;
            yield return null;
        }

        //// Ensure that each material is fully transparent at the end
        foreach (Renderer vehiclePartRenderer in vehiclePartRenderers)
        {
            foreach (Material mat in vehiclePartRenderer.materials)
            {
                if (mat.shader.name.Contains("Universal Render Pipeline/Lit"))
                {
                    Color c = mat.GetColor("_BaseColor");
                    c.a = 0f;
                    mat.SetColor("_BaseColor", c);
                }
                else
                {
                    Color c = mat.color;
                    c.a = 0f;
                    mat.color = c;
                }
            }
        }

        Destroy(gameObject);
    }

    private void MakeMaterialTransparent(Material mat)
    {
        if (mat == null)
        {
            return;
        }

        // URP Lit Shader support
        if (mat.shader.name.Contains("Universal Render Pipeline/Lit"))
        {
            mat.SetFloat("_Surface", 1f);  // 1 = Transparent
            mat.SetFloat("_Blend", 0f);  // 0 = Alpha Blend
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.SetInt("SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }
        else  // fallback statement for non-URP materials
        {
            mat.SetFloat("_Mode", 3); // 3 = Transparent in Standard Shader
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
        }
    }

    private void CheckBounds()
    {
        Vector3 pos = transform.position;

        // Small buffer to prevent boats from being deleted too early if their model origin isn’t centered
        float buffer = 5f;

        if (pos.x < minX - buffer || pos.x > maxX + buffer ||
            pos.z < minZ - buffer || pos.z > maxZ + buffer)
        {
            DestroyBoatOutOfBounds();
        }
    }

    private void DestroyBoatOutOfBounds()
    {
        // Avoid duplicate calls if already crashing/sinking
        if (vehicle != null && hasCrashed)
        {
            return;
        }

        //Debug.Log($"Boat {name} went out of bounds and was destroyed.");
        Destroy(gameObject);
    }
}
