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
        if (raceProperties.turnPollTimer == 0 || vehicleProperties.spawnState != ALIVE)
        {
            // Clamp heading when not turning
            if (!vehicleProperties.controllerLeft && !vehicleProperties.controllerRight)
            {
                vehicleProperties.heading = HEADING_CLAMP_LUT[vehicleProperties.heading];
            }
            return;
        }

        // Move forward if turning while not moving and not a chopper
        if ((vehicleProperties.controllerLeft || vehicleProperties.controllerRight) && !vehicleProperties.isMoving && raceProperties.vehicleType != CHOPPERS) {
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
        if (raceProperties.turnPollTimer == 0 && !vehicleProperties.controllerAccelerate && !vehicleProperties.controllerBrake)
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
        sbyte speed = Math.Abs(vehicleProperties.velocity);
        sbyte xScalar;
        if (vehicleProperties.velocity < 0)
        {
            xScalar = (sbyte)-raceProperties.VELOCITY_SCALAR_X_LUT[vehicleProperties.heading];
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
        sbyte yScalar;
        if (vehicleProperties.velocity < 0)
        {
            yScalar = (sbyte)-raceProperties.VELOCITY_SCALAR_Y_LUT[vehicleProperties.heading];
        }
        else
        {
            yScalar = raceProperties.VELOCITY_SCALAR_Y_LUT[vehicleProperties.heading];
        }
        vehicleProperties.yVelocity = MultiplyDeltaFromVelocity(yScalar, speed);
    }

    public short MultiplyDeltaFromVelocity(sbyte scalar, sbyte speed)
    {
        return (short)(scalar * speed);
    }

    public override void CalculateVelocityEffects(ref VehicleProperties vehicleProperties, ref RaceProperties raceProperties)
    {
        // Calculate differences
        vehicleProperties.xVelocityForceDifference = (short)Math.Abs(vehicleProperties.xVelocity - vehicleProperties.xForce);
        vehicleProperties.yVelocityForceDifference = (short)Math.Abs(vehicleProperties.yVelocity - vehicleProperties.yForce);

        // Check if vehicle should drift or be affected by external friction
        int driftThresholdIndexLUT = 0;
        if (!vehicleProperties.hasUnlimitedGrip)
        {
            if (raceProperties.hasUnlimitedGrip && vehicleProperties.playerIndex == 0)
            {
                goto ApplyForces;
            }
            else
            {
                if (vehicleProperties.gripChangeTimer == 0)
                {
                    driftThresholdIndexLUT = raceProperties.HANDICAP_LUT[vehicleProperties.handicapAmount];
                }
                else if (vehicleProperties.gripChangeTimer < 0)
                {
                    vehicleProperties.gripChangeTimer--;
                    if (vehicleProperties.gripChangeTimer < -127)
                    {
                        vehicleProperties.gripChangeTimer = 0;
                    }
                    goto ApplyForces;
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
        vehicleProperties.xyVelocityForceDifferenceMagnitude = (short)(vehicleProperties.yVelocityForceDifference + vehicleProperties.xVelocityForceDifference);
        if (vehicleProperties.xyVelocityForceDifferenceMagnitude >= raceProperties.DRIFT_THRESHOLD_LUT[driftThresholdIndexLUT])
        {
            CalculateExternalFriction(ref vehicleProperties, ref raceProperties);
            return;
        }
    ApplyForces:
        vehicleProperties.xForce = vehicleProperties.xVelocity;
        vehicleProperties.yForce = vehicleProperties.yVelocity;

    }
    public void CalculateExternalFriction(ref VehicleProperties vehicleProperties, ref RaceProperties raceProperties)
    {
        // Find which drift amount to use
        int driftForceIndexLUT;
        if (!vehicleProperties.hasUnlimitedGrip)
        {
            driftForceIndexLUT = 0x15;
        }
        else if (raceProperties.vehicleType == CHOPPERS && !vehicleProperties.isAIControlledSpeed)
        {
            driftForceIndexLUT = 0x0A;
        }
        else if (vehicleProperties.gripChangeTimer > 0)
        {
            driftForceIndexLUT = 0x16;
        }
        else
        {
            driftForceIndexLUT = raceProperties.HANDICAP_LUT[vehicleProperties.handicapAmount];
        }

        // Calculate X forces based on velocity and current force
        if (vehicleProperties.xVelocityForceDifference >= raceProperties.DRIFT_FORCE_AMOUNT_LUT[driftForceIndexLUT])
        {
            if (vehicleProperties.xVelocity < vehicleProperties.xForce)
            {
                vehicleProperties.xForce += raceProperties.DRIFT_FORCE_AMOUNT_LUT[driftForceIndexLUT];
            }
            else if (vehicleProperties.xVelocity > vehicleProperties.xForce)
            {
                vehicleProperties.xForce -= raceProperties.DRIFT_FORCE_AMOUNT_LUT[driftForceIndexLUT];
            }
        }
        else
        {
            vehicleProperties.xForce = vehicleProperties.xVelocity;
        }

        // Calculate Y forces based on velocity and current force
        if (vehicleProperties.yVelocityForceDifference >= raceProperties.DRIFT_FORCE_AMOUNT_LUT[driftForceIndexLUT])
        {
            if (vehicleProperties.yVelocity < vehicleProperties.yForce)
            {
                vehicleProperties.yForce += raceProperties.DRIFT_FORCE_AMOUNT_LUT[driftForceIndexLUT];
            }
            else if (vehicleProperties.yVelocity > vehicleProperties.yForce)
            {
                vehicleProperties.yForce -= raceProperties.DRIFT_FORCE_AMOUNT_LUT[driftForceIndexLUT];
            }
        }
        else
        {
            vehicleProperties.yForce = vehicleProperties.yVelocity;
        }

        // Decrease speed if not a chopper, drift timer is polled, and vehicle is moving
        if (raceProperties.vehicleType != CHOPPERS && raceProperties.driftSpeedLossTimer == 0 && vehicleProperties.velocity != 0)
        {
            if (vehicleProperties.velocity < 0)
            {
                vehicleProperties.velocity++;
            }
            else
            {
                vehicleProperties.velocity--;
            }
        }

        vehicleProperties.isDrifting = true;
        if ((raceProperties.isChallengeMode && vehicleProperties.playerIndex != 0) || vehicleProperties.playerIndex >= 2 || vehicleProperties.driftSoundTimer != 0)
        {
            return;
        }
        byte soundIndex = DRIFT_SOUND_LUT[raceProperties.vehicleType];
        PlayDriftSFX(ref vehicleProperties, soundIndex);
    }

    public void PlayDriftSFX(ref VehicleProperties vehicleProperties, byte soundIndex)
    {
        if (vehicleProperties.hasUnlimitedGrip)
        {
            return;
        }
        if (!vehicleProperties.sfx.Contains(soundIndex))
        {
            vehicleProperties.sfx.Add(soundIndex);
        }
    }
}
