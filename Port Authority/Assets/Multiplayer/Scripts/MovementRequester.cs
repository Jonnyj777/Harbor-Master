using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MovementRequester : NetworkBehaviour
{
    [SerializeField]
    private Camera camera;

    private List<Vector3> linePositions;

    private bool isPressedDown = false;
    void Update()
    {
        if(Input.GetMouseButtonDown(0)) { 
            if(!isPressedDown)
            {
                //CmdRequestControl(netId);
            }
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray, out var hit))
            {
                if(hit.collider.TryGetComponent<LineFollow>(out var vehicle))
                {
                    //CmdRequestMove(netId, vehicle.netId, mousePos);
                }
            }
        }
    }
}
