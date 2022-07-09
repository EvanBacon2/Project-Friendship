using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShipController : MonoBehaviour {
    private Rigidbody selfRigidBody;

    private float speed;

    // Start is called before the first frame update
    void Start() {
        selfRigidBody = GetComponent<Rigidbody>();
        speed = 1500;
    }

    // Update is called once per frame
    void Update() {
        
    }

	private void FixedUpdate() {
        rotateToMouse();
        moveShip();
	}

	private void rotateToMouse() {
        Vector3 mouse_pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float turnAngle = Mathf.Atan2(mouse_pos.y - transform.position.y, mouse_pos.x - transform.position.x) * Mathf.Rad2Deg;
        Quaternion lookRotation = Quaternion.AngleAxis(turnAngle - 90, new Vector3(0,0,70));
        transform.rotation = lookRotation;
    }

    private void moveShip() {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        Vector2 movement = new Vector2(moveHorizontal, moveVertical);
        if (selfRigidBody.velocity.magnitude < 2000)
            selfRigidBody.AddForce(movement * speed);
    }
}
