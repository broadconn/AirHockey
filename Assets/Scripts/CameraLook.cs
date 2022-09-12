using UnityEngine;

public class CameraLook : MonoBehaviour
{
    [SerializeField] Transform lookAt;
    [SerializeField] float lookSpeed = 10;

    private void LateUpdate() { 
        var targetRotation = Quaternion.LookRotation(lookAt.position / 10 - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lookSpeed * Time.deltaTime);
    }
}
