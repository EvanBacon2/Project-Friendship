using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShipController : MonoBehaviour {
    private Rigidbody selfRigidBody;
    private float speed;

    // Start is called before the first frame update
    void Start() {
        selfRigidBody = GetComponent<Rigidbody>();
        speed = 100;
    }

    // Update is called once per frame
    void Update() {
        rotateToMouse();
    }

	private void FixedUpdate() {
        moveShip();
	}

	private void rotateToMouse() {
        /*Vector3 mouse_pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float turnAngle = Mathf.Atan2(mouse_pos.y - transform.position.y, mouse_pos.x - transform.position.x) * Mathf.Rad2Deg;
        Quaternion lookRotation = Quaternion.AngleAxis(turnAngle - 90, new Vector3(0,0,70));
        transform.rotation = lookRotation;*/

        Vector3 mouse_pos = Input.mousePosition;
        float turnAngle = Mathf.Atan2(Screen.height / 2 - mouse_pos.y, 
                                      Screen.width / 2 - mouse_pos.x) * Mathf.Rad2Deg;
        Quaternion lookRotation = Quaternion.AngleAxis(turnAngle - 90, new Vector3(0, 0, 70));
        transform.rotation = lookRotation;
    }

    private void moveShip() {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        Vector2 movement = new Vector2(moveHorizontal, moveVertical);
        selfRigidBody.AddForce(movement * speed);
        if (selfRigidBody.velocity.magnitude > 120)
            selfRigidBody.velocity = selfRigidBody.velocity.normalized * 120;
    }
}
