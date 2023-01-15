using System.Collections.Generic;
using UnityEngine;

/*
 * A collection of RequestableProperties
 */
/*public abstract class RequestableModel : RequestManager {
    private List<RequestManager> properties;

    /*
     * Adds a property to the model.  Returns the property as a RequestPort.  Use this port
     * to expose the property and allow it to take requests.
     *
    protected RequestPort<T> addProperty<T>(RequestableProperty<T> property) {
        properties.Add(property);

        return (RequestPort<T>)property;
    } 

    /*
     * Executes the priority requests for all properties
     *
    public void executeRequests() {
        foreach(RequestManager property in properties) {
            property.executeRequests();
        }
    }

    /*
     * Changes the models RequestReference, and updates the model's properties appropriately
     *
    public void setReference(RequestablePropertyReference reference) {
        foreach(RequestReferenceManager manager in referenceManagers) {
            manager.setReference(newReference);
        }
    }
}*/

public class ShipModel {
    private RequestableProperty<float> _acceleration;
    private RequestableProperty<float> _maxSpeed;
    private RequestableProperty<(Vector3, ForceMode)> _force;
    private RequestableProperty<float> _magnitude;
    private RequestableProperty<(float, float)> _rotation;

    public RequestPort<float> Acceleration { 
        get { return _acceleration; }
    }

    public RequestPort<float> MaxSpeed { 
        get { return _maxSpeed; }
    }
    public RequestPort<(Vector3, ForceMode)> Force {
        get { return _force; }
    }
    public RequestPort<float> Magnitude {
        get { return _magnitude; }
    }
    public RequestPort<(float, float)> Rotation {
        get { return _rotation; }
    }

    public ShipModel(ShipReference reference, float acceleration = 0, float maxSpeed = 0, float rotation = 0) {
        _acceleration = new RequestableProperty<float>(acceleration, reference.Acceleration);
        _maxSpeed = new RequestableProperty<float>(maxSpeed, reference.MaxSpeed);
        _force = new RequestableProperty<(Vector3, ForceMode)>((Vector3.zero, ForceMode.Force), reference.Force);
        _magnitude = new RequestableProperty<float>(0, reference.Magnitude);
        _rotation = new RequestableProperty<(float, float)>((rotation, 0), reference.Rotation);
    }

    public void executeRequests() {
        _acceleration.executeRequests();
        _maxSpeed.executeRequests();
        _force.executeRequests();
        _magnitude.executeRequests();
        _rotation.executeRequests();
    }

    public void setReference(ShipReference reference) {
        _acceleration.setReference(reference.Acceleration);
        _maxSpeed.setReference(reference.MaxSpeed);
        _force.setReference(reference.Force);
        _magnitude.setReference(reference.Magnitude);
        _rotation.setReference(reference.Rotation);
    }
}
