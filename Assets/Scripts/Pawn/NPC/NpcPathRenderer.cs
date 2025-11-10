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

    void Start()
    {
        _line = GetComponent<LineRenderer>();
    }

    public void SetVisibilityOfLineRenderer(bool visible) => _line.enabled = visible;

    public void DrawPath(List<BuildingNode> nodeLinks)
    {
        if (nodeLinks.Count <= 1)
        {
            if (nodeLinks.Count != 1)
            {
                SetVisibilityOfLineRenderer(false);
            }

            return;
        }

        List<Vector3> fullPath = new();

        Vector3 pastNodePosition = nodeLinks[^1].transform.position + Vector3.one * .01f;
        foreach (BuildingNode buildingNode in nodeLinks)
        {
            NavMeshPath path = new NavMeshPath();
            Vector3 nodePosition = buildingNode.transform.position;

            if (NavMesh.CalculatePath(pastNodePosition, nodePosition, NavMesh.AllAreas, path))
            {
                if (fullPath.Count > 0)
                    fullPath.RemoveAt(fullPath.Count - 1);

                fullPath.AddRange(path.corners);
            }

            pastNodePosition = nodePosition;
        }

        _line.enabled = fullPath.Count > 1;
        if (_line.enabled)
        {
            _line.positionCount = fullPath.Count;
            _line.SetPositions(fullPath.ToArray());
        }
    }

    public void DrawPathNoBuildingNode(Vector3 start, Vector3 end)
    {
        List<Vector3> combinedCorners = new();

        NavMeshPath path = new();

        if (NavMesh.CalculatePath(start, end, NavMesh.AllAreas, path))
        {
            if (path.corners.Length > 1)
                combinedCorners.AddRange(path.corners);
        }

        _line.enabled = combinedCorners.Count > 1;
        if (_line.enabled)
        {
            _line.positionCount = combinedCorners.Count;
            _line.SetPositions(combinedCorners.ToArray());
        }
    }
}
