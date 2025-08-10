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
        if (raceProperties.preRaceTimer != 0)
            return;

        // Alternate turning logic for tanks
        if (raceProperties.vehicleType == TANKS)
        {
            TurnTanks(ref vehicleProperties, ref raceProperties);
            return;
        }

        // Do not turn if not polled at the turn rate or in an undriveable state
        if (raceProperties.turnPollTimer != 1 || vehicleProperties.spawnState != ALIVE)
            return;

        // Clamp heading when not turning
        if (!vehicleProperties.controllerLeft && !vehicleProperties.controllerRight)
        {
            vehicleProperties.heading = HEADING_CLAMP_LUT[vehicleProperties.heading];
            return;
        }

        // Move forward if turning while not moving and not a chopper
        if (!vehicleProperties.isMoving && raceProperties.vehicleType != CHOPPERS)
            vehicleProperties.velocity = 1;

        // Turn in direction of controller input
        // Intentionally prioritizes left to match original behavior
        if (vehicleProperties.controllerLeft)
            vehicleProperties.heading--;
        else if (vehicleProperties.controllerRight)
            vehicleProperties.heading++;

        // Clamp direction between 0 and 63
        if (vehicleProperties.heading < 0)
            vehicleProperties.heading = 63;
        else if (vehicleProperties.heading > 63)
            vehicleProperties.heading = 0;
    }

    public void TurnTanks(ref VehicleProperties vehicleProperties, ref RaceProperties raceProperties)
    {
        // Do not turn if in an undriveable state
        if (vehicleProperties.spawnState != ALIVE)
            return;

        // Clamp heading when not turning
        if (!vehicleProperties.controllerLeft && !vehicleProperties.controllerRight)
        {
            vehicleProperties.heading = HEADING_CLAMP_LUT[vehicleProperties.heading];
            return;
        }

        // Do not turn if not polled at any turn rate and in the incorrect acceleration state
        if (!vehicleProperties.controllerAccelerate && !vehicleProperties.controllerBrake && !vehicleProperties.isAIControlledSpeed)
        {
            if (raceProperties.turnPollTimer != 0)
                return;
        }
        else if (raceProperties.tankSlowTurnPollTimer != 0)
            return;

        // Turn in direction of controller input
        // Intentionally prioritizes left to match original behavior
        if (vehicleProperties.controllerLeft)
            vehicleProperties.heading--;
        else if (vehicleProperties.controllerRight)
            vehicleProperties.heading++;

        // Clamp direction between 0 and 63
        if (vehicleProperties.heading < 0)
            vehicleProperties.heading = 63;
        else if (vehicleProperties.heading > 63)
            vehicleProperties.heading = 0;
    }

    public override void CalculateVelocityScalars(ref VehicleProperties vehicleProperties, ref RaceProperties raceProperties)
    {
        // Do not add velocity if it's not the current player's turn to get calculated
        if (vehicleProperties.playerIndex != raceProperties.velocityPollTimer)
            return;

        // Calculate delta in X direction
        sbyte speed = Math.Abs(vehicleProperties.velocity);
        sbyte xScalar = vehicleProperties.velocity < 0 ?
                        (sbyte)-raceProperties.VELOCITY_SCALAR_X_LUT[vehicleProperties.heading] :
                        raceProperties.VELOCITY_SCALAR_X_LUT[vehicleProperties.heading];
        vehicleProperties.xVelocity = (short)(xScalar * speed);

        // TODO: find what terrain type 0x12 is and if this is correct
        //       It seems like it adds 2 to the high byte of x velocity
        //       There is not similar code for y velocity
        if (vehicleProperties.terrainType == 0x12)
            vehicleProperties.xVelocity += 0x200;

        // Calculate delta in Y direction
        sbyte yScalar = vehicleProperties.velocity < 0 ?
                        (sbyte)-raceProperties.VELOCITY_SCALAR_Y_LUT[vehicleProperties.heading] :
                        raceProperties.VELOCITY_SCALAR_Y_LUT[vehicleProperties.heading];
        vehicleProperties.yVelocity = (short)(yScalar * speed);
    }

    public override void CalculateVelocityForces(ref VehicleProperties vehicle, ref RaceProperties race)
    {
        // Calculate differences
        vehicle.xVelocityForceDifference = (short)Math.Abs(vehicle.xVelocity - vehicle.xForce);
        vehicle.yVelocityForceDifference = (short)Math.Abs(vehicle.yVelocity - vehicle.yForce);
        vehicle.xyVelocityForceDifferenceMagnitude = (short)(vehicle.yVelocityForceDifference + vehicle.xVelocityForceDifference);

        // Check if vehicle should drift or be affected by external friction
        int driftThresholdIndexLUT = 0;
        if (!vehicle.hasUnlimitedGrip && (!race.hasUnlimitedGrip || vehicle.playerIndex != 0))
        {
            if (vehicle.gripChangeTimer >= 0)
            {
                if (vehicle.gripChangeTimer > 0)
                {
                    vehicle.gripChangeTimer--;
                    driftThresholdIndexLUT = 0;
                }
                if (vehicle.gripChangeTimer == 0)
                    driftThresholdIndexLUT = race.GRIP_HANDICAP_LUT[vehicle.handicapAmount];
                
                if (vehicle.xyVelocityForceDifferenceMagnitude >= race.DRIFT_THRESHOLD_LUT[driftThresholdIndexLUT])
                    CalculateExternalFriction(ref vehicle, ref race);
            }
            else if (vehicle.gripChangeTimer < 0)
            {
                vehicle.gripChangeTimer--;
                if (vehicle.gripChangeTimer >= 0)
                    vehicle.gripChangeTimer = 0;
            }
        }
        else
        {
            driftThresholdIndexLUT = 0x15;
            if (vehicle.xyVelocityForceDifferenceMagnitude >= race.DRIFT_THRESHOLD_LUT[driftThresholdIndexLUT])
                CalculateExternalFriction(ref vehicle, ref race);
        }


        vehicle.xForce = vehicle.xVelocity;
        vehicle.yForce = vehicle.yVelocity;
    }
    public void CalculateExternalFriction(ref VehicleProperties vehicleProperties, ref RaceProperties raceProperties)
    {
        // Find which drift amount to use
        int driftForceIndexLUT;
        if (!vehicleProperties.hasUnlimitedGrip)
            driftForceIndexLUT = 0x15;
        else if (raceProperties.vehicleType == CHOPPERS && !vehicleProperties.isAIControlledSpeed)
            driftForceIndexLUT = 0x0A;
        else if (vehicleProperties.gripChangeTimer > 0)
            driftForceIndexLUT = 0x16;
        else
            driftForceIndexLUT = raceProperties.GRIP_HANDICAP_LUT[vehicleProperties.handicapAmount];

        // Calculate X forces based on velocity and current force
        if (vehicleProperties.xVelocityForceDifference >= raceProperties.DRIFT_FORCE_AMOUNT_LUT[driftForceIndexLUT])
        {
            if (vehicleProperties.xVelocity < vehicleProperties.xForce)
                vehicleProperties.xForce += raceProperties.DRIFT_FORCE_AMOUNT_LUT[driftForceIndexLUT];
            else if (vehicleProperties.xVelocity > vehicleProperties.xForce)
                vehicleProperties.xForce -= raceProperties.DRIFT_FORCE_AMOUNT_LUT[driftForceIndexLUT];
        }
        else
            vehicleProperties.xForce = vehicleProperties.xVelocity;

        // Calculate Y forces based on velocity and current force
        if (vehicleProperties.yVelocityForceDifference >= raceProperties.DRIFT_FORCE_AMOUNT_LUT[driftForceIndexLUT])
        {
            if (vehicleProperties.yVelocity < vehicleProperties.yForce)
                vehicleProperties.yForce += raceProperties.DRIFT_FORCE_AMOUNT_LUT[driftForceIndexLUT];
            else if (vehicleProperties.yVelocity > vehicleProperties.yForce)
                vehicleProperties.yForce -= raceProperties.DRIFT_FORCE_AMOUNT_LUT[driftForceIndexLUT];
        }
        else
            vehicleProperties.yForce = vehicleProperties.yVelocity;

        // Decrease speed if not a chopper, drift timer is polled, and vehicle is moving
        if (raceProperties.vehicleType != CHOPPERS && raceProperties.driftSpeedLossTimer == 0 && vehicleProperties.velocity != 0)
        {
            if (vehicleProperties.velocity < 0)
                vehicleProperties.velocity++;
            else
                vehicleProperties.velocity--;
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

    public override void CalculateVerticalForces(ref VehicleProperties vehicleProperties, ref RaceProperties raceProperties)
    {
        // Clamp Z position to 0x7f if greater than that, otherwise add force
        vehicleProperties.zPosition = (sbyte)((vehicleProperties.zForce >= 0 && (sbyte)(vehicleProperties.zPosition + vehicleProperties.zForce) < 0)
                                      ? 0x7F : vehicleProperties.zPosition + vehicleProperties.zForce);

        // Select timer and decrease force if z position is positive,
        // otherwise calculate bounces from ground
        if (vehicleProperties.zPosition <= 0)
        {
            CalculateVerticalBounce(ref vehicleProperties, ref raceProperties);
            return;
        }

        sbyte timer = vehicleProperties.bounceBehavior == 1
                      ? raceProperties.changeZForceTimer1
                      : raceProperties.changeZForceTimer2;

        if (timer == 0)
            vehicleProperties.zForce--;
    }

    public void CalculateVerticalBounce(ref VehicleProperties vehicleProperties, ref RaceProperties raceProperties)
    {
        sbyte newZForce = vehicleProperties.zForce;
        if (vehicleProperties.zForce == 0)
            return;

        if (vehicleProperties.bounceBehavior == 2)
        {
            vehicleProperties.checkpointIndex = -1;
            PhysicsReset(ref vehicleProperties, ref raceProperties);
            unkE193(ref vehicleProperties, 9);
            vehicleProperties.unk0438 = -1;
            newZForce = 0;
        }
        else
        {
            newZForce += (sbyte)BOUNCE_AMOUNT_LUT[raceProperties.vehicleType];
            if (newZForce >= 0)
            {
                if (vehicleProperties.unk0438 >= 0)
                {
                    unkC59F(ref vehicleProperties, ref raceProperties);
                    vehicleProperties.unk0438 = -1;
                }
                newZForce = 0;
            }
        }
        vehicleProperties.zForce = (sbyte)-newZForce;
        vehicleProperties.zPosition = 0;
        vehicleProperties.bounceBehavior = 0;
        if (raceProperties.vehicleType != POWERBOATS)
            PlayNonEngineSFX(ref vehicleProperties, 8);
        else
        {
            PlayNonEngineSFX(ref vehicleProperties, 1);
            vehicleProperties.unk04D4 = -1;
        }
    }

    public void unkE193(ref VehicleProperties vehicleProperties, sbyte spawnState)
    {
        vehicleProperties.spawnState = spawnState;
        vehicleProperties.unk041C = 0;
    }

    public void PhysicsReset(ref VehicleProperties vehicleProperties, ref RaceProperties raceProperties)
    {
        vehicleProperties.xForce = 0;
        vehicleProperties.yForce = 0;
        vehicleProperties.xVelocity = 0;
        vehicleProperties.yVelocity = 0;
        vehicleProperties.isDrifting = false;
        vehicleProperties.velocity = 0;
        vehicleProperties.zPosition = 0;
        vehicleProperties.zForce = 0;
        vehicleProperties.controllerAccelerate = false;
        vehicleProperties.controllerBrake = false;
        vehicleProperties.controllerLeft = false;
        vehicleProperties.controllerRight = false;
        vehicleProperties.bounceBehavior = 0;
        vehicleProperties.gripChangeTimer = 0;
        unkC2D6(ref vehicleProperties, ref raceProperties);
    }

    public void unkC2D6(ref VehicleProperties vehicleProperties, ref RaceProperties raceProperties)
    {
        sbyte tempUnk0438 = vehicleProperties.unk0438;
        vehicleProperties.unk0438 = -1;
        if (tempUnk0438 >= 0)
        {
            unkC59F(ref vehicleProperties, ref raceProperties);
        }
    }

    public void unkC59F(ref VehicleProperties vehicleProperties, ref RaceProperties raceProperties)
    {
        vehicleProperties.unk059C = 0;
        if (vehicleProperties.unk05B8 >= 0)
        {
            // yeah idk I'll do it later
        }
    }

    public void PlayNonEngineSFX(ref VehicleProperties vehicleProperties, byte soundIndex)
    {
        if (vehicleProperties.hasUnlimitedGrip)
            return;
        if (!vehicleProperties.sfx.Contains(soundIndex))
            vehicleProperties.sfx.Add(soundIndex);
    }

    public override void CalculateAIAndTerrainEffects(ref VehicleProperties vehicleProperties, ref RaceProperties raceProperties)
    {

    }

    public override void CalculateAcceleration(ref VehicleProperties vehicleProperties, ref RaceProperties raceProperties)
    {
        if (vehicleProperties.forcedMovementVectorIndex == 0)
        {
            //unk8428();
        }
        if (raceProperties.preRaceTimer == 0 && vehicleProperties.spawnState == ALIVE)
        {
            sbyte velocityHandicapLUTIndex;
            //if (vehicleProperties.unk043C < 0)
            //{
            //    unk83C5();
            //}
            // Deceleration due to external friction
            if (!vehicleProperties.controllerAccelerate && !vehicleProperties.controllerBrake)
            {
                //unk83EC();
                return;
            }
            // Deceleration due to braking
            else if (vehicleProperties.controllerBrake)
            {
                //unk83CC();
                return;
            }
            // Cruise control
            else if (vehicleProperties.controllerAccelerate && vehicleProperties.controllerBrake)
                return;
            // Acceleration
            else
            {
                // TODO: uncomment when unkE8 is figured out
                //if (raceProperties.unkE8 != 0 && vehicleProperties.playerIndex != 0)
                //{
                //    velocityHandicapLUTIndex = 6;
                //}
                //else
                //{
                    velocityHandicapLUTIndex = vehicleProperties.handicapAmount;
                //}
                if (vehicleProperties.velocity < 0 || raceProperties.TOP_SPEED_HANDICAP_LUT[velocityHandicapLUTIndex] >= 0)
                {
                    // TODO: figure out unk101A
                    if (/*(raceProperties.unk010A != 0 && vehicleProperties.playerIndex == 0) || (raceProperties.unk010A == 0 && */ raceProperties.accelerationTimer == 0 /*)*/)
                        vehicleProperties.velocity++;
                }
            }
        }
    }
}
