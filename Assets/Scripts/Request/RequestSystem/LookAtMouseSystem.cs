using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class LookAtMouseSystem : RequestSystem<ShipState> {
    private RECSShipbody rb;
    private float responsiveness = 15;

    float prevRotation = 0;
    float rotation = 0;
    float scale = 1;
    float sign = 1;

    float gapTotal = 0;
    float prevScaledRotation = 0;

    ScaledRotation scaledShip = new ScaledRotation();
    ScaledRotation scaledMouse = new ScaledRotation();

    ScaledDifference shipMouseDiff = new ScaledDifference(0, 0, 4);

    public override void OnStateReceived(object sender, ShipState state) {
        rb = state.rigidbody;

        Vector2 mousePos = state.lookDirection;
        Vector2 playerPos = Camera.main.ViewportToScreenPoint(distort(Camera.main.WorldToViewportPoint(rb.Position.pendingValue())));
        
        Vector3 cameraPos = Camera.main.transform.position;
        Vector3 startPoint = new Vector3();
        Vector3 endPoint = new Vector3();

        /*rotation = rb.Rotation.value.eulerAngles.z;

        if (rotation >= 0 && rotation < 90 && prevRotation >= 270 && prevRotation < 360) 
            scale = Mathf.Min(4, scale + (scale == -1 ? 2 : 1));

        if (rotation >= 270 && rotation < 360 && prevRotation >= 0 && prevRotation < 90) 
            scale = Mathf.Max(-4, scale - (scale == 1 ? 2 : 1));

        prevRotation = rotation;

        float scaledRotation;
    
        if (scale > 0)
            scaledRotation = rotation + (360 * (scale - 1));
        else
            scaledRotation = -(360 - rotation) + (360 * (scale + 1));*/
        
        float shipAngle = rb.Rotation.pendingValue().eulerAngles.z + 
                        rb.AngularVelocity.pendingValue().z * Time.fixedDeltaTime;
        float mouseAngle = Mathf.Atan2(mousePos.y - playerPos.y, mousePos.x - playerPos.x) * Mathf.Rad2Deg - 90;

        if (mouseAngle < 0) 
            mouseAngle += 360;

        //scaledShip.rotateTo(shipAngle);
        //scaledMouse.rotateTo(mouseAngle);

        shipMouseDiff.setStart(shipAngle);
        shipMouseDiff.setEnd(mouseAngle);
        
        //Debug.Log("pending " + scaledPending.value + " goal " + scaledGoal.value);

        float angSign = Mathf.Sign(shipMouseDiff.arc);//Mathf.Sign(scaledShip.value);

        Color[] lineColors =  {Color.white, Color.cyan, Color.green};

        float pendingRotation = 0;//scaledShip.value;//rb.Rotation.pendingValue().eulerAngles.z + rb.AngularVelocity.pendingValue().z * Time.fixedDeltaTime; 
        float goalRotation = shipMouseDiff.arc;//scaledMouse.value;//Mathf.Atan2(mousePos.y - playerPos.y, mousePos.x - playerPos.x) * Mathf.Rad2Deg - 90;
       
        //Debug.Log("pending " + pendingRotation);
        //Debug.Log("goal " + goalRotation);

        //Debug.Log("ship " + shipAngle);
        //Debug.Log("mouse " + mouseAngle);
        
        Debug.Log(shipMouseDiff.arc);

        /*if (pendingRotation < 0) 
            pendingRotation += 360;
        if (goalRotation < 0) 
            goalRotation += 360;*/
        
        float angleGap = goalRotation - pendingRotation;
        //gapTotal += angleGap;
        //Debug.Log(angleGap);

        //if (Mathf.Abs(angleGap) > 180)
            //angleGap = -(Mathf.Sign(angleGap) * 360 - angleGap);
        
        float targetVelocity = Mathf.Clamp(angleGap * responsiveness, 
                                -rb.AngularMax.pendingValue(), rb.AngularMax.pendingValue());
        float currentVelocity = rb.AngularVelocity.pendingValue().z * Mathf.Rad2Deg;
        float velocityChange = targetVelocity - currentVelocity;
        
        velocityChange = Mathf.Clamp(velocityChange, 
                                    -rb.AngularAcceleration.pendingValue(), 
                                    rb.AngularAcceleration.pendingValue()) * Mathf.Deg2Rad;

        rb.Torque.mutate(RequestClass.LookAtMouse, (List<(Vector3, ForceMode)> torques) => {
            torques.Add((new Vector3(0, 0, velocityChange), ForceMode.VelocityChange));
            return torques;
        });

        int pendingInt = Mathf.RoundToInt(Mathf.Abs(scaledShip.value));

        int iStart;
        int iEnd;

        if (shipMouseDiff.arc > 0) {
            iStart = Mathf.RoundToInt(shipMouseDiff.start);
            iEnd = Mathf.RoundToInt(shipMouseDiff.end);
        } else {
            iStart = Mathf.RoundToInt(shipMouseDiff.end);
            iEnd = Mathf.RoundToInt(shipMouseDiff.start);
        }

        for (int i = 0; i < Mathf.RoundToInt(Mathf.Abs(shipMouseDiff.arc)); i++) {
            int ring = Mathf.Min(2, (int)Mathf.Floor((i - pendingInt) / 360));
            float radius = (5 + (ring * 1));

            float ang = shipMouseDiff.start + (i * angSign) + 90;

            startPoint.x = cameraPos.x + Mathf.Cos(ang * Mathf.Deg2Rad) * radius;
            startPoint.y = cameraPos.y + Mathf.Sin(ang * Mathf.Deg2Rad) * radius;

            endPoint.x = cameraPos.x + Mathf.Cos((ang + angSign) * Mathf.Deg2Rad) * radius;
            endPoint.y = cameraPos.y + Mathf.Sin((ang + angSign) * Mathf.Deg2Rad) * radius;

            Debug.DrawLine(startPoint, endPoint, lineColors[ring], Time.fixedDeltaTime);
        }

        //prevScaledRotation = scaledRotation;
    }

    /*
     * Adjusts viewport coordinates of playerShip to account for LensDistortion
     */
    public Vector2 distort(Vector2 uv) {
        LensDistortion fisheye;
        GameObject.Find("Fisheye").GetComponent<Volume>().profile.TryGet(out fisheye);

        float amount = 1.6f * Mathf.Max(Mathf.Abs(fisheye.intensity.value * 100), 1f);
        float theta = Mathf.Deg2Rad * Mathf.Min(160f, amount);
        float sigma = 2f * Mathf.Tan(theta * 0.5f);

        Vector4 p0 = new Vector4(fisheye.center.value.x * 2f - 1f, fisheye.center.value.y * 2f - 1f,
                                 Mathf.Max(fisheye.xMultiplier.value, 1e-4f), Mathf.Max(fisheye.xMultiplier.value, 1e-4f));
        Vector4 p1 = new Vector4((fisheye.intensity.value >= 0f ? theta : 1f / theta), sigma,
                                  1f / fisheye.scale.value, fisheye.intensity.value * 100f);

        Vector2 half = Vector2.one / 2f;
        Vector2 center = new Vector2(p0.x, p0.y);

        uv = uv - half * p1.z + half;
        Vector2 ruv = new Vector2(p0.z, p0.w) * (uv - half - center);
        float ru = ruv.magnitude;

        if (p1.w > 0.0f) {
            float wu = ru * p1.x;
            ru = Mathf.Tan(wu) * (1.0f / (ru * sigma));
            uv = uv - ruv * (ru - 1.0f);
        }
        else {
            ru = (1.0f / ru) * p1.x * Mathf.Atan(ru * sigma);
            uv = uv - ruv * (ru - 1.0f);
        }

        return uv;
    }
}

