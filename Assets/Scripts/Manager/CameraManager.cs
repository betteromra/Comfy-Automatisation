using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] CinemachineCamera _mainCamera;
    public CinemachineCamera mainCamera { get => _mainCamera; }
    CinemachineOrbitalFollow _mainCameraOrbit;
    public CinemachineOrbitalFollow mainCameraOrbit { get => _mainCameraOrbit; }
    [SerializeField] Transform _cameraTarget;

    [Header("Camera Movement")]
    [SerializeField] Vector2 _moveSpeed;
    [SerializeField] AnimationCurve _moveSpeedZoomCurve = AnimationCurve.Linear(0, 0.5f, 1f, 1f);
    [SerializeField] float _moveSmoothing;
    [SerializeField] float _keyboardMoveSpeedAdjustement;
    [SerializeField] float _maxCameraDistance;
    Vector3 _targetCameraTargetPosition = Vector2.zero;

    [Header("Camera Zoom")]
    [SerializeField] float _zoomSpeed;
    [SerializeField] float _zoomSmoothing;
    [SerializeField] AnimationCurve _zoomSpeedZoomCurve = AnimationCurve.Linear(0, 1f, 1f, .5f);
    float _targetOrbitRadialAxis = 0;
    float _zoomLevel = 0;

    private void Awake()
    {
        _mainCameraOrbit = _mainCamera.GetComponent<CinemachineOrbitalFollow>();
        RefreshZoomLevel(_mainCameraOrbit.RadialAxis);
        _targetOrbitRadialAxis = _mainCameraOrbit.RadialAxis.Value;
    }

    void Update()
    {
        UpdateMovement();
    }

    #region Control
    void UpdateMovement()
    {
        Player player = GameManager.instance.player;
        // Camera movement
        MoveCamera(player.mouseMoveInput + player.keyboardMoveInput * _keyboardMoveSpeedAdjustement);

        // Camera Zoom
        ZoomCamera(player.zoomInput);
    }
    void MoveCamera(Vector2 mouvement2dDelta)
    {
        Vector3 movement3d = new Vector3(mouvement2dDelta.x * _moveSpeed.x, 0, mouvement2dDelta.y * _moveSpeed.y);
        Vector3 motion = movement3d * Time.unscaledDeltaTime;

        // The more zoomed you are the less move you will do
        float zoomMultiplier = _moveSpeedZoomCurve.Evaluate(_zoomLevel);
        _targetCameraTargetPosition += motion * zoomMultiplier;

        // If we don't need to move the Camera then we don't
        if (Vector3.Distance(_targetCameraTargetPosition, _cameraTarget.position) < 0.01f) return;

        // Clamping the position so the camera can't go ouside of the game bound
        Vector3 clampedPos = _targetCameraTargetPosition;

        Vector2 clampedPos2d = new Vector2(Mathf.Abs(clampedPos.x), Mathf.Abs(clampedPos.z));
        float manhattanDifference = clampedPos2d.x + clampedPos2d.y - _maxCameraDistance;

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

        // Smoothing the movement so it feels more floaty
        Vector3 smoothMotion = Vector3.zero;
        smoothMotion.x = Mathf.Lerp(_cameraTarget.position.x, _targetCameraTargetPosition.x, Time.unscaledDeltaTime * _moveSmoothing);
        smoothMotion.z = Mathf.Lerp(_cameraTarget.position.z, _targetCameraTargetPosition.z, Time.unscaledDeltaTime * _moveSmoothing);

        _cameraTarget.position = smoothMotion;
    }

    void ZoomCamera(float zoomDelta)
    {
        float motion = zoomDelta * _zoomSpeed * Time.unscaledDeltaTime;
        InputAxis radialAxis = _mainCameraOrbit.RadialAxis;

        // The more zoomed you are the more it zoom fast
        float zoomMultiplier = _zoomSpeedZoomCurve.Evaluate(_zoomLevel);
        _targetOrbitRadialAxis += motion * zoomMultiplier;

        // If we don't need to move the Camera then we don't
        if (Mathf.Abs(radialAxis.Value - _targetOrbitRadialAxis) < 0.01f) return;

        RefreshZoomLevel(radialAxis);

        // Clamping the zoom so there is a limit to the zoom in and out
        _targetOrbitRadialAxis = Mathf.Clamp(_targetOrbitRadialAxis, radialAxis.Range.x, radialAxis.Range.y);

        // smoothing the zoom so it feels less clancky with the wheel scroll dent
        float smoothMotion = Mathf.Lerp(radialAxis.Value, _targetOrbitRadialAxis, Time.unscaledDeltaTime * _zoomSmoothing);

        _mainCameraOrbit.RadialAxis.Value = smoothMotion;
    }
    #endregion

    #region Calculate

    void RefreshZoomLevel(InputAxis radialAxis)
    {
        _zoomLevel = Mathf.InverseLerp(radialAxis.Range.x, radialAxis.Range.y, radialAxis.Value);
    }
    #endregion
}
