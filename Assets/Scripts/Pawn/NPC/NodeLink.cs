using UnityEngine;

public class NodeLink
{
    public GameObject NodeA { get => _nodeA; }
    private GameObject _nodeA;
    public GameObject NodeB { get => _nodeB; }
    private GameObject _nodeB;

    public NodeLink(GameObject nodeA, GameObject nodeB)
    {
        _nodeA = nodeA;
        _nodeB = nodeB;
    }

    public bool Contains(GameObject node) => node == _nodeA || node == _nodeB;

    public bool Matches(GameObject nodeA, GameObject nodeB) => nodeA == _nodeA && nodeB == _nodeB;

    public override bool Equals(object obj)
    {
        if (obj is not NodeLink other)
            return false;

        return _nodeA == other.NodeA && _nodeB == other.NodeB;
    }


    public override int GetHashCode()
    {
        int hashA = _nodeA != null ? _nodeA.GetHashCode() : 0;
        int hashB = _nodeB != null ? _nodeB.GetHashCode() : 0;

        return hashA ^ hashB;
    }
}
