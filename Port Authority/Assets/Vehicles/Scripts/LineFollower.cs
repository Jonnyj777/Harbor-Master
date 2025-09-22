using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LineFollower : MonoBehaviour
{
    public GameObject pathLinePrefab;
    private DrawLine drawControl;

    public float speed = 5f;

    bool pathFinding = false;
    Vector3[] positions;
    int moveIndex = 0;

    // draw line once the object is clicked
    private void OnMouseDown()
    {
        // if this vehicle already has a path, clear it
        if (drawControl != null)
            Destroy(drawControl.gameObject);

        // create a new pathLine instance for this vehicle alone
        GameObject pathLine = Instantiate(pathLinePrefab);
        drawControl = pathLine.GetComponent<DrawLine>();

        pathFinding = false;
        drawControl.StartLine(transform.position);
    }

    private void OnMouseDrag()
    {
        if (drawControl != null)
            drawControl.UpdateLine();
    }

    private void OnMouseUp()
    {
        if (drawControl != null)
        {
            positions = new Vector3[drawControl.drawLine.positionCount];
            drawControl.drawLine.GetPositions(positions);
            pathFinding = true;
            moveIndex = 0;
        }
    }

    private void Update()
    {
        if (pathFinding)
        {
            // update position and direction of object
            Vector3 currentPos = positions[moveIndex];
            transform.position = Vector3.MoveTowards(transform.position, currentPos, speed * Time.deltaTime);

            Vector3 direction = (currentPos - transform.position).normalized;

            if (direction.sqrMagnitude > 0.001f) 
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                float angleOffset = Quaternion.Angle(transform.rotation, targetRotation);

                if (angleOffset > 5f)
                {
                    transform.LookAt(currentPos);
                }
            }

            float distance = Vector3.Distance(currentPos, transform.position);
            if (distance <= 0.05f)
            {
                moveIndex++;
            }

            // remove the part of line already traveled on
            if (drawControl.drawLine.positionCount > 1)
            {
                Vector3[] currentPositions = new Vector3[drawControl.drawLine.positionCount];
                drawControl.drawLine.GetPositions(currentPositions);

                currentPositions[0] = transform.position;

                if (Vector3.Distance(currentPositions[0], currentPositions[1]) < 0.05f)
                {
                    List<Vector3> temp = new List<Vector3>(currentPositions);
                    temp.RemoveAt(0);
                    currentPositions = temp.ToArray();
                }

                drawControl.drawLine.positionCount = currentPositions.Length;
                drawControl.drawLine.SetPositions(currentPositions);
            }

            // remove line after following finishes
            if (moveIndex > positions.Length - 1)
            {
                pathFinding = false;
                drawControl.DeleteLine();
            }
        }
        else
        {
            // Move forward at a constant velocity
            transform.position += transform.forward * speed * Time.deltaTime;
        }
    }
}
