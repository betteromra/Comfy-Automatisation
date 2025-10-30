using Unity.Cinemachine;
using UnityEngine;
using System;

public class CameraManager : MonoBehaviour
{
    [SerializeField] Camera _mainCamera;
    public Camera mainCamera { get => _mainCamera; }
    [SerializeField] CinemachineCamera _mainCinemachineCamera;
    public CinemachineCamera mainCinemachineCamera { get => _mainCinemachineCamera; }
    CinemachineOrbitalFollow _mainCameraOrbit;
    public CinemachineOrbitalFollow mainCameraOrbit { get => _mainCameraOrbit; }
    [SerializeField] Transform _cameraTarget;

    [Header("Camera Movement")]
    [SerializeField] Vector2 _moveSpeed;
    [SerializeField] AnimationCurve _moveSpeedZoomCurve = AnimationCurve.Linear(0, 0.5f, 1f, 1f);
    [SerializeField] float _moveSmoothing;
    [SerializeField] float _keyboardMoveSpeedAdjustement;
    [SerializeField] float _maxCameraDistance;
    [SerializeField] AnimationCurve _maxCameraDistanceZoomCurve = AnimationCurve.Linear(0, 0.2f, 1f, 1f);
    Vector3 _targetCameraTargetPosition = Vector2.zero;
    public event Action onMove;

    [Header("Camera Zoom")]
    [SerializeField] float _zoomSpeed;
    [SerializeField] float _zoomSmoothing;
    [SerializeField] AnimationCurve _zoomSpeedZoomCurve = AnimationCurve.Linear(0, 1f, 1f, .5f);
    float _targetOrbitRadialAxis = 0;
    float _zoomLevel = 0;
    public float zoomLevel { get => _zoomLevel; }
    public event Action onZoom;
    Player _player;

    private void Awake()
    {
        _mainCameraOrbit = _mainCinemachineCamera.GetComponent<CinemachineOrbitalFollow>();
        RefreshZoomLevel();
        _targetCameraTargetPosition = _cameraTarget.position;
        _targetOrbitRadialAxis = _mainCameraOrbit.RadialAxis.Value;
        _player = GameManager.instance.player;
    }

    void Update()
    {
        // if we didn't open a menu, then we can move the camera
        if (GameManager.instance.userInterfaceManager.currentBuildingUIOpen == null)
        {
            UpdateMovement();
            UpdateZoom();
        }
    }

    #region Control
    void UpdateMovement()
    {
        // Camera movement
        MoveCamera(_player.mouseMoveInput + _player.keyboardMoveInput * _keyboardMoveSpeedAdjustement);
    }
    void UpdateZoom()
    {
        // Camera Zoom
        ZoomCamera(_player.zoomInput);
    }
    void MoveCamera(Vector2 mouvement2dDelta)
    {
        Vector3 movement3d = new Vector3(mouvement2dDelta.x * _moveSpeed.x, 0, mouvement2dDelta.y * _moveSpeed.y);
        Vector3 motion = movement3d * Time.deltaTime;

        // The more zoomed you are the less move you will do
        float zoomMultiplier = _moveSpeedZoomCurve.Evaluate(_zoomLevel);
        _targetCameraTargetPosition += motion * zoomMultiplier;

        ClampCameraPosition();

        // If we don't need to move the Camera then we don't
        bool atTarget = Vector3.Distance(_targetCameraTargetPosition, _cameraTarget.position) < 0.01f;
        if (atTarget) return;

        // Smoothing the movement so it feels more floaty
        Vector3 smoothMotion = Vector3.zero;
        smoothMotion.x = Mathf.Lerp(_cameraTarget.position.x, _targetCameraTargetPosition.x, Time.deltaTime * _moveSmoothing);
        smoothMotion.z = Mathf.Lerp(_cameraTarget.position.z, _targetCameraTargetPosition.z, Time.deltaTime * _moveSmoothing);

        _cameraTarget.position = smoothMotion;

        if (!atTarget) onMove?.Invoke();
    }

    void ZoomCamera(float zoomDelta)
    {
        float motion = zoomDelta * _zoomSpeed * Time.deltaTime;
        InputAxis radialAxis = _mainCameraOrbit.RadialAxis;

        // if we go in another direction don't make it go in the wrong way
        // for some time of the blend
        int zoomDeltaSign = Math.Sign(zoomDelta);
        if (zoomDeltaSign != 0)
        {
            if (Math.Sign(_targetOrbitRadialAxis - radialAxis.Value) != zoomDeltaSign) _targetOrbitRadialAxis = radialAxis.Value;
        }
        // The more zoomed you are the more it zoom fast
        float zoomMultiplier = _zoomSpeedZoomCurve.Evaluate(_zoomLevel);
        _targetOrbitRadialAxis += motion * zoomMultiplier;

        // If we don't need to move the Camera then we don't
        bool atTarget = Mathf.Abs(radialAxis.Value - _targetOrbitRadialAxis) < 0.01f;
        if (atTarget) return;
        else onMove?.Invoke();

        // Clamping the zoom so there is a limit to the zoom in and out
        _targetOrbitRadialAxis = Mathf.Clamp(_targetOrbitRadialAxis, radialAxis.Range.x, radialAxis.Range.y);

        // smoothing the zoom so it feels less clancky with the wheel scroll dent
        float smoothMotion = Mathf.Lerp(radialAxis.Value, _targetOrbitRadialAxis, Time.deltaTime * _zoomSmoothing);

        _mainCameraOrbit.RadialAxis.Value = smoothMotion;
        RefreshZoomLevel();
        if (!atTarget) onZoom?.Invoke();
    }
    #endregion

    #region Calculate

    void RefreshZoomLevel()
    {
        _zoomLevel = Mathf.InverseLerp(_mainCameraOrbit.RadialAxis.Range.x, _mainCameraOrbit.RadialAxis.Range.y, _mainCameraOrbit.RadialAxis.Value);
    }

    void ClampCameraPosition()
    {
        // Clamping the position so the camera can't go ouside of the game bound
        Vector3 clampedPos = _targetCameraTargetPosition;

        Vector2 clampedPos2d = new Vector2(Mathf.Abs(clampedPos.x), Mathf.Abs(clampedPos.z));
        float manhattanDistance = clampedPos2d.x + clampedPos2d.y;
        float manhattanDifference = manhattanDistance - _maxCameraDistance * _maxCameraDistanceZoomCurve.Evaluate(_zoomLevel);

        // Add a little bit of room for the bottom
        // Debug.Log(clampedPos.z);
        // if (clampedPos.z < 0) manhattanDifference -= 10f * Mathf.Abs(1 - _zoomLevel);

        // we outside of the bound
        if (manhattanDifference > 0f)
        {
            // remove overflow from the larger axis
            if (clampedPos2d.x > clampedPos2d.y)
            {
                clampedPos2d.x -= manhattanDifference;
            }
            else
            {
                clampedPos2d.y -= manhattanDifference;
            }

            // put back the right sign
            clampedPos.x = Mathf.Sign(clampedPos.x) * clampedPos2d.x;
            clampedPos.z = Mathf.Sign(clampedPos.z) * clampedPos2d.y;
        }

        _targetCameraTargetPosition = clampedPos;
    }
    #endregion
}
