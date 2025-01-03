using System;
using System.Net;
using System.Reflection.Emit;
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

    public override void CalculateVelocityScalars(ref VehicleProperties vehicleProperties, ref RaceProperties raceProperties)
    {
        // Do not add velocity if it's not the current player's turn to get calculated
        if (vehicleProperties.playerIndex != raceProperties.velocityPollTimer)
        {
            return;
        }

        // Calculate delta in X direction
        int speed = Math.Abs(vehicleProperties.velocity);
        int xScalar;
        if (vehicleProperties.velocity < 0)
        {
            xScalar = -raceProperties.VELOCITY_SCALAR_X_LUT[vehicleProperties.heading];
        }
        else
        {
            xScalar = raceProperties.VELOCITY_SCALAR_X_LUT[vehicleProperties.heading];
        }
        vehicleProperties.xVelocity = MultiplyDeltaFromVelocity(xScalar, speed);
        // TODO: find what terrain type 0x12 is and if this is correct
        //       It seems like it adds 2 to the high byte of x velocity
        //       There is not similar code for y velocity
        if (vehicleProperties.terrainType == 0x12)
        {
            vehicleProperties.xVelocity += 0x200;
        }

        // Calculate delta in Y direction
        int yScalar;
        if (vehicleProperties.velocity < 0)
        {
            yScalar = -raceProperties.VELOCITY_SCALAR_Y_LUT[vehicleProperties.heading];
        }
        else
        {
            yScalar = raceProperties.VELOCITY_SCALAR_Y_LUT[vehicleProperties.heading];
        }
        vehicleProperties.yVelocity = MultiplyDeltaFromVelocity(yScalar, speed);
    }

    public int MultiplyDeltaFromVelocity(int scalar, int speed)
    {
        return scalar * speed;
    }

    public override void CalculateVelocityEffects(ref VehicleProperties vehicleProperties, ref RaceProperties raceProperties)
    {
        // Calculate differences
        vehicleProperties.xVelocityForceDifference = Math.Abs(vehicleProperties.xVelocity - vehicleProperties.xForce);
        vehicleProperties.yVelocityForceDifference = Math.Abs(vehicleProperties.yVelocity - vehicleProperties.yForce);

        // Check if vehicle should drift or be affected by external friction
        int driftThresholdIndexLUT = 0;
        if (!vehicleProperties.hasUnlimitedGrip)
        {
            //Label8596:
            if (raceProperties.hasUnlimitedGrip && vehicleProperties.playerIndex == 0)
            {
                vehicleProperties.xForce = vehicleProperties.xVelocity;
                vehicleProperties.yForce = vehicleProperties.yVelocity;
                return;
            }
            else
            {
                //Label859F:
                if (vehicleProperties.gripChangeTimer == 0)
                {
                    //Label85AE:
                    driftThresholdIndexLUT = raceProperties.HANDICAP_LUT[vehicleProperties.handicapAmount];
                }
                else if (vehicleProperties.gripChangeTimer < 0)
                {
                    Debug.Log("DecreaseTimerAndCalculateExternalFriction()");
                    //DecreaseTimerAndCalculateExternalFriction();
                    return;
                }
                else
                {
                    vehicleProperties.gripChangeTimer--;
                    driftThresholdIndexLUT = 0;
                }
            }
        }
        else
        {
            driftThresholdIndexLUT = 0x15;
        }
        vehicleProperties.xyVelocityForceDifferenceMagnitude = vehicleProperties.yVelocityForceDifference + vehicleProperties.xVelocityForceDifference;
        if (vehicleProperties.xyVelocityForceDifferenceMagnitude >= raceProperties.DRIFT_THRESHOLD_LUT[driftThresholdIndexLUT])
        {
            Debug.Log("CalculateExternalFriction()");
            //CalculateExternalFriction();
            return;
        }
        vehicleProperties.xForce = vehicleProperties.xVelocity;
        vehicleProperties.yForce = vehicleProperties.yVelocity;

    }
}
