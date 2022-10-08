using Assets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestInverseLerp : MonoBehaviour
{
    public Transform A;
    public Transform B;
    public Transform P;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        print(PuckFuturePath.InverseLerp(A.position, B.position, P.position));
    }
}
