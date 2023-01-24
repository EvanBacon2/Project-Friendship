public interface ShipReference {
	public IRequestReference Acceleration { get; } 
	public IRequestReference MaxSpeed { get; }
	public IRequestReference Force { get; }
	public IRequestReference Magnitude { get; }
	public IRequestReference Rotation { get; }
}

public class PlayerShipRequestReference : ShipReference {
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
}
