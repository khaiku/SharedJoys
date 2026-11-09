using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace Blues
{
    public class JobDriver_CarrynLove : JobDriver
    {
        private const TargetIndex CarryTargetInd = TargetIndex.A;
        private const TargetIndex DestBedInd = TargetIndex.B;
        protected Pawn CarryTarget => (Pawn)job.GetTarget(CarryTargetInd).Thing;
        protected Building_Bed DestBed => (Building_Bed)job.GetTarget(DestBedInd).Thing;
        public override bool TryMakePreToilReservations(bool errorOnFailed) => pawn.Reserve(CarryTarget, job, 1, -1, null, errorOnFailed);

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(CarryTargetInd);
            this.FailOnDestroyedOrNull(DestBedInd);
            yield return Toils_Goto.GotoThing(CarryTargetInd, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(CarryTargetInd);
            yield return Toils_Haul.StartCarryThing(CarryTargetInd);

            Toil carryToBed = Toils_Goto.GotoThing(DestBedInd, PathEndMode.Touch);
            carryToBed.tickAction = delegate
            {
                if (pawn.IsHashIntervalTick(20))
                {
                    FleckMaker.ThrowMetaIcon(pawn.Position, pawn.Map, FleckDefOf.Heart);
                }
            };

            yield return carryToBed;

            Toil tuckIntoBed = new Toil
            {
                initAction = delegate
                {
                    IntVec3 dropLoc = RestUtility.GetBedSleepingSlotPosFor(CarryTarget, DestBed);
                    pawn.carryTracker.TryDropCarriedThing(dropLoc, ThingPlaceMode.Direct, out Thing droppedThing);
                    Pawn droppedPawn = droppedThing as Pawn;
                    if (droppedPawn != null && !droppedPawn.DeadOrDowned)
                    {
                        droppedPawn.jobs?.EndCurrentJob(JobCondition.InterruptForced);
                        Job layDown = JobMaker.MakeJob(JobDefOf.LayDownAwake, DestBed);
                        droppedPawn.jobs.TryTakeOrderedJob(layDown, JobTag.Misc);

                        pawn.interactions.TryInteractWith(droppedPawn, PgbDefOf.Blues_SweepOffFeet);

                        Job lovinJob = JobMaker.MakeJob(JobDefOf.Lovin, droppedPawn, DestBed);
                        pawn.jobs.jobQueue.EnqueueFirst(lovinJob);
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield return tuckIntoBed;
        }
    }

    public class JobDriver_CarrynChill : JobDriver
    {
        private const TargetIndex CarryTargetInd = TargetIndex.A;
        private const TargetIndex DestBuildingInd = TargetIndex.B;
        protected Pawn CarryTarget => (Pawn)job.GetTarget(CarryTargetInd).Thing;
        protected Building DestBuilding => (Building)job.GetTarget(DestBuildingInd).Thing;
        public override bool TryMakePreToilReservations(bool errorOnFailed) => pawn.Reserve(CarryTarget, job, 1, -1, null, errorOnFailed);

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(CarryTargetInd);
            this.FailOnDestroyedOrNull(DestBuildingInd);

            yield return Toils_Goto.GotoThing(CarryTargetInd, PathEndMode.ClosestTouch)
                .FailOnDespawnedNullOrForbidden(CarryTargetInd)
                .FailOnSomeonePhysicallyInteracting(CarryTargetInd);

            yield return Toils_Haul.StartCarryThing(CarryTargetInd, putRemainderInQueue: false, subtractNumTakenFromJobCount: false);

            Toil carryToBuilding = Toils_Goto.GotoThing(DestBuildingInd, PathEndMode.Touch)
                            .FailOnDespawnedNullOrForbidden(DestBuildingInd);

            carryToBuilding.tickAction = delegate
            {
                if (pawn.IsHashIntervalTick(40))
                {
                    FleckMaker.ThrowMetaIcon(pawn.Position, pawn.Map, FleckDefOf.Heart);
                }
            };

            yield return carryToBuilding;

            Toil dropAndPlayToil = new Toil
            {
                initAction = delegate
                {
                    IntVec3 dropLoc = pawn.Position;
                    pawn.carryTracker.TryDropCarriedThing(dropLoc, ThingPlaceMode.Direct, out Thing droppedThing);

                    Pawn droppedPawn = droppedThing as Pawn;
                    if (droppedPawn != null && !droppedPawn.DeadOrDowned)
                    {
                        droppedPawn.jobs?.EndCurrentJob(JobCondition.InterruptForced);

                        bool success = JoyUtil.TryAssignSharedJoy(pawn, droppedPawn, DestBuilding);

                        if (!success)
                        {
                            // Only the player's own orders earn a popup; autonomous carries fail quietly.
                            if (job.playerForced) JoyUtil.Notify("BluesNoRoomForJoy".Translate(DestBuilding.LabelShort), DestBuilding);
                        }
                        else { pawn.interactions.TryInteractWith(droppedPawn, PgbDefOf.Blues_SweepOffFeet); }
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield return dropAndPlayToil;
        }
    }
}
