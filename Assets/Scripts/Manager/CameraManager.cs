using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] CinemachineCamera _mainCamera;
    public CinemachineCamera mainCamera { get => _mainCamera; }
    CinemachineOrbitalFollow _mainCameraOrbit;
    public CinemachineOrbitalFollow mainCameraOrbit { get => _mainCameraOrbit; }

    private void Awake()
    {
        _mainCameraOrbit = _mainCamera.GetComponent<CinemachineOrbitalFollow>();
    }

}
