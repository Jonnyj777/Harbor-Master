using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class DrawLine : MonoBehaviour
{
    private List<Vector3> positions;

    private float timer;
    public float timerDelay = 0.01f; 

    public LineRenderer drawLine;

    [SerializeField]
    private LayerMask drawMask;

    public float lineWidth;

    public float heightOffset;

    private void Start()
    {
        positions = new List<Vector3>();
        timer = timerDelay;
        Renderer rend = GetComponent<Renderer>();
        heightOffset = rend.bounds.size.y * 0.5f;
    }

    public void StartLine(Vector3 position)
    {
        drawLine.positionCount = 1;
        drawLine.startWidth = drawLine.endWidth = lineWidth;
        drawLine.endWidth = lineWidth;
        drawLine.material = new Material(Shader.Find("Sprites/Default"));
        drawLine.startColor = Color.red;
        drawLine.endColor = Color.red;
    }

    public void UpdateLine()
    {
        if (Input.GetMouseButton(0))
        {
            Debug.DrawRay(Camera.main.ScreenToWorldPoint(Input.mousePosition), GetMousePosition(), Color.red);
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                positions.Add(GetMousePosition());
                drawLine.positionCount = positions.Count; 
                drawLine.SetPositions(positions.ToArray());
                timer = timerDelay;
            }
        }
    }

    public void DeleteLine()
    {
        drawLine.positionCount = 0;
        positions.Clear();
    }

    public void SnapToSurface()
    {
        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, Mathf.Infinity, drawMask))
        {
            Vector3 projectedForward = Vector3.ProjectOnPlane(transform.forward, hit.normal).normalized;

            Quaternion targetRotation = Quaternion.LookRotation(projectedForward, hit.normal);

            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 10f * Time.deltaTime);

            transform.position = hit.point + transform.up * heightOffset;
        }
    }

    Vector3 GetMousePosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, drawMask))
        {
            Vector3 pos = hit.point + Vector3.up * 0.75f;
            return pos;
        }

        return ray.origin + ray.direction * 10;
    }
}
