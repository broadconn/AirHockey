using UnityEngine;

public class DebugChildEnabler : MonoBehaviour
{
    void Update()
    {
        foreach(Transform t in transform) {
            t.gameObject.SetActive(GameController.Instance.Debug);
        }
    }
}
