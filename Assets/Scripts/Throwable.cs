using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Allows for objects to be grabbed and thrown by a rope

class Throwable : MonoBehaviour {
    public float health;
	public bool hookable = true;

    private Rigidbody rigidBody;
	private HookSegment hook;

	private void Start() {
		rigidBody = GetComponent<Rigidbody>();
	}

	private void Update() {
		if (health <= 0) {
			ThrowOffset.tracked = null;
			Destroy(gameObject);
		}
	}

	private void OnTriggerEnter(Collider other) {
		if (other.gameObject.CompareTag("Rope") && hookable)
			hook = other.GetComponent<HookSegment>();
	}

	private void OnCollisionEnter(Collision collision) {
		if (collision.gameObject.CompareTag("Wall")) {
			health -= collision.impulse.magnitude / 4.0f;
			if (hook != null && collision.impulse.magnitude > 2) {
				hook.unHook();
				hookable = false;
			}
		}
	}

	public void unHook() {
		hook = null;
	}
}
