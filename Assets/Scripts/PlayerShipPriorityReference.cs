using System.Collections.Generic;

public class RequestablePropertyReference {
	private List<RequestClass> noOrder = new List<RequestClass>();
	private Dictionary<RequestClass, int> priority;
	private Dictionary<int, List<RequestClass>> order;

	public RequestablePropertyReference(Dictionary<RequestClass, int> priority, Dictionary<int, List<RequestClass>> order) {
		this.priority = priority;
		this.order = order;
	} 

	public int getPriority(RequestClass request) {
		return priority.ContainsKey(request) ? priority[request] : (int)RequestClass.NoRequest;
	}

	public IEnumerable<RequestClass> getOrder(int priority) {
		return order.ContainsKey(priority) ? order[priority] : noOrder;
	}
}

public interface ShipReference {
	public RequestablePropertyReference Acceleration { get; } 
	public RequestablePropertyReference MaxSpeed { get; }
	public RequestablePropertyReference Force { get; }
	public RequestablePropertyReference Magnitude { get; }
	public RequestablePropertyReference Rotation { get; }
}

public class PlayerShipPriorityReference : ShipReference {
	private RequestablePropertyReference _acceleration = new RequestablePropertyReference(
		new() {
			{ RequestClass.Boost, 0 },
			{ RequestClass.Brake, 1 },
			{ RequestClass.Move, 2 },
			{ RequestClass.BoostReset, 3 },
		},
		new()
	);

	private RequestablePropertyReference _maxSpeed = new RequestablePropertyReference(
		new() {
			{ RequestClass.Boost, 0 },
			{ RequestClass.Brake, 1 },
			{ RequestClass.BoostReset, 2 },
		},
		new()
	);

	private RequestablePropertyReference _force = new RequestablePropertyReference(
		new() {
			{ RequestClass.Move, 0 }, 
			{ RequestClass.Boost, 1 },
			{ RequestClass.Brake, 2 }, 
			{ RequestClass.BoostReset, 3 },
		},
		new()
	);

	private RequestablePropertyReference _magnitude = new RequestablePropertyReference(
		new() {
			{ RequestClass.Brake, 0 },
			{ RequestClass.Boost, 1 },
			{ RequestClass.BoostReset, 2 },
		},
		new()
	);

	private RequestablePropertyReference _rotation = new RequestablePropertyReference(
		new() {
			{ RequestClass.LookAtMouse, 0 }
		},
		new()
	);

	public RequestablePropertyReference Acceleration {
		get { return _acceleration; }
	}

	public RequestablePropertyReference MaxSpeed {
		get { return _maxSpeed; }
	}

	public RequestablePropertyReference Force {
		get { return _force; }
	}

	public RequestablePropertyReference Magnitude {
		get { return _magnitude; }
	}

	public RequestablePropertyReference Rotation {
		get { return _rotation; }
	}
}
