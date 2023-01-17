using System;

/*
* A Request is an object that can be used to manipulate the value of a Requestable
*
* The base Request class contains all fields necessary to identify a request
*
* system - Identifies the sender of the request, used to notify said sender in the case that their 
*       request was executed
* requestClass - Determines the priority of the request.  Can be passed to a PriorityReference to 
*       get an integer priority in return
* id - Uniquely identifies the Request.  The id of a request will be passed back to its sender to 
*       to notify it that the request was executed.
*
* Subclasses of Request will generaly add fields that specify how exactly they wish to modify a given
* propety
*/
namespace Request {
    public abstract class RequestBase {
        public RequestSender system;
        public RequestClass requestClass;
        public readonly Guid id;

        public RequestBase(RequestSender system, RequestClass requestClass) {
            this.system = system;
            this.requestClass = requestClass;
            this.id = Guid.NewGuid();
        }
    }

    /*
     * A request to set the value of a property.
     */
    public class SetRequest<T> : RequestBase {
        public T value;

        public SetRequest(RequestSender system, RequestClass requestClass, T value) 
                : base(system, requestClass) {
            this.value = value; 
        }
    }

    /*
     * A request to mutate the value of a property via the mutation function.
     */
    public class MutateRequest<T> : RequestBase {
        public Func<T, T> mutation;

        public MutateRequest(RequestSender system, RequestClass requestClass, Func<T, T> mutation) 
                : base(system, requestClass) {
            this.mutation = mutation; 
        }
    }

    /*
     * A request to raise the priority of a property without affecting its value.  Used to prevent other requests
     * from changing the given property.
     */
    public class BlockRequest : RequestBase {
        public BlockRequest(RequestSender system, RequestClass requestClass) 
                : base(system, requestClass) {}
    }
}
