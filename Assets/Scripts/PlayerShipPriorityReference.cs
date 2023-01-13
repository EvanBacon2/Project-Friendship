using System.Collections.Generic;

public class PlayerShipPriorityReference : PriorityReference {
	private readonly Dictionary<RequestClass, int> accelerationPriorities = new()
	{
		{ RequestClass.Boost, 0 },
		{ RequestClass.Brake, 1 },
		{ RequestClass.Move, 2 },
		{ RequestClass.BoostReset, 3 },
	};
	private readonly Dictionary<int, List<RequestClass>> accelerationOrder = new()
	{
	};

	private readonly Dictionary<RequestClass, int> maxSpeedPriorities = new()
	{
		{ RequestClass.Boost, 0 },
		{ RequestClass.Brake, 1 },
		{ RequestClass.BoostReset, 2 },
	};
	private readonly Dictionary<int, List<RequestClass>> maxSpeedOrder = new() 
	{
	};

	private readonly Dictionary<RequestClass, int> forcePriorities = new()
	{
		{ RequestClass.Move, 0 }, //low priority
		{ RequestClass.Boost, 1 },
		{ RequestClass.Brake, 2 }, //high priority
		{ RequestClass.BoostReset, 3 },
	};
	private readonly Dictionary<int, List<RequestClass>> forceOrder = new() 
	{
	};

	private readonly Dictionary<RequestClass, int> magnitudePriorities = new()
	{
		{ RequestClass.Brake, 0 },
		{ RequestClass.Boost, 1 },
		{ RequestClass.BoostReset, 2 },
	};
	private readonly Dictionary<int, List<RequestClass>> magnitudeOrder = new() 
	{
	};

	private readonly Dictionary<RequestClass, int> rotationPriorities = new()
	{
		{ RequestClass.LookAtMouse, 0 }
	};
	private readonly Dictionary<int, List<RequestClass>> rotationOrder = new() 
	{
	};

	private readonly Dictionary<string, Dictionary<RequestClass, int>> _propertyMap;

	private readonly Dictionary<string, Dictionary<int, List<RequestClass>>> _orderMap;

	protected override Dictionary<string, Dictionary<RequestClass, int>> propertyMap {
        get { return _propertyMap; }
    }

    protected override Dictionary<string, Dictionary<int, List<RequestClass>>> orderMap {
        get { return _orderMap; }
    }

    private static PlayerShipPriorityReference _instance;
    public static PlayerShipPriorityReference instance {
        get { return _instance == null ? new() : _instance; }
    }

	private PlayerShipPriorityReference() {
        _propertyMap = new()
	    {
            { PlayerShipProperties.Acceleration, accelerationPriorities },
            { PlayerShipProperties.MaxSpeed, maxSpeedPriorities },
            { PlayerShipProperties.Force, forcePriorities },
            { PlayerShipProperties.Magnitude, magnitudePriorities },
            { PlayerShipProperties.Rotation, rotationPriorities }
	    };

        _orderMap = new()
	    {
            {PlayerShipProperties.Acceleration, accelerationOrder},
            {PlayerShipProperties.MaxSpeed, maxSpeedOrder},
            {PlayerShipProperties.Force, forceOrder},
            {PlayerShipProperties.Magnitude, magnitudeOrder},
            {PlayerShipProperties.Rotation, rotationOrder},
	    };

		foreach (KeyValuePair<string, Dictionary<RequestClass, int>> entry in propertyMap) {
			foreach (int priority in entry.Value.Values) {
				if (!orderMap[entry.Key].ContainsKey(priority)) {
					orderMap[entry.Key][priority] = new List<RequestClass>();
				}
			}
		}
	}
}
