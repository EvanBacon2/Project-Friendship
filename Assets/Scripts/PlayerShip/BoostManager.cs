using UnityEngine;

public class BoostManager : MonoBehaviour {
    public int maxBoostLevel;

    public float boostCooldown;//Minimum time between boosts
    public float boostTime;//Time length of a single boost

    public float boostAccelerationMod;//Amount acceleration is multiplied by per boostLevel
    public int boostMaxSpeedMod;//Amount added to maxSpeed per boostLevel

    public float coastTime;//Time spent coasting before boostLevel is reset, coasting is a state where boostLevel > 0 and the provided rigidbody is not accelerating
}
