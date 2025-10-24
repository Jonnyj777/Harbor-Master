using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boat : MonoBehaviour
{
    public List<Cargo> cargo = new List<Cargo>();
    private Port port;
    public List<GameObject> cargoBoxes;

    // settings for boat collisions
    private bool hasCrashed = false;
    private float sinkDelay = 2f;
    private float sinkLength = 10f;  // distance the boat sinks down
    private float sinkDuration = 2f;  // time it takes to sink down to the desired length
    //private float fadeDelay = 1f;  // time to wait before fading starts
    //private float fadeDuration = 5f;  // how long to fully fade out
    LineFollow vehicle;
    public Color crashedColor = Color.cyan;  // Color to when boat vehicles crash
    //private Renderer vehicleRenderer;
    private List<Renderer> vehiclePartRenderers = new List<Renderer>();

    // World bounds
    float minX, maxX, minZ, maxZ;

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
        // boat vehicle crash state
        // if boat vehicle collides into another boat vehicle, both boat vehicles enter boat crash state
        // boat crash state = disappear off map after a few seconds (do NOT act as additional obstacles)
        if (other.CompareTag("Terrain")) 
        {
            EnterCrashState();
        }
        if (other.CompareTag("Boat"))
        {
            Boat otherBoat = other.GetComponent<Boat>();
            EnterCrashState();
            otherBoat.EnterCrashState();
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
        int cargoAmount = Random.Range(1, cargoBoxes.Count + 1);

        for (int i = 0; i < cargoBoxes.Count; i++)
        {
            if (i < cargoAmount)
            {
                cargoBoxes[i].SetActive(true);

                // set random color
                Color randomColor = new Color(Random.value, Random.value, Random.value);
                Renderer rend = cargoBoxes[i].GetComponent<Renderer>();
                rend.material.color = randomColor;

                // set type
                Cargo c = new Cargo
                {
                    type = "Coffee",
                    amount = 1,
                    color = randomColor,
                    spawnTime = Time.time,
                    price = 20
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
        //yield return new WaitForSeconds(fadeDelay);

        //// Fade out logic
        //if (vehiclePartRenderers.Count == 0)
        //{
        //    yield break;
        //}

        //// Cache original material colors for all parts of a ship
        //List<Color[]> originalMatColors = new List<Color[]>();
        //foreach (Renderer vehiclePartRenderer in vehiclePartRenderers)
        //{
        //    Color[] colors = new Color[vehiclePartRenderer.materials.Length];
        //    for (int i = 0; i < vehiclePartRenderer.materials.Length; i++)
        //    {
        //        colors[i] = vehiclePartRenderer.materials[i].color;
        //    }
        //    originalMatColors.Add(colors);
        //}

        //float fadeElapsed = 0f;

        //// Fade all materials over time
        //while (fadeElapsed < fadeDuration)
        //{
        //    float ratioFaded = fadeElapsed / fadeDuration;

        //    for (int r = 0; r < vehiclePartRenderers.Count; r++)
        //    {
        //        Renderer vehiclePartRenderer = vehiclePartRenderers[r];
        //        for (int i = 0; i < vehiclePartRenderer.materials.Length; ++i)
        //        {
        //            // Ensure material supports transparency
        //            MakeMaterialTransparent(vehiclePartRenderer.materials[i]);

        //            Color startColor = originalMatColors[r][i];
        //            Color targetColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
        //            vehiclePartRenderer.materials[i].color = Color.Lerp(startColor, targetColor, ratioFaded);
        //        }
        //    }

        //    fadeElapsed += Time.deltaTime;
        //    yield return null;
        //}

        //// Ensure that each material is fully transparent at the end
        //foreach (Renderer vehiclePartRenderer in vehiclePartRenderers)
        //{
        //    foreach (Material mat in vehiclePartRenderer.materials)
        //    {
        //        Color c = mat.color;
        //        c.a = 0f;
        //        mat.color = c;
        //    }
        //}

        Destroy(gameObject);
    }

    //private void MakeMaterialTransparent(Material mat)
    //{
    //    mat.SetFloat("_Mode", 3); // 3 = Transparent in Standard Shader
    //    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
    //    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
    //    mat.SetInt("_ZWrite", 0);
    //    mat.DisableKeyword("_ALPHATEST_ON");
    //    mat.EnableKeyword("_ALPHABLEND_ON");
    //    mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
    //    mat.renderQueue = 3000;
    //}

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
