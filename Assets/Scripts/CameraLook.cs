using UnityEngine;

public class CameraLook : MonoBehaviour {
    [SerializeField] Transform defaultLookPos;
    [SerializeField] Transform lookTarget;
    [SerializeField] float lookSpeed = 1;
    [SerializeField] float deviationLimit = 2;

    private void LateUpdate() {
        var defToLook = lookTarget.position - defaultLookPos.position;
        var tgtPos = defaultLookPos.position + defToLook * defToLook.magnitude / deviationLimit;
        var targetRotation = Quaternion.LookRotation(tgtPos - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lookSpeed * Time.deltaTime);
    }
}
