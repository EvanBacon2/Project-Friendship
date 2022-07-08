using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteTest : MonoBehaviour {
    private float rotLimit = 60.0f;
    private float rotDir = 1.0f;
    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        Transform tr = GetComponent<Transform>();
        float rotation = tr.eulerAngles.y > 180 ? tr.eulerAngles.y - 360 : tr.eulerAngles.y;
        if (Math.Abs(rotation) > rotLimit) {
            rotDir *= -1.0f;
            if (rotDir < 0)
                tr.eulerAngles = new Vector3(tr.eulerAngles.x, rotLimit, tr.eulerAngles.z);
            else
                tr.eulerAngles = new Vector3(tr.eulerAngles.x, 360 - rotLimit, tr.eulerAngles.z);
        }
        float speed = (rotLimit - Math.Abs(rotation)) / rotLimit; 
        
        //tr.Rotate(0.0f, rotDir * (0.03f + 0.4f * speed), 0.0f, Space.Self);
        tr.Translate(rotDir * 0.8f, 0.0f, 0.0f);
    }
}
