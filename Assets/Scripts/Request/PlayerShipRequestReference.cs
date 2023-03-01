using UnityEngine;

public interface RigidbodyReference {
	public IRequestReference Velocity { get; }
	public IRequestReference Acceleration { get; } 
	public IRequestReference MaxSpeed { get; }
	public IRequestReference Force { get; }
	public IRequestReference Magnitude { get; }
	public IRequestReference Rotation { get; }
	public IRequestReference Position { get; } 
}

public class PlayerShipRequestReference : RigidbodyReference {
	private IRequestReference _velocity = new RequestReferenceMap(
		new() {
			{ RequestClass.Rope, 0 },
		},
		new()
	);

	private IRequestReference _acceleration = new RequestReferenceMap(
		new() {
			{ RequestClass.Boost, 0 },
			{ RequestClass.Brake, 1 },
			{ RequestClass.BoostReset, 3 },
		},
		new()
	);

	private IRequestReference _maxSpeed = new RequestReferenceMap(
		new() {
			{ RequestClass.Boost, 0 },
			{ RequestClass.Brake, 1 },
			{ RequestClass.BoostReset, 2 },
		},
		new()
	);

	private IRequestReference _force = new RequestReferenceMap(
		new() {
			{ RequestClass.Move, 0 }, 
			{ RequestClass.Boost, 1 },
			{ RequestClass.Brake, 2 }, 
			{ RequestClass.BoostReset, 3 },
			{ RequestClass.Rope, 4 }
		},
		new()
	);

	private IRequestReference _magnitude = new RequestReferenceMap(
		new() {
			{ RequestClass.Brake, 0 },
			{ RequestClass.Boost, 1 },
			{ RequestClass.BoostReset, 2 },
		},
		new()
	);

	private IRequestReference _rotation = new RequestReferenceMap(
		new() {
			{ RequestClass.LookAtMouse, 0 }
		},
		new()
	);

	private IRequestReference _position = new RequestReferenceMap(
		new() {
			{ RequestClass.Rope, 0 },
		},
		new()
	);

	public IRequestReference Velocity {
		get { return _velocity; }
	}

	public IRequestReference Acceleration {
		get { return _acceleration; }
	}

	public IRequestReference MaxSpeed {
		get { return _maxSpeed; }
	}

	public IRequestReference Force {
		get { return _force; }
	}

	public IRequestReference Magnitude {
		get { return _magnitude; }
	}

	public IRequestReference Rotation {
		get { return _rotation; }
	}

	public IRequestReference Position {
		get { return _position; }
	}
}
