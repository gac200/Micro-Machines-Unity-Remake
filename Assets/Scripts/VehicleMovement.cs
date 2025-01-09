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

    [SerializeField] GameVersion gameVersion = new GameVersion();

    public VehiclePhysics vehiclePhysics;
    public VehicleProperties vehicleProperties;
    public RaceProperties raceProperties;

    [SerializeField] Sprite[] vehicleSprites;

    private SpriteRenderer spriteRenderer;
    private void Awake()
    {
        controls = new Controls();
        SelectGamePhysics(gameVersion.ToString());
        LoadVehicleProperties(raceProperties.vehicleType);
        raceProperties.turnPollTimer = VehiclePhysics.NORMAL_TURN_POLL_RATE;
        raceProperties.tankSlowTurnPollTimer = VehiclePhysics.TANK_SLOW_TURN_POLL_RATE;
        raceProperties.velocityPollTimer = VehiclePhysics.VELOCITY_POLL_RATE;
        raceProperties.driftSpeedLossTimer = VehiclePhysics.DRIFT_SPEED_LOSS_RATE;
        raceProperties.changeZForceTimer1 = VehiclePhysics.CHANGE_Z_FORCE_TIMER_1_RATE;
        raceProperties.changeZForceTimer2 = VehiclePhysics.CHANGE_Z_FORCE_TIMER_2_RATE;
        vehicleProperties.handicapAmount = 0x10;
        vehicleProperties.xPosition = (int)transform.position.x * 256;
        vehicleProperties.yPosition = (int)-transform.position.y * 256;
        spriteRenderer = GetComponent<SpriteRenderer>();
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
        if (vehicleProperties.zPosition == 0 && vehicleProperties.terrainType != 14)
        {
            vehiclePhysics.CalculateVelocityScalars(ref vehicleProperties, ref raceProperties);
            //vehiclePhysics.CalculateVelocityEffects(ref vehicleProperties, ref raceProperties); ;
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

        vehicleProperties.xPosition += vehicleProperties.xForce;
        vehicleProperties.yPosition += vehicleProperties.yForce;
        // TODO: finish
        vehiclePhysics.CalculateVerticalForces(ref vehicleProperties, ref raceProperties);

        // TODO: implement
        //vehiclePhysics.$EA96
        //vehiclePhysics.$DD5B
        if (vehicleProperties.heading >= 0 && vehicleProperties.heading < 16)
        {
            spriteRenderer.flipX = false;
            spriteRenderer.flipY = false;
            spriteRenderer.sprite = vehicleSprites[vehicleProperties.heading / 2];
        }
        else if (vehicleProperties.heading >= 16 && vehicleProperties.heading < 32)
        {
            spriteRenderer.flipX = false;
            spriteRenderer.flipY = true;
            spriteRenderer.sprite = vehicleSprites[vehicleSprites.Length - 1 - (vehicleProperties.heading / 2 - 8)];
        }
        else if (vehicleProperties.heading >= 32 && vehicleProperties.heading < 48)
        {
            spriteRenderer.flipX = true;
            spriteRenderer.flipY = true;
            spriteRenderer.sprite = vehicleSprites[vehicleProperties.heading / 2 - 16];
        }
        else
        {
            spriteRenderer.flipX = true;
            spriteRenderer.flipY = false;
            spriteRenderer.sprite = vehicleSprites[vehicleSprites.Length - 1 - (vehicleProperties.heading / 2 - 24)];
        }
        // Enhanced rotations
        if (vehicleProperties.heading % 2 == 0)
        {
            transform.eulerAngles = Vector3.zero;
        }
        else
        {
            transform.eulerAngles = new Vector3(0f, 0f, -5.625f);
        }
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
            raceProperties.velocityPollTimer = VehiclePhysics.VELOCITY_POLL_RATE;
        }
        if (raceProperties.driftSpeedLossTimer > 0)
        {
            raceProperties.driftSpeedLossTimer--;
        }
        else
        {
            raceProperties.driftSpeedLossTimer = VehiclePhysics.DRIFT_SPEED_LOSS_RATE;
        }
        if (raceProperties.changeZForceTimer1 > 0)
        {
            raceProperties.changeZForceTimer1--;
        }
        else
        {
            raceProperties.changeZForceTimer1 = VehiclePhysics.CHANGE_Z_FORCE_TIMER_1_RATE;
        }
        if (raceProperties.changeZForceTimer2 > 0)
        {
            raceProperties.changeZForceTimer2--;
        }
        else
        {
            raceProperties.changeZForceTimer2 = VehiclePhysics.CHANGE_Z_FORCE_TIMER_2_RATE;
        }
        Vector3 newPosition = new Vector3(vehicleProperties.xPosition / 256f, -vehicleProperties.yPosition / 256f, vehicleProperties.zPosition);
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