public class ScaledRotation {
    private float prevRotation;
    private float scale;
    public float value { get; private set; }

    public ScaledRotation() {
        this.prevRotation = 0;
        this.scale = 1;
        this.value = 0;
    }

    public void rotateTo(float rotation) {
        if (rotation >= 0 && rotation < 90 && prevRotation >= 270 && prevRotation < 360) 
            scale = Mathf.Min(4, scale + (scale == -1 ? 2 : 1));

        if (rotation >= 270 && rotation < 360 && prevRotation >= 0 && prevRotation < 90) 
            scale = Mathf.Max(-4, scale - (scale == 1 ? 2 : 1));

        if (scale > 0)
            value = rotation + (360 * (scale - 1));
        else
            value = -(360 - rotation) + (360 * (scale + 1));

        prevRotation = rotation;
    }
}

public class ScaledDifference {
    private float _start;
    private float _end;
    private float loops;
    private float range;

    private float maxLoops;

    public float start {
        get { return _start; }
    }

    public float end {
        get { return _end; }
    }

    public float arc { 
        get { return loops * 360 + range; }
    }

    public ScaledDifference(float start, float end, float maxLoops) {
        this._start = start;
        this._end = end;
        this.maxLoops = maxLoops;

        this.loops = 0;
        this.range = calculateRange(start, end);
    }

    public void setStart(float newStart) {
        setRange(range - calculateRange(start, newStart) * Mathf.Sign(range));
        _start = newStart;
    }

    public void setEnd(float newEnd) {
        setRange(range + calculateRange(end, newEnd) * Mathf.Sign(range));
        _end = newEnd;
    }

    private void setRange(float newRange) {
        range = newRange;

        if (range >= 360) {
            range -= 360;
            loops += 1;
        } else if (range <= 360) {
            range += 360;
            loops -= 1;
        }
    }

    private float calculateRange(float start, float end) {
        float diff = end - start;
        float invDiff;

        if (diff <= 0)
            invDiff = 360 + diff;
        else 
            invDiff = diff - 360;

        if (Mathf.Abs(diff) < Mathf.Abs(invDiff)) 
            return diff;
        else
            return invDiff;
    }
}
