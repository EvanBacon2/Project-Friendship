using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shootable : MonoBehaviour{
    public float health;

    void Update() {
        if (health == 0)
            Destroy(this.gameObject);
    }

	private void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("Bullet"))
            health -= 1;
    }
}
