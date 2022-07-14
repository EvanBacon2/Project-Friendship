using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShipModel : MonoBehaviour {
    private Rigidbody rigidBody;

    public Vector3 direction;
    public float magnitude;

    public Vector3 position {
        get { return transform.position; }
        set { transform.position = value; }
    }
    public Quaternion rotation {
        get { return transform.rotation; }
        set { transform.rotation = value; }
    }

    public float acceleration;
    public float maxSpeed;

    void Start() {
        rigidBody = GetComponent<Rigidbody>();

        acceleration = 40;
        maxSpeed = 25;
    }

    public Vector3 getVelocity() {
        return rigidBody.velocity;   
    }
    public void setVelocity() {
        rigidBody.velocity = direction.normalized * magnitude;
    }
}
