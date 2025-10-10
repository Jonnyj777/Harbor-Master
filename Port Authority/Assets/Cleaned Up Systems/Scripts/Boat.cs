using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boat : MonoBehaviour
{
    public List<Cargo> cargo = new List<Cargo>();
    private Port port;
    public List<GameObject> cargoBoxes;

    // settings for boat collisions
    private float sinkLength = 0.5f;  // distance the boat sinks down
    private float sinkDuration = 2f;  // time it takes to sink down to the desired length
    private float fadeDelay = 1f;  // time to wait before fading starts
    private float fadeDuration = 2f;  // how long to fully fade out
    LineFollow vehicle;
    public Color crashedColor = Color.cyan;  // Color to when boat vehicles crash
    private Renderer vehicleRenderer;

    private void Start()
    {
        AssignCargo();
        vehicle = GetComponent<LineFollow>();
        vehicleRenderer = GetComponent<Renderer>();
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
        vehicle.SetIsCrashed(true);
        if (vehicleRenderer != null)
        {
            vehicleRenderer.material.color = crashedColor;
        }
        StartCoroutine(SinkFadeOut());
    }

    // function to make boats sink, fade, then destroyed after crashing into another boat vehicle
    private IEnumerator SinkFadeOut()
    {
        // sinking
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

        // wait before fade
        yield return new WaitForSeconds(fadeDelay);

        // fade out
        if (vehicleRenderer != null)
        {
            Material mat = vehicleRenderer.material;
            Color fadeColor = mat.color;
            float fadeElapsed = 0f;

            // ensure material supports transparency
            mat.SetFloat("_Mode", 2);
            mat.color = fadeColor;

            while (fadeElapsed < fadeDuration)
            {
                float alpha = Mathf.Lerp(1f, 0f, fadeElapsed / fadeDuration);
                mat.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
                fadeElapsed += Time.deltaTime;
                yield return null;
            }
        }
        // destroy game object
        Destroy(gameObject);
    }
}
