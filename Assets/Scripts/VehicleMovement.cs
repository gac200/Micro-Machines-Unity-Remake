using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.UI;
using Vehicle.Shared;
using Unity.VisualScripting;

public class VehicleMovement: MonoBehaviour {
    private Controls controls;
    enum GameVersion
    {
        NES_NTSC,
        NES_PAL,
        MD_NTSC,
        MD_PAL,
        Amiga,
        MS,
        GG,
        SNES_NTSC,
        SNES_PAL,
        DOS,
        GB,
        CDi,
        GBC
    }

    [SerializeField]
    GameVersion gameVersion = new GameVersion();

    public VehiclePhysics vehiclePhysics;
    public VehicleProperties vehicleProperties;
    public RaceProperties raceProperties;
    private void Awake()
    {
        controls = new Controls();
        SelectGamePhysics(gameVersion.ToString());
        LoadVehicleProperties(raceProperties.vehicleType);
        raceProperties.tankSlowTurnPollTimer = VehiclePhysics.TANK_SLOW_TURN_POLL_RATE;
        raceProperties.velocityPollTimer = VehiclePhysics.VELOCITY_POLL_RATE;
        vehicleProperties.handicapAmount = 0x10;
    }
    private void OnEnable()
    {
        controls.Enable();
    }
    private void OnDisable()
    {
        controls.Disable();
    }

    void FixedUpdate()
    {
        vehiclePhysics.Turn(ref vehicleProperties, ref raceProperties);
        vehicleProperties.isDrifting = false;
        // TODO: find constant for terrain type 14
        if (vehicleProperties.altitude == 0 && vehicleProperties.terrainType != 14)
        {
            // TODO: implement
            vehiclePhysics.CalculateVelocityScalars(ref vehicleProperties, ref raceProperties);
            vehiclePhysics.CalculateVelocityEffects(ref vehicleProperties, ref raceProperties); ;
            if (raceProperties.vehicleType != VehiclePhysics.POWERBOATS)
            {
                //vehiclePhysics.$9543();
            }
        }
        //vehiclePhysics.$8265
        //vehiclePhysics.$EA96
        //vehiclePhysics.$DD5B
        // TODO: temporary until proper sprites are used
        transform.eulerAngles = new Vector3(0, 0, -(vehicleProperties.heading / 63f) * 354.375f);
        // TODO: temporary until poll timers are really figured out
        if (raceProperties.turnPollTimer == true)
        {
            raceProperties.turnPollTimer = false;
        }
        else
        {
            raceProperties.turnPollTimer = true;
        }
        if (raceProperties.tankSlowTurnPollTimer > 0)
        {
            raceProperties.tankSlowTurnPollTimer--;
        }
        else
        {
            raceProperties.tankSlowTurnPollTimer = VehiclePhysics.TANK_SLOW_TURN_POLL_RATE;
        }
        if (raceProperties.velocityPollTimer > 0)
        {
            raceProperties.velocityPollTimer--;
        }
        else
        {
            raceProperties.tankSlowTurnPollTimer = VehiclePhysics.VELOCITY_POLL_RATE;

        }
    }

    void Update()
    {
        vehicleProperties.controllerLeft = controls.Vehicle.Left.IsPressed();
        vehicleProperties.controllerRight = controls.Vehicle.Right.IsPressed();
        vehicleProperties.controllerAccelerate = controls.Vehicle.Accelerate.IsPressed(); ;
        vehicleProperties.controllerBrake = controls.Vehicle.Brake.IsPressed();
    }

    void SelectGamePhysics(string gameVersion)
    {
        switch (gameVersion)
        {
            case "NES_NTSC":
                // Updates once per NES frame
                Time.fixedDeltaTime = 1 / 60.0988f;
                vehiclePhysics = new NESPhysics();
                break;
            case "NES_PAL":
                // Updates once per NES frame
                Time.fixedDeltaTime = 1 / 50.0070f;
                vehiclePhysics = new NESPhysics();
                break;
            default:
                break;
        }
    }
    void LoadVehicleProperties(int vehicleType)
    {
        switch (vehicleType)
        {
            case VehiclePhysics.POWERBOATS:
                raceProperties.VELOCITY_SCALAR_X_LUT = VehiclePhysics.POWERBOATS_VELOCITY_SCALAR_X_LUT;
                raceProperties.VELOCITY_SCALAR_Y_LUT = VehiclePhysics.POWERBOATS_VELOCITY_SCALAR_Y_LUT;
                raceProperties.HANDICAP_LUT = VehiclePhysics.POWERBOATS_HANDICAP_LUT;
                raceProperties.DRIFT_THRESHOLD_LUT = VehiclePhysics.POWERBOATS_DRIFT_THRESHOLD_LUT;
                break;
            default:
                break;
        }
    }
}
