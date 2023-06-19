using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBullet : MonoBehaviour {
    public float lifeTime = 5;
    public float speed = 15f;
    
    private float lifeStart;
    private Vector2 direction;

    void Start() {
        lifeStart = Time.time;
        direction = new Vector2(Mathf.Cos((transform.rotation.eulerAngles.z + 90) * Mathf.Deg2Rad), Mathf.Sin((transform.rotation.eulerAngles.z + 90) * Mathf.Deg2Rad));
    }

    void FixedUpdate() {
        transform.Translate(direction * speed, Space.World);
        
        if (Time.time > lifeStart + lifeTime)
            Destroy(gameObject);
    }

	private void OnTriggerEnter(Collider other) {
        Debug.Log(other.tag);
        if (!other.CompareTag("Player") && !other.CompareTag("Rope"))
            Destroy(gameObject);
    }
}
