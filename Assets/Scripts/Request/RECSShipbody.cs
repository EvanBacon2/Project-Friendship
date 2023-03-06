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

    private ManagedAnyRequestableValue<float> _linearMax;
    private ManagedAnyRequestableValue<float> _linearAcceleration;
    private ManagedAnyRequestableValue<float> _angularMax;
    private ManagedAnyRequestableValue<float> _angularAcceleration;
    
    public IManagedAnyRequest<float> LinearMax { 
        get { return _linearMax; }
    }

    public IManagedAnyRequest<float> LinearAcceleration { 
        get { return _linearAcceleration; }
    }

    public IManagedAnyRequest<float> AngularMax {
        get { return _angularMax; }
    }

    public IManagedAnyRequest<float> AngularAcceleration {
        get { return _angularAcceleration; }
    }

    protected override void start() {
        this.reference = new PlayerShipRequestReference();
        base.reference = reference; 

        this._baseLinearMax = linearMax;
        this._baseLinearAcceleration = linearAcceleration;
        this._baseAngularMax = angularMax;
        this._baseAngularAcceleration = angularAcceleration;

        AnyRequestPool<float> LinearMaxPool = new();    
        _linearMax = new ManagedAnyRequestableValue<float>(
            linearMax, 
            reference.LinearMax, 
            new IncreasingPriority(() => { LinearMaxPool.reset(); }), 
            LinearMaxPool
        );

        AnyRequestPool<float> linearAccelerationPool = new();
        _linearAcceleration = new ManagedAnyRequestableValue<float>(
            linearAcceleration, 
            reference.LinearAcceleration, 
            new IncreasingPriority(() => { linearAccelerationPool.reset(); }), 
            linearAccelerationPool
        );

        AnyRequestPool<float> angularMaxPool = new();
        _angularMax = new ManagedAnyRequestableValue<float>(
            angularMax,
            reference.AngularMax,
            new IncreasingPriority(() => { angularMaxPool.reset(); }),
            angularMaxPool
        );

        AnyRequestPool<float> angularAccelerationPool = new();
        _angularAcceleration = new ManagedAnyRequestableValue<float>(
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

        if (rb.velocity.magnitude > LinearMax.value)
            rb.velocity = rb.velocity.normalized * LinearMax.value;
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

        _linearMax.notifySenders();
        _linearAcceleration.notifySenders();
        _angularMax.executeRequests();
        _angularAcceleration.executeRequests();
    }
}
