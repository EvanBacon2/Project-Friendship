using System.Collections;
using System.Collections.Generic;

public static class Request {
	public const int NoRequest = -1;
	public const int Move = 0;
	public const int Brake = 1;
	public const int Boost = 2;
	public const int LookAtMouse = 3;
}
public static class PlayerShipRequestPriority {
	private static readonly Dictionary<int, int> accelerationPriorities = new Dictionary<int, int>() 
	{
		{ Request.Boost, 0 },
		{ Request.Brake, 1 },
	};
	private static readonly Dictionary<int, int> maxSpeedPriorities = new Dictionary<int, int>()
	{
		{ Request.Boost, 0 },
		{ Request.Brake, 1 },
	};
	private static readonly Dictionary<int, int> forcePriorities = new Dictionary<int, int>()
	{
		{ Request.Move, 0 },
		{ Request.Boost, 1 },
		{ Request.Brake, 2 },
	};
	private static readonly Dictionary<int, int> directionPriorities = new Dictionary<int, int>()
	{
		{ Request.Boost, 0 },
		{ Request.Brake, 1 },
	};
	private static readonly Dictionary<int, int> magnitudePriorities = new Dictionary<int, int>()
	{
		{ Request.Boost, 0 },
		{ Request.Brake, 1 },
	};
	private static readonly Dictionary<int, int> rotationPriorities = new Dictionary<int, int>()
	{
		{ Request.LookAtMouse, 0 }
	};

	public static int accelerationPriority(int request) {
		return accelerationPriorities.ContainsKey(request) ? accelerationPriorities[request] : -1;
	}

	public static int maxSpeedPriority(int request) {
		return maxSpeedPriorities.ContainsKey(request) ? maxSpeedPriorities[request] : -1;
	}

	public static int forcePriority(int request) {
		return forcePriorities.ContainsKey(request) ? forcePriorities[request] : -1;
	}

	public static int directionPriority(int request) {
		return directionPriorities.ContainsKey(request) ? directionPriorities[request] : -1;
	}

	public static int magnitudePriority(int request) {
		return magnitudePriorities.ContainsKey(request) ? magnitudePriorities[request] : -1;
	}

	public static int rotationPriority(int request) {
		return rotationPriorities.ContainsKey(request) ? rotationPriorities[request] : -1;
	}
}
