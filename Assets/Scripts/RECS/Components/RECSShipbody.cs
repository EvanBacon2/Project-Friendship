using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RECSShipbody : RECSRigidbody {
    protected new PlayerShipReference reference;

    public float linearMax;
    public float linearAcceleration;
    public float angularMax;
    public float angularAcceleration;

    private float _baseLinearMax;
    private float _baseLinearAcceleration;
    private float _baseAngularMax;
    private float _baseAngularAcceleration;
    
    public float BASE_LINEAR_MAX { get { return _baseLinearMax; } }
    public float BASE_LINEAR_ACCELERATION { get { return _baseLinearAcceleration; } }
    public float BASE_ANGULAR_MAX { get { return _baseAngularMax; } }
    public float BASE_ANGULAR_ACCELERATION { get { return _baseAngularAcceleration; } }

    private ManagedRequestableValue<float> _linearMax;
    private ManagedRequestableValue<float> _linearAcceleration;
    private ManagedRequestableValue<float> _angularMax;
    private ManagedRequestableValue<float> _angularAcceleration;
    
    public IManagedRequestPort<float> LinearMax { 
        get { return _linearMax; }
    }

    public IManagedRequestPort<float> LinearAcceleration { 
        get { return _linearAcceleration; }
    }

    public IManagedRequestPort<float> AngularMax {
        get { return _angularMax; }
    }

    public IManagedRequestPort<float> AngularAcceleration {
        get { return _angularAcceleration; }
    }

    protected override void start() {
        this.reference = new PlayerShipPriorityReference();
        base.reference = reference; 

        this._baseLinearMax = linearMax;
        this._baseLinearAcceleration = linearAcceleration;
        this._baseAngularMax = angularMax;
        this._baseAngularAcceleration = angularAcceleration;

        PoolManager<float> LinearMaxPool = new();    
        _linearMax = new ManagedRequestableValue<float>(
            linearMax, 
            reference.LinearMax, 
            new IncreasingPriority(() => { LinearMaxPool.reset(); }), 
            LinearMaxPool
        );

        PoolManager<float> linearAccelerationPool = new();
        _linearAcceleration = new ManagedRequestableValue<float>(
            linearAcceleration, 
            reference.LinearAcceleration, 
            new IncreasingPriority(() => { linearAccelerationPool.reset(); }), 
            linearAccelerationPool
        );

        PoolManager<float> angularMaxPool = new();
        _angularMax = new ManagedRequestableValue<float>(
            angularMax,
            reference.AngularMax,
            new IncreasingPriority(() => { angularMaxPool.reset(); }),
            angularMaxPool
        );

        PoolManager<float> angularAccelerationPool = new();
        _angularAcceleration = new ManagedRequestableValue<float>(
            angularAcceleration, 
            reference.AngularAcceleration, 
            new IncreasingPriority(() => { angularAccelerationPool.reset(); }), 
            angularAccelerationPool
        );
    }

    protected override void _executeRequests() {
        base._executeRequests();

        _linearMax.executeRequests();
        _linearAcceleration.executeRequests();
        _angularMax.executeRequests();
        _angularAcceleration.executeRequests();
    }

    public void setReference(PlayerShipReference reference){
        base.setReference(reference);
        this.reference = reference;
        
        _linearMax.setReference(reference.LinearMax);
        _linearAcceleration.setReference(reference.LinearAcceleration);
        _angularMax.setReference(reference.AngularMax);
        _angularAcceleration.setReference(reference.AngularAcceleration);
    }

    protected override void _notifySenders(){
        base._notifySenders();

        _linearMax.addSendersTo(senders);
        _linearAcceleration.addSendersTo(senders);
        _angularMax.addSendersTo(senders);
        _angularAcceleration.addSendersTo(senders);
    }

    private Vector2 sGiz1 = new Vector2();
    private Vector2 sGiz2 = new Vector2();

    /*private void OnDrawGizmos() {
        if (!Application.isPlaying) 
			return;

        Gizmos.color = Color.cyan;

        sGiz1.x = Position.value.x;
        sGiz1.y = Position.value.y;
        sGiz2.x = Position.value.x + Mathf.Cos((Rotation.value.eulerAngles.z + 90) * Mathf.Deg2Rad) * 15;
        sGiz2.y = Position.value.y + Mathf.Sin((Rotation.value.eulerAngles.z + 90) * Mathf.Deg2Rad) * 15;

        Gizmos.DrawLine(sGiz1, sGiz2);
    }*/
}
