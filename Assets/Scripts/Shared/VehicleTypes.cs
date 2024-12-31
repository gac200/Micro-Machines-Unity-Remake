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

        // Vehicle States
        public int playerIndex;
        public int spawnState;
        public bool isMoving;
        public bool canReleaseDriftParticles;

        // Vehicle Handling
        public int terrainType;

        // Vehicle Motion and Position
        public int heading;
        public int velocity;
        public int altitude;
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

        // LUTs
        public byte[] VELOCITY_SCALAR_X_LUT;
        public byte[] VELOCITY_SCALAR_Y_LUT;
    }
}
