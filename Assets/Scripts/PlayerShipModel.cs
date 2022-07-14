using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShipModel : MonoBehaviour {
    public Rigidbody selfRigidBody;
    public Transform selfTransform;

    public float acceleration;
    public float maxSpeed;

    void Start() {
        selfRigidBody = GetComponent<Rigidbody>();
        selfTransform = GetComponent<Transform>();

        acceleration = 40;
        maxSpeed = 25;
    }
}
