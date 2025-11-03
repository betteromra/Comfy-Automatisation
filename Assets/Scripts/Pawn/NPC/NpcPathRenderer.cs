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

    public void DrawPathBetween(List<NodeLink> nodeLinks)
    {
        List<Vector3> fullPath = new();
        Vector3? lastPoint = null;

        foreach (NodeLink link in nodeLinks)
        {
            var path = new NavMeshPath();
            Vector3 start = link.NodeA.transform.position;
            Vector3 end = link.NodeB.transform.position;

            if (lastPoint != null)
            {
                if (NavMesh.CalculatePath(lastPoint.Value, start, NavMesh.AllAreas, path))
                {
                    if (fullPath.Count > 0)
                        fullPath.RemoveAt(fullPath.Count - 1);

                    fullPath.AddRange(path.corners);
                }
            }

            if (NavMesh.CalculatePath(start, end, NavMesh.AllAreas, path))
            {
                if (fullPath.Count > 0)
                    fullPath.RemoveAt(fullPath.Count - 1); // avoid duplicate junction point

                fullPath.AddRange(path.corners);
            }

            lastPoint = end;
        }
        
        if(nodeLinks[^1].NodeB != nodeLinks[0].NodeA)
        {
            var path = new NavMeshPath();
            Vector3 start = nodeLinks[^1].NodeB.transform.position;
            Vector3 end = nodeLinks[0].NodeA.transform.position;

            if (lastPoint != null)
            {
                if (NavMesh.CalculatePath(lastPoint.Value, start, NavMesh.AllAreas, path))
                {
                    if (fullPath.Count > 0)
                        fullPath.RemoveAt(fullPath.Count - 1);

                    fullPath.AddRange(path.corners);
                }
            }

            if (NavMesh.CalculatePath(start, end, NavMesh.AllAreas, path))
            {
                if (fullPath.Count > 0)
                    fullPath.RemoveAt(fullPath.Count - 1); // avoid duplicate junction point

                fullPath.AddRange(path.corners);
            }
        }

        if (fullPath.Count >= 2)
        {
            _line.positionCount = fullPath.Count;
            _line.SetPositions(fullPath.ToArray());
            _line.enabled = true;
        }
        else
        {
            _line.enabled = false;
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
