using System.Collections.Generic;

namespace Vehicle.Shared
{
    [System.Serializable]
    public struct VehicleProperties
    {
        // Controller Properties
        public bool controllerLeft;
        public bool controllerRight;
        public bool controllerAccelerate;
        public bool controllerBrake;

        // Timers
        public sbyte gripChangeTimer;
        public sbyte driftSoundTimer;

        // AI Properties
        public bool isAIControlledSpeed;

        // Vehicle States
        public sbyte playerIndex;
        public sbyte spawnState;
        public bool isMoving;
        public bool isDrifting;

        // Vehicle Handling
        public sbyte terrainType;
        public bool hasUnlimitedGrip;
        public sbyte handicapAmount;

        // Vehicle Motion and Position
        public int xPosition;
        public int yPosition;
        public sbyte zPosition;
        public sbyte heading;
        public sbyte velocity;
        public short xVelocity;
        public short yVelocity;
        public short xForce;
        public short yForce;
        public sbyte zForce;
        public short xVelocityForceDifference;
        public short yVelocityForceDifference;
        public short xyVelocityForceDifferenceMagnitude;

        // Other Properties
        public byte bounceBehavior;
        public sbyte checkpointIndex;

        //TODO: implement
        public int lastXPosition;
        public int lastYPosition;

        // Sound
        // not like original, but allows multiple sfx at once
        public List<byte> sfx;
    }

    [System.Serializable]
    public struct RaceProperties
    {
        // Timers
        public sbyte countdownTimer;
        public sbyte turnPollTimer;
        public sbyte tankSlowTurnPollTimer;
        public sbyte velocityPollTimer;
        public sbyte driftSpeedLossTimer;
        public sbyte changeZForceTimer1;
        public sbyte changeZForceTimer2;

        // General Properties
        public sbyte vehicleType;
        public bool hasUnlimitedGrip;
        public bool isChallengeMode;

        // LUTs
        public sbyte[] VELOCITY_SCALAR_X_LUT;
        public sbyte[] VELOCITY_SCALAR_Y_LUT;
        public byte[] HANDICAP_LUT;
        public ushort[] DRIFT_THRESHOLD_LUT;
        public byte[] DRIFT_FORCE_AMOUNT_LUT;
    }
}
