using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(NavMeshAgent))]
public class NpcPathRenderer : MonoBehaviour
{
    private LineRenderer _line;
    private NavMeshAgent _agent;

    void Start()
    {
        _line = GetComponent<LineRenderer>();
        _agent = GetComponentInParent<NavMeshAgent>();
    }

    public void SetVisibilityOfLineRenderer(bool visible) => _line.enabled = visible;

    public void DrawPathBetween(Vector3 start, Vector3 end)
    {
        NavMeshPath path = new();
        if (NavMesh.CalculatePath(start, end, NavMesh.AllAreas, path))
        {
            if (path.corners.Length >= 2)
            {
                _line.positionCount = path.corners.Length;
                _line.SetPositions(path.corners);
                _line.enabled = true;
            }
        }
    }

    public void DrawPathThroughNPC(Vector3 start, Vector3 end)
    {
        List<Vector3> combinedCorners = new();

        NavMeshPath pathToNPC = new();
        NavMeshPath pathFromNPC = new();

        if (NavMesh.CalculatePath(start, _agent.transform.position, NavMesh.AllAreas, pathToNPC))
        {
            if (pathToNPC.corners.Length > 1)
                combinedCorners.AddRange(pathToNPC.corners);
        }
        
        if (NavMesh.CalculatePath(_agent.transform.position, end, NavMesh.AllAreas, pathFromNPC))
        {
            if (pathFromNPC.corners.Length > 1)
                combinedCorners.AddRange(pathFromNPC.corners.Skip(1));
        }

        if (combinedCorners.Count >= 2)
        {
            _line.positionCount = combinedCorners.Count;
            _line.SetPositions(combinedCorners.ToArray());
            _line.enabled = true;
        }
        else
        {
            _line.enabled = false;
        }
    }
}
