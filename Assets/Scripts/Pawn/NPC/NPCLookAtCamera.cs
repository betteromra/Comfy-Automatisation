using UnityEngine;

public class NPCLookAtCamera : MonoBehaviour
{
    void Update()
    {
        Vector3 targetPosition = GameManager.instance.cameraManager.mainCamera.transform.position;
        targetPosition.y = transform.position.y; // keep same height
        transform.LookAt(targetPosition);
        transform.Rotate(0f, 180f, 0f);
    }
}
