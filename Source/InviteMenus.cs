using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Blues
{
    public abstract class FriendlyMenuProvider : FloatMenuOptionProvider
    {
        protected override bool Drafted => false;
        protected override bool Multiselect => false;
        protected override bool Undrafted => true;
        protected override bool RequiresManipulation => true;

        // Friends need this much mutual opinion to be invited. Lovers override with the romance bar.
        protected virtual int OpinionThreshold => JoyousSet.NeedFren;
        protected virtual string NotEnoughOpinionKey => "BluesNotEnoughFren";

        public override bool SelectedPawnValid(Pawn pawn, FloatMenuContext context)
        {
            if (!JoyousSet.enableMenus) return false;

            if (!(pawn.IsColonist || pawn.IsSlaveOfColony) || pawn.DeadOrDowned || !pawn.Awake()) return false;

            if (pawn.jobs?.curJob != null && !pawn.jobs.curJob.def.playerInterruptible) return false;

            return base.SelectedPawnValid(pawn, context);
        }
        public override bool TargetPawnValid(Pawn pawn, FloatMenuContext context)
        {
            if (pawn.needs?.joy == null || pawn.HostileTo(Faction.OfPlayer)) return false;
            if (pawn.DeadOrDowned || !pawn.Awake() || !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation)) return false;
            if (pawn.jobs?.curJob != null && !pawn.jobs.curJob.def.playerInterruptible) return false;
            return base.TargetPawnValid(pawn, context);
        }

        protected override FloatMenuOption GetSingleOptionFor(Pawn clickedPawn, FloatMenuContext context)
        {
            Pawn inviter = context.FirstSelectedPawn;
            if (inviter.needs?.joy?.CurLevel >= JoyousSet.MaxRecTrig)
                return new FloatMenuOption("BluesAlreadySatisfied".Translate(inviter.LabelShort), null) { Disabled = true };

            if (clickedPawn.needs?.joy?.CurLevel >= JoyousSet.MaxRecTrig)
                return new FloatMenuOption("BluesAlreadySatisfied".Translate(clickedPawn.LabelShort), null) { Disabled = true };

            if (!inviter.CanReach(clickedPawn, PathEndMode.ClosestTouch, Danger.Deadly))
                return new FloatMenuOption("BluesCantReach".Translate(inviter.LabelShort, clickedPawn.LabelShort), null) { Disabled = true };

            if (inviter.relations.OpinionOf(clickedPawn) < OpinionThreshold || clickedPawn.relations.OpinionOf(inviter) < OpinionThreshold)
                return new FloatMenuOption(NotEnoughOpinionKey.Translate(inviter.LabelShort, clickedPawn.LabelShort), null) { Disabled = true };

            return null;
        }
    }

    public class GroupJoyMenuProvider : FriendlyMenuProvider
    {
        protected override bool Multiselect => true;
        protected override FloatMenuOption GetSingleOptionFor(Pawn clickedPawn, FloatMenuContext context) => null;
        protected override FloatMenuOption GetSingleOptionFor(Thing clickedThing, FloatMenuContext context)
        {
            if (!(clickedThing is Building b) || !JoyUtil.IsValidJoyBuilding(b, true)) return null;

            var selPawns = new List<Pawn>();
            foreach (Pawn validSelectedPawn in context.ValidSelectedPawns)
            {
                if (CanReach(validSelectedPawn, b).Accepted)
                {
                    selPawns.Add(validSelectedPawn);
                }
            }
            if (selPawns.NullOrEmpty()) return null;

            // Capture the current state of selPawns into a new local list
            List<Pawn> pawnsToAssign = new List<Pawn>(selPawns);

            return new FloatMenuOption("BluesRelaxAt".Translate(clickedThing.LabelShort), delegate
            {
                // Use the captured local list here!
                JoyUtil.TryAssignGroupJoy(pawnsToAssign, b, true);
            }, MenuOptionPriority.High);
        }
        private static AcceptanceReport CanReach(Pawn pawn, Thing t)
        {
            if (!pawn.CanReach(t, PathEndMode.ClosestTouch, Danger.Deadly))
            {
                return "NoPath".Translate();
            }
            if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
            {
                return "Incapable".Translate();
            }
            return true;
        }
    }

    public class InviteMenuProvider : FriendlyMenuProvider
    {
        protected override FloatMenuOption GetSingleOptionFor(Pawn clickedPawn, FloatMenuContext context)
        {
            Pawn inviter = context.FirstSelectedPawn;
            FloatMenuOption baseOption = base.GetSingleOptionFor(clickedPawn, context); if (baseOption != null) return baseOption;

            //Messages.Message("inviter relations: " + inviter.relations.OpinionOf(clickedPawn).ToString(), MessageTypeDefOf.NeutralEvent);

            return new FloatMenuOption("BluesInviteTo".Translate(clickedPawn.LabelShort), delegate
            {
                TargetingParameters tp = new TargetingParameters
                {
                    canTargetBuildings = true,
                    validator = (TargetInfo t) => t.Thing is Building
                };
                       
                Find.Targeter.BeginTargeting(tp, delegate (LocalTargetInfo dest)
                {
                    if (!(dest.Thing is Building b)) return;

                    if (b.IsForbidden(inviter) || !inviter.CanReach(b, PathEndMode.ClosestTouch, Danger.Deadly))
                    {
                        JoyUtil.Notify("BluesCantReach".Translate(inviter.LabelShort, b.LabelShort), b); return;
                    }
                    if (b.IsForbidden(clickedPawn) || !clickedPawn.CanReach(b, PathEndMode.ClosestTouch, Danger.Deadly))
                    {
                        JoyUtil.Notify("BluesCantReach".Translate(clickedPawn.LabelShort, b.LabelShort), b); return;
                    }

                    if (!JoyUtil.IsValidJoyBuilding(b, true))
                    {
                        JoyUtil.Notify("BluesNoJoySpot".Translate(b.LabelShort), b); return;
                    }

                    if (!JoyUtil.TryAssignSharedJoy(inviter, clickedPawn, b))
                    {
                        JoyUtil.Notify("BluesNoRoomForJoy".Translate(b.LabelShort), b);
                    }

                }, inviter);
            }, MenuOptionPriority.High);
        }
    }

    public class CarryLoverProvider : FriendlyMenuProvider
    {
        protected override int OpinionThreshold => JoyousSet.NeedLove;
        protected override string NotEnoughOpinionKey => "BluesNotEnoughLove";

        public override bool TargetPawnValid(Pawn pawn, FloatMenuContext context)
        {
            if (!base.TargetPawnValid(pawn, context)) return false;
            if (!LovePartnerRelationUtility.LovePartnerRelationExists(context.FirstSelectedPawn, pawn)) return false;
            return true;
        }

        protected override FloatMenuOption GetSingleOptionFor(Pawn clickedPawn, FloatMenuContext context)
        {
            Pawn inviter = context.FirstSelectedPawn;
            FloatMenuOption baseOption = base.GetSingleOptionFor(clickedPawn, context); if (baseOption != null) return baseOption;

            return new FloatMenuOption("BluesCarryLoverTo".Translate(clickedPawn.LabelShort, clickedPawn.gender.GetObjective()), delegate
            {
                TargetingParameters tp = new TargetingParameters
                {
                    canTargetBuildings = true,
                    validator = (TargetInfo t) => t.Thing is Building
                };

                Find.Targeter.BeginTargeting(tp, delegate (LocalTargetInfo dest)
                {
                    if (!(dest.Thing is Building b)) return;

                    if (b.IsForbidden(inviter) || !inviter.CanReach(b, PathEndMode.ClosestTouch, Danger.Deadly))
                    {
                        JoyUtil.Notify("BluesCantReach".Translate(inviter.LabelShort, b.LabelShort), b); return;
                    }
                    if (b.IsForbidden(clickedPawn) || !clickedPawn.CanReach(b, PathEndMode.ClosestTouch, Danger.Deadly))
                    {
                        JoyUtil.Notify("BluesCantReach".Translate(clickedPawn.LabelShort, b.LabelShort), b); return;
                    }

                    Job job;

                    if (b is Building_Bed bed && bed.IsOwner(inviter) && bed.IsOwner(clickedPawn))
                    {
                        job = JobMaker.MakeJob(PgbDefOf.BluesCarryLoverToBed, clickedPawn, bed);
                    }
                    else
                    {
                        if (!JoyUtil.IsValidJoyBuilding(b, true))
                        {
                            JoyUtil.Notify("BluesNoJoySpot".Translate(b.LabelShort), b); return;
                        }
                        job = JobMaker.MakeJob(PgbDefOf.BluesCarryLoverToPlay, clickedPawn, b);
                    }

                    job.count = 1;
                    inviter.jobs.TryTakeOrderedJob(job, JobTag.Misc);

                }, inviter);
            }, MenuOptionPriority.High);
        }
    }
}
