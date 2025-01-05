using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System;
using UnityEngine.UI;
using Vehicle.Shared;
using Unity.VisualScripting;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

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
        vehicleProperties.xPosition = (int)transform.position.x * 256;
        vehicleProperties.yPosition = (int)-transform.position.y * 256;
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
            vehiclePhysics.CalculateVelocityScalars(ref vehicleProperties, ref raceProperties);
            vehiclePhysics.CalculateVelocityEffects(ref vehicleProperties, ref raceProperties); ;
            if (raceProperties.vehicleType == VehiclePhysics.POWERBOATS)
            {
                // TODO: implement
                //vehiclePhysics.$9543();
            }
        }
        if (vehicleProperties.xForce != 0 || vehicleProperties.yForce != 0)
        {
            vehicleProperties.isMoving = true;
        }

        //DCDC  BD E4 03       LDA $03E4,X
        //DCDF  9D FC 03       STA $03FC,X
        //DCE2  18             CLC
        //DCE3  7D 5C 03       ADC xForceLo, X
        //DCE6  9D E4 03       STA $03E4,X
        //DCE9  BD DC 03       LDA xPositionLo, X
        //DCEC  9D F4 03       STA $03F4,X
        //DCEF  7D 58 03       ADC xForceHi, X
        //DCF2  9D DC 03       STA xPositionLo, X
        //DCF5  BD E0 03       LDA xPositionHi, X
        //DCF8  9D F8 03       STA $03F8,X


        vehicleProperties.xPosition += vehicleProperties.xForce;
        vehicleProperties.yPosition += vehicleProperties.yForce;

        // TODO: implement
        //vehiclePhysics.$8265
        //vehiclePhysics.$EA96
        //vehiclePhysics.$DD5B
        // TODO: temporary until proper sprites are used
        transform.eulerAngles = new Vector3(0, 0, -(vehicleProperties.heading / 63f) * 354.375f);
        // TODO: temporary until poll timers are really figured out
        if (raceProperties.turnPollTimer > 0)
        {
            raceProperties.turnPollTimer--;
        }
        else
        {
            raceProperties.turnPollTimer = VehiclePhysics.NORMAL_TURN_POLL_RATE;
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
        if (raceProperties.driftSpeedLossTimer > 0)
        {
            raceProperties.driftSpeedLossTimer--;
        }
        else
        {
            raceProperties.driftSpeedLossTimer = VehiclePhysics.DRIFT_SPEED_LOSS_RATE;
        }
        Vector3 newPosition = new Vector3(vehicleProperties.xPosition / 256f, -vehicleProperties.yPosition / 256f, vehicleProperties.altitude);
        transform.position = newPosition;
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
                raceProperties.VELOCITY_SCALAR_X_LUT = Array.ConvertAll(VehiclePhysics.POWERBOATS_VELOCITY_SCALAR_X_LUT, b => unchecked((sbyte)b));
                raceProperties.VELOCITY_SCALAR_Y_LUT = Array.ConvertAll(VehiclePhysics.POWERBOATS_VELOCITY_SCALAR_Y_LUT, b => unchecked((sbyte)b));
                raceProperties.HANDICAP_LUT = VehiclePhysics.POWERBOATS_HANDICAP_LUT;
                raceProperties.DRIFT_THRESHOLD_LUT = VehiclePhysics.POWERBOATS_DRIFT_THRESHOLD_LUT;
                raceProperties.DRIFT_FORCE_AMOUNT_LUT = VehiclePhysics.POWERBOATS_DRIFT_FORCE_AMOUNT_LUT;
                break;
            default:
                break;
        }
    }
}
