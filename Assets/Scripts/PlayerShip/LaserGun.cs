using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserGun : MonoBehaviour {
    public GameObject bullet;

    public float fireRate = .1f;
    private float fireStamp = 0;

    void Update() {
        if (Input.GetMouseButton(0) && Time.time > fireStamp + fireRate) {
            fireStamp = Time.time;
            Instantiate(bullet, transform.position, transform.rotation);
        }
    }
}
