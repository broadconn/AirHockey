using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts {
    public class FloatInPlace: MonoBehaviour { 
        [SerializeField] float bobMag = 0.2f;
        [SerializeField] float bobSpeed = 0.2f;
        [SerializeField] float bobOffset = 0;
        Vector3 startPos;

        private void Start() {
            startPos = transform.position;
        }

        private void Update() { 
            transform.position = new Vector3(startPos.x, startPos.y + Mathf.Sin((Time.time + bobOffset) * bobSpeed) * bobMag, startPos.z);
        }
    }
}
