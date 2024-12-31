using System;
using Unity.VisualScripting;
using UnityEngine;
using Vehicle.Shared;
using static Unity.Burst.Intrinsics.X86.Avx;
using static UnityEngine.InputSystem.Controls.AxisControl;

public class NESPhysics : VehiclePhysics
{
    public override void Turn(ref VehicleProperties vehicleProperties, ref RaceProperties raceProperties)
    {
        // Disable turning if countdown timer is not done
        if (raceProperties.countdownTimer > 0)
        {
            return;
        }

        // Alternate turning logic for tanks
        if (raceProperties.vehicleType == TANKS)
        {
            TurnTanks(ref vehicleProperties, ref raceProperties);
            return;
        }

        // Do not turn if not polled at the turn rate or in an undriveable state
        if (raceProperties.turnPollTimer == false || vehicleProperties.spawnState != ALIVE)
        {
            // Clamp heading when not turning
            if (!vehicleProperties.controllerLeft && !vehicleProperties.controllerRight)
            {
                vehicleProperties.heading = HEADING_CLAMP_LUT[vehicleProperties.heading];
            }
            return;
        }

        // Move forward if turning while not moving and not a chopper
        if (!vehicleProperties.isMoving && raceProperties.vehicleType != CHOPPERS) {
            vehicleProperties.velocity = 1;
        }

        // Turn in direction of controller input
        // Intentionally prioritizes left to match original behavior
        if (vehicleProperties.controllerLeft)
        {
            vehicleProperties.heading--;
        }
        else if (vehicleProperties.controllerRight)
        {
            vehicleProperties.heading++;
        }

        // Clamp direction between 0 and 63
        if (vehicleProperties.heading < 0)
        {
            vehicleProperties.heading = 63;
        }
        else if (vehicleProperties.heading > 63)
        {
            vehicleProperties.heading = 0;
        }
    }

    public void TurnTanks(ref VehicleProperties vehicleProperties, ref RaceProperties raceProperties)
    {
        // Do not turn if in an undriveable state
        if (vehicleProperties.spawnState != 0)
        {
            return;
        }

        // Clamp heading when not turning
        if (!vehicleProperties.controllerLeft && !vehicleProperties.controllerRight)
        {
            vehicleProperties.heading = HEADING_CLAMP_LUT[vehicleProperties.heading];
            return;
        }

        // Do not turn if not polled at the normal turn rate and not acc/decelerating
        if (raceProperties.turnPollTimer == false && !vehicleProperties.controllerAccelerate && !vehicleProperties.controllerBrake)
        {
            return;
        }

        // Do not turn if not polled at the tank turn rate and acc/decelerating
        if (raceProperties.tankSlowTurnPollTimer != 0 && (vehicleProperties.controllerAccelerate || vehicleProperties.controllerBrake))
        {
            return;
        }

        // Turn in direction of controller input
        // Intentionally prioritizes left to match original behavior
        if (vehicleProperties.controllerLeft)
        {
            vehicleProperties.heading--;
        }
        else if (vehicleProperties.controllerRight)
        {
            vehicleProperties.heading++;
        }

        // Clamp direction between 0 and 63
        if (vehicleProperties.heading < 0)
        {
            vehicleProperties.heading = 63;
        }
        else if (vehicleProperties.heading > 63)
        {
            vehicleProperties.heading = 0;
        }
    }

    public override void CalculateVelocityVector(ref VehicleProperties vehicleProperties, ref RaceProperties raceProperties)
    {
        // Do not add velocity if it's not the current player's turn to get calculated
        if (vehicleProperties.playerIndex != raceProperties.velocityPollTimer)
        {
            return;
        }

        // Calculate the reverse velocity vector
        int speed;
        int xScalar;
        int yScalar;
        if (vehicleProperties.velocity < 0)
        {
            speed = Math.Abs(vehicleProperties.velocity);
            xScalar = -raceProperties.VELOCITY_SCALAR_X_LUT[vehicleProperties.heading];
            speed = ApplyVelocityScalar(ref vehicleProperties, ref raceProperties, xScalar, speed);
        }
        else
        {
            speed = vehicleProperties.velocity;
        }
    }

    //TODO: finish
    public int ApplyVelocityScalar(ref VehicleProperties vehicleProperties, ref RaceProperties raceProperties, int scalar, int speed)
    {
        return speed;
    }
}
