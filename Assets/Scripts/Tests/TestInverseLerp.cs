using Assets.Scripts.Utility;
using UnityEngine;

public class TestInverseLerp : MonoBehaviour
{
    public Transform A;
    public Transform B;
    public Transform P;

    // Update is called once per frame
    void Update()
    {
        print(Utilities.InverseLerp(A.position, B.position, P.position));
    }
}
