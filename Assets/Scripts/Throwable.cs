using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Throwable : MonoBehaviour {
    public float health;

    private Rigidbody rigidBody;

	private void Start() {
		rigidBody = GetComponent<Rigidbody>();
	}

	private void Update() {
		if (health <= 0)
			Destroy(gameObject);
	}

	private void OnCollisionEnter(Collision collision) {
		if (collision.gameObject.CompareTag("Wall"))
			health -= collision.impulse.magnitude / 4.0f;
		Debug.Log(collision.impulse.magnitude);
	}
}
