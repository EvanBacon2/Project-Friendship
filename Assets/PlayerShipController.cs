using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShipController : MonoBehaviour {
    private Rigidbody selfRigidBody;
    private float speed;
    private float maxSpeed;

    // Start is called before the first frame update
    void Start() {
        selfRigidBody = GetComponent<Rigidbody>();
        speed = 45;
        maxSpeed = 30;
    }

    // Update is called once per frame
    void Update() {
        
    }

	private void FixedUpdate() {
        moveShip();
        rotateToMouse();
    }

	private void rotateToMouse() {
        Vector3 mousePos = Input.mousePosition;
        Vector3 playerPos = Camera.main.WorldToScreenPoint(transform.position);
        Debug.Log(mousePos);
        float turnAngle = Mathf.Atan2(mousePos.y - playerPos.y, mousePos.x - playerPos.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(turnAngle - 90, Vector3.forward);
    }

    private void moveShip() {
        Vector2 movement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (selfRigidBody.velocity.magnitude <= maxSpeed)
            selfRigidBody.AddForce(movement * speed);
        if (selfRigidBody.velocity.magnitude > maxSpeed)
            selfRigidBody.velocity = selfRigidBody.velocity.normalized * maxSpeed;
    }
}
