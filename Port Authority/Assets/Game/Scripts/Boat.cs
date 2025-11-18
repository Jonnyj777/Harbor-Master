using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Apple;
using static UnityEngine.GraphicsBuffer;

public class Boat : MonoBehaviour
{
    [Header("Cargo Prefabs")]
    public List<GameObject> cargoBoxes;
    public List<Cargo> cargo = new List<Cargo>();

    [SerializeField]
    private List<Cargo> unlockedCargo = new List<Cargo>();
    private Port port;

    [Header("Boat Collisions Settings")]
    public Color crashedColor = Color.cyan;  // Color to when boat vehicles crash

    private bool hasCrashed = false;
    private float sinkDelay = 2f;
    private float sinkLength = 10f;  // distance the boat sinks down
    private float sinkDuration = 2f;  // time it takes to sink down to the desired length
    //private float fadeDelay = 1f;  // time to wait before fading starts
    //private float fadeDuration = 5f;  // how long to fully fade out

    [Header("Boat Snapping")]
    public float dockingTime = 1.5f;
    public float rotationSmooth = 10f;
    public Renderer rend;
    private Transform dockEndPoint;
    private float t = 0f;
    private Vector3 p0, p1, p2;
    private bool isDelivering = false;
    private float boatLength;

    [Header("Instance Settings")]
    private LineFollow vehicle;
    private List<Renderer> vehiclePartRenderers = new List<Renderer>();
    private float minX, maxX, minZ, maxZ;   // World bounds

    [Header("Whirlpool Settings")]
    [SerializeField] private float whirlpoolSinkLength = 7f;

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
        /*
        GameObject terrain = GameObject.Find("TerrainGenerator");
        MeshFilter terrainMeshFilter = terrain.GetComponent<MeshFilter>();
        Bounds terrainMeshBounds = terrainMeshFilter.mesh.bounds;
        Vector3 terrainUnscaledSize = terrainMeshBounds.size;
        Vector3 terrainScaledSize = Vector3.Scale(terrainUnscaledSize, terrain.transform.localScale);
        */

        minX = 0;
        maxX = 1000;
        minZ = 0;
        maxZ = 1000;

        // Get boat size
        boatLength = rend.bounds.size.z;
    }

    private void Update()
    {
        CheckBounds();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Boat vehicle crash state:
        // Disappear off map after a few seconds (do NOT act as additional obstacles)
        if (other.CompareTag("Boat"))
        {
            bool multipleCollisions = true;
            if (GetInstanceID() < other.GetInstanceID())
            {
                multipleCollisions = false;
            }
            EnterCrashState(multipleCollisions);
        }
        if (other.CompareTag("Terrain"))
        {
            bool multipleCollisions = false;
            EnterCrashState(multipleCollisions);
        }
        if (other.CompareTag("Port") && !isDelivering) 
        {
            isDelivering = true;
            vehicle.SetAtPort(true);
            vehicle.DeleteLine();
            port = other.GetComponent<Port>();
            StartCoroutine(ParkBoatAndDeliver());
            //DeliverCargo();
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
            StartCoroutine(DeliverCargoRoutine());
        }
    }

    private IEnumerator DeliverCargoRoutine()
    {
        vehicle.SetIsMovingCargo(true);
        for (int i = 0; i < cargo.Count; i++)
        {
            yield return new WaitForSeconds(vehicle.boatLoadingDelay);
            
            port.ReceiveCargoBox(cargo[i]);
            cargoBoxes[i].SetActive(false);
        }

        cargo.Clear();
        vehicle.SetIsMovingCargo(false);
        AudioManager.Instance.PlayBoatDelivery();
        isDelivering = false;
    }

    private IEnumerator ParkBoatAndDeliver()
    {
        yield return ParkBoat();
        DeliverCargo();
        
    }

    private IEnumerator ParkBoat()
    {
        t = 0f;
        dockEndPoint = port.endPoint;

        Vector3 p0 = transform.position;
        Vector3 p2 = dockEndPoint.position - dockEndPoint.forward * (boatLength / 2f);
        p2.y = p0.y;

        while (true)
        {
            if (hasCrashed)
                yield break;

            t += Time.deltaTime / dockingTime;
            float easedT = Mathf.SmoothStep(0, 1, t);

            transform.position = Vector3.Lerp(p0, p2, easedT);

            Quaternion targetRot = Quaternion.LookRotation(dockEndPoint.forward);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                rotationSmooth * Time.deltaTime
            );

            if (t >= 1f)
            {
                transform.position = p2;
                transform.rotation = dockEndPoint.rotation;
                break;
            }

            yield return null;
        }

        transform.Rotate(0f, 180f, 0f);
    }

    public void EnterCrashState(bool multipleCollisions, bool skipFadeOut = false)
    {
        // Prevent multiple triggers
        if (hasCrashed)
        {
            return;
        }
        hasCrashed = true;

        if (!multipleCollisions)
        {
            // Only trigger the collision sound once
            AudioManager.Instance.PlayBoatCollision();
        }
        
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

        if (!skipFadeOut)
        {
            StartCoroutine(SinkFadeOut());
        }
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

    public void EnterWhirlpool(Transform whirlpoolCenter, float duration, System.Action<Boat> callback = null)
    {
        if (!hasCrashed)
        {
            EnterCrashState(multipleCollisions: false, skipFadeOut: true);

            StartCoroutine(SuckedInWhirlpool(whirlpoolCenter, duration, callback));
        }
    }

    private IEnumerator SuckedInWhirlpool(Transform center, float duration, System.Action<Boat> callback)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }

        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(center.position.x, center.position.y - whirlpoolSinkLength, center.position.z);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // move toward center while sinking
            transform.position = Vector3.Lerp(startPos, endPos, t);

            // rotate around center while moving inward
            transform.RotateAround(center.position, Vector3.up, 360f * Time.deltaTime);

            yield return null;
        }

        // once centered, trigger immediate crash for boat(s)
        Destroy(gameObject);

        // notify the whirlpool that this boat is done
        callback?.Invoke(this);
    }

    private void CheckBounds()
    {
        Vector3 pos = transform.position;

        // Small buffer to prevent boats from being deleted too early if their model origin isnt centered
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