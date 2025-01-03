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
        public int gripChangeTimer;

        // Vehicle States
        public int playerIndex;
        public int spawnState;
        public bool isMoving;
        public bool isDrifting;

        // Vehicle Handling
        public int terrainType;
        public bool hasUnlimitedGrip;
        public int handicapAmount;

        // Vehicle Motion and Position
        public int heading;
        public int velocity;
        public int altitude;
        public int xVelocity;
        public int yVelocity;
        public int xForce;
        public int yForce;
        public int xVelocityForceDifference;
        public int yVelocityForceDifference;
        public int xyVelocityForceDifferenceMagnitude;
    }

    [System.Serializable]
    public struct RaceProperties
    {
        // Timers
        public int countdownTimer;
        public bool turnPollTimer;
        public int tankSlowTurnPollTimer;
        public int velocityPollTimer;

        // General Properties
        public int vehicleType;
        public bool hasUnlimitedGrip;

        // LUTs
        public byte[] VELOCITY_SCALAR_X_LUT;
        public byte[] VELOCITY_SCALAR_Y_LUT;
        public byte[] HANDICAP_LUT;
        public ushort[] DRIFT_THRESHOLD_LUT;
    }
}
