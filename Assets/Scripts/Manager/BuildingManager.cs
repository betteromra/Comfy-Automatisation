using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    [SerializeField] Building[] _buildings;
    public Building[] buildings { get => _buildings; }
}
