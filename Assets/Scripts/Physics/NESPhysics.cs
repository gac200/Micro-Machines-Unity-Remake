using System;
using System.Net;
using System.Reflection.Emit;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Vehicle.Shared;
using static Unity.Burst.Intrinsics.X86.Avx;
using static UnityEngine.InputSystem.Controls.AxisControl;

public class NESPhysics : VehiclePhysics
{
    public override void Turn(ref VehicleProperties vehicleProperties, ref RaceProperties raceProperties)
    {
        // Disable turning if countdown timer is not done
        if (raceProperties.countdownTimer == 0)
        {
            // Alternate turning logic for tanks
            if (raceProperties.vehicleType == TANKS)
            {
                // Do not turn if in an undriveable state
                if (vehicleProperties.spawnState == ALIVE)
                {
                    // Clamp heading when not turning
                    if (!vehicleProperties.controllerLeft && !vehicleProperties.controllerRight)
                    {
                        vehicleProperties.heading = HEADING_CLAMP_LUT[vehicleProperties.heading];
                    }
                    else
                    {
                        // Do not turn if not polled at any turn rate and in the incorrect acceleration state
                        if (!vehicleProperties.controllerAccelerate && !vehicleProperties.controllerBrake && !vehicleProperties.isAIControlledSpeed)
                        {
                            if (raceProperties.turnPollTimer != 0)
                            {
                                return;
                            }
                        }
                        else if (raceProperties.tankSlowTurnPollTimer != 0)
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
                }
            }

            // Do not turn if not polled at the turn rate or in an undriveable state
            else if (raceProperties.turnPollTimer == 1 && vehicleProperties.spawnState == ALIVE)
            {
                // Clamp heading when not turning
                if (!vehicleProperties.controllerLeft && !vehicleProperties.controllerRight)
                {
                    vehicleProperties.heading = HEADING_CLAMP_LUT[vehicleProperties.heading];
                }
                else
                {
                    // Move forward if turning while not moving and not a chopper
                    if (!vehicleProperties.isMoving && raceProperties.vehicleType != CHOPPERS)
                    {
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
            }
        }
    }

    public void TurnTanks(ref VehicleProperties vehicleProperties, ref RaceProperties raceProperties)
    {
        
    }

    public override void CalculateVelocityScalars(ref VehicleProperties vehicleProperties, ref RaceProperties raceProperties)
    {
        // Do not add velocity if it's not the current player's turn to get calculated
        if (vehicleProperties.playerIndex == raceProperties.velocityPollTimer)
        {
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
            vehicleProperties.xVelocity = (short)(xScalar * speed);
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
            vehicleProperties.yVelocity = (short)(yScalar * speed);
        }
    }

    public override void CalculateVelocityForces(ref VehicleProperties vehicleProperties, ref RaceProperties raceProperties)
    {
        // Calculate differences
        vehicleProperties.xVelocityForceDifference = (short)Math.Abs(vehicleProperties.xVelocity - vehicleProperties.xForce);
        vehicleProperties.yVelocityForceDifference = (short)Math.Abs(vehicleProperties.yVelocity - vehicleProperties.yForce);

        // Check if vehicle should drift or be affected by external friction
        int driftThresholdIndexLUT = 0;
        if (!vehicleProperties.hasUnlimitedGrip && (!raceProperties.hasUnlimitedGrip || vehicleProperties.playerIndex != 0))
        {
            if (vehicleProperties.gripChangeTimer >= 0)
            {
                if (vehicleProperties.gripChangeTimer > 0)
                {
                    vehicleProperties.gripChangeTimer--;
                    driftThresholdIndexLUT = 0;
                }
                if (vehicleProperties.gripChangeTimer == 0)
                {
                    driftThresholdIndexLUT = raceProperties.HANDICAP_LUT[vehicleProperties.handicapAmount];
                }
                vehicleProperties.xyVelocityForceDifferenceMagnitude = (short)(vehicleProperties.yVelocityForceDifference + vehicleProperties.xVelocityForceDifference);
                if (vehicleProperties.xyVelocityForceDifferenceMagnitude >= raceProperties.DRIFT_THRESHOLD_LUT[driftThresholdIndexLUT])
                {
                    CalculateExternalFriction(ref vehicleProperties, ref raceProperties);
                }
            }
            else if (vehicleProperties.gripChangeTimer < 0)
            {
                vehicleProperties.gripChangeTimer--;
                if (vehicleProperties.gripChangeTimer >= 0)
                {
                    vehicleProperties.gripChangeTimer = 0;
                }
            }
        }
        else
        {
            driftThresholdIndexLUT = 0x15;
            vehicleProperties.xyVelocityForceDifferenceMagnitude = (short)(vehicleProperties.yVelocityForceDifference + vehicleProperties.xVelocityForceDifference);
            if (vehicleProperties.xyVelocityForceDifferenceMagnitude >= raceProperties.DRIFT_THRESHOLD_LUT[driftThresholdIndexLUT])
            {
                CalculateExternalFriction(ref vehicleProperties, ref raceProperties);
            }
        }
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

        // There is more logic to determine whether to play drift sounds in the original,
        // but that is unnecessary now that we have more sound channels.
        if (vehicleProperties.driftSoundTimer == 0)
        {
            byte soundIndex = DRIFT_SOUND_LUT[raceProperties.vehicleType];
            PlayNonEngineSFX(ref vehicleProperties, soundIndex);
        }
    }

    public void PlayNonEngineSFX(ref VehicleProperties vehicleProperties, byte soundIndex)
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

    public override void CalculateVerticalForces(ref VehicleProperties vehicleProperties, ref RaceProperties raceProperties)
    {
        // Don't allow negative Z Position if Z Force is positive
        sbyte newZPosition;
        if (vehicleProperties.zForce >= 0)
        {
            newZPosition = (sbyte)(vehicleProperties.zPosition + vehicleProperties.zForce);
            if (newZPosition < 0)
            {
                newZPosition = 0x7f;
            }
        }
        else
        {
            newZPosition = (sbyte)(vehicleProperties.zPosition + vehicleProperties.zForce);
        }
        vehicleProperties.zPosition = newZPosition;

        // Select timer and decrease force if z position is positive,
        // otherwise calculate bounces from ground
        sbyte timer;
        if (vehicleProperties.zPosition > 0)
        {
            if (vehicleProperties.bounceBehavior == 1)
            {
                timer = raceProperties.changeZForceTimer1;
            }
            else
            {
                timer = raceProperties.changeZForceTimer2;
            }
        }
        else
        {
            CalculateVerticalBounce(ref vehicleProperties, ref raceProperties);
            return;
        }

        if (timer == 0)
        {
            vehicleProperties.zForce--;
        }
    }

    public static void CalculateVerticalBounce(ref VehicleProperties vehicleProperties, ref RaceProperties raceProperties)
    {
        sbyte newZPosition = vehicleProperties.zPosition;
        if (vehicleProperties.zForce == 0)
        {
            return;
        }
        if (vehicleProperties.bounceBehavior == 2)
        {
            vehicleProperties.checkpointIndex = -1;
            //PhysicsReset();
            //newZPosition = unkE193(9);
        }
        else
        {
            // 82B9 to 82C7
            if (BOUNCE_AMOUNT_LUT[raceProperties.vehicleType] + newZPosition >= 0)
            {
                //if (vehicleProperties.unk0438 >= 0)
                //{
                //    doC59F();
                //}
            }
        }
    }
}
