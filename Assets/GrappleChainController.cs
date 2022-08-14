using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleChainController : MonoBehaviour{
    void Start(){
        GetComponent<Rigidbody>().inertiaTensor = Vector3.one * 1f;
    }

    void Update(){
        
    }
}
