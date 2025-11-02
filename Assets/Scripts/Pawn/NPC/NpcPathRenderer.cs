using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(LineRenderer))]
public class NpcPathRenderer : MonoBehaviour
{
    private LineRenderer _line;
    private Transform _npcTransform;

    void Start()
    {
        _line = GetComponent<LineRenderer>();
        _npcTransform = transform.parent;
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

        if (NavMesh.CalculatePath(start, _npcTransform.position, NavMesh.AllAreas, pathToNPC))
        {
            if (pathToNPC.corners.Length > 1)
                combinedCorners.AddRange(pathToNPC.corners);
        }
        
        if (NavMesh.CalculatePath(_npcTransform.position, end, NavMesh.AllAreas, pathFromNPC))
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
