
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

    private void Start()
    {
        positions = new List<Vector3>();
        timer = timerDelay;
    }

    public void StartLine(Vector3 position)
    {
        drawLine.positionCount = 1;
        drawLine.startWidth = drawLine.endWidth = lineWidth;
        drawLine.endWidth = lineWidth;
        drawLine.material = new Material(Shader.Find("Sprites/Default"));
        drawLine.startColor = RandomColor();
        drawLine.endColor = RandomColor();
    }

    public void UpdateLine()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null || !mouse.leftButton.isPressed)
        {
            return;
        }

        Vector2 mouseScreen = mouse.position.ReadValue();
        Vector3 screenPoint = new Vector3(mouseScreen.x, mouseScreen.y, 0f);
        Debug.DrawRay(Camera.main.ScreenToWorldPoint(screenPoint), GetMousePosition(), Color.red);
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            positions.Add(GetMousePosition());
            drawLine.positionCount = positions.Count;
            drawLine.SetPositions(positions.ToArray());
            timer = timerDelay;
        }
    }

    public void DeleteLine()
    {
        drawLine.positionCount = 0;
        positions.Clear();
    }

    Color RandomColor()
    {
        return Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
    }

    Vector3 GetMousePosition()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null)
        {
            return Vector3.zero;
        }

        Ray ray = Camera.main.ScreenPointToRay(mouse.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, drawMask))
        {
            return hit.point + Vector3.up * 0.1f;
        }

        return ray.origin + ray.direction * 10;
    }
}
