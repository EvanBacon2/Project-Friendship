using System;
/*
* A Request is an object that can be used to manipulate the property of a RequestableModel
*
* The base Request class contains all fields necessary to identify a request
*
* system - Identifies the sender of the request, used to notify said sender in the case that their 
*       request was executed
* requestClass - Determines the priority of the request.  Can be passed to a PriorityReference to 
*       get an integer priority in return
* property - The name of the property this request wants to affect.
* id - Uniquely identifies the Request.  The id of a request will be passed back to its sender to 
*       to notify it that the request was executed.
*
* Subclasses of Request will generaly add fields that specify how exactly they wish to modify a given
* propety
*/
namespace Request {
    public abstract class Request {
        public RequestSystem system;
        public RequestClass requestClass;
        public readonly Guid id;

        public Request(RequestSystem system, RequestClass requestClass) {
            this.system = system;
            this.requestClass = requestClass;
            this.id = Guid.NewGuid();
        }
    }

    /*
     * A request to set the value of a property.
     */
    public class SetRequest<T> : Request {
        public T value;

        public SetRequest(RequestSystem system, RequestClass requestClass, T value) : base(system, requestClass) {
            this.value = value; 
        }
    }

    /*
     * A request to mutate the value of a property via the mutation function.
     */
    public class MutateRequest<T> : Request {
        public Func<T, T> mutation;

        public MutateRequest(RequestSystem system, RequestClass requestClass, Func<T, T> mutation) : base(system, requestClass) {
            this.mutation = mutation; 
        }
    }

    /*
     * A request to raise the priority of a property without affecting its value.  Used to prevent other requests
     * from changing the given property.
     */
    public class BlockRequest : Request {
        public BlockRequest(RequestSystem system, RequestClass requestClass) : base(system, requestClass) {}
    }
}
