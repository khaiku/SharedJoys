using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Blues
{
    // Asks a building's own JoyGiver worker for the job instead of guessing where the spots are.
    // Every vanilla game/TV/telescope giver, and most modded ones, derive from
    // JoyGiver_InteractBuilding, whose CanInteractWith/TryGivePlayJob know the building better
    // than any heuristic of ours. Both are protected, hence the reflection.
    public static class JoyJobFactory
    {
        private const BindingFlags AnyInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly MethodInfo canInteractWith = typeof(JoyGiver_InteractBuilding).GetMethod("CanInteractWith", AnyInstance);
        private static readonly MethodInfo tryGivePlayJob = typeof(JoyGiver_InteractBuilding).GetMethod("TryGivePlayJob", AnyInstance);

        public static bool HasOwnWorker(JoyGiverDef giver)
        {
            return giver?.jobDef != null && giver.Worker is JoyGiver_InteractBuilding
                && canInteractWith != null && tryGivePlayJob != null;
        }

        // The job the giver itself would hand out right now, or null. Because this consults live
        // reservations, calling it for one pawn after another yields distinct spots.
        public static Job TryMakeJob(Pawn pawn, Building b, JoyGiverDef giver)
        {
            JoyGiver_InteractBuilding worker = (JoyGiver_InteractBuilding)giver.Worker;
            if (!worker.CanBeGivenTo(pawn)) return null;
            if (!(bool)canInteractWith.Invoke(worker, new object[] { pawn, b, false })) return null;
            return (Job)tryGivePlayJob.Invoke(worker, new object[] { pawn, b });
        }

        // Vanilla's capacity is the participant cap on the building itself.
        public static int FreeParticipantSlots(Building b, JoyGiverDef giver)
        {
            int reserved = 0;
            foreach (ReservationManager.Reservation r in b.Map.reservationManager.ReservationsReadOnly)
            {
                if (r.Target.Thing == b) reserved++;
            }
            return giver.jobDef.joyMaxParticipants - reserved;
        }
    }

    public static class JoyUtil
    {
        private static Dictionary<ThingDef, JoyGiverDef> joyGiverCache;

        public static JoyGiverDef GetJoyGiverFor(ThingDef buildingDef)
        {
            if (buildingDef == null) return null;
            if (joyGiverCache == null) BuildJoyGiverCache();
            return joyGiverCache.TryGetValue(buildingDef, out JoyGiverDef giver) ? giver : null;
        }

        // Built on first use rather than in a static ctor, so it can't run before defs load.
        private static void BuildJoyGiverCache()
        {
            List<JoyGiverDef> allGivers = DefDatabase<JoyGiverDef>.AllDefsListForReading;
            joyGiverCache = new Dictionary<ThingDef, JoyGiverDef>(allGivers.Count * 2);

            // Keeping the first giver per building matches what FirstOrDefault used to return.
            foreach (JoyGiverDef giver in allGivers)
            {
                if (giver.thingDefs == null) continue;
                foreach (ThingDef buildingDef in giver.thingDefs)
                {
                    if (buildingDef != null && !joyGiverCache.ContainsKey(buildingDef))
                        joyGiverCache[buildingDef] = giver;
                }
            }
        }

        // No default on 'manual' on purpose: every call site has to declare which side it's on.
        public static bool IsValidJoyBuilding(Building b, bool manual)
        {
            JoyGiverDef joyGiver = GetJoyGiverFor(b.def);
            CompGatherSpot spot = b.TryGetComp<CompGatherSpot>();
            if (joyGiver == null && (spot == null || !spot.Active)) return false;

            // Manual invites are free. Mile high club in the toilet is the player's business.
            if (manual) return true;

            return !IsPrison(b.GetRoom());
        }

        // Role is only PrisonCell for actual cells, so IsPrisonCell (which looks at the beds)
        // is what catches a prison barracks. Checking both costs nothing.
        private static bool IsPrison(Room room)
        {
            if (room == null) return false;
            if (room.IsPrisonCell) return true;

            RoomRoleDef role = room.Role;
            return role == RoomRoleDefOf.PrisonCell || role == RoomRoleDefOf.PrisonBarracks;
        }

        // Draw weight for autonomous events. Neutral is 1f, and the pickiness slider is applied
        // as an exponent over the whole product, so 0% flattens every room back to 1f.
        public static float GroupJoyWeight(Room room, Pawn inviter)
        {
            if (JoyousSet.Picky <= 0f) return 1f;

            float weight = RoomRoleFactor(room, inviter) * ImpressivenessFactor(room);
            return Mathf.Pow(Mathf.Max(weight, 0.0001f), JoyousSet.Picky);
        }

        private static float RoomRoleFactor(Room room, Pawn inviter)
        {
            if (room == null) return 1f;

            if (room.PsychologicallyOutdoors)
            {
                return JoyUtility.EnjoyableOutsideNow(inviter) ? 2f : 0.2f;
            }

            RoomRoleDef role = room.Role;
            if (role == null) return 1f;

            // A defName switch rather than RoomRoleDefOf: vanilla's DefOf has no RecRoom or
            // DiningRoom field, and this picks up modded roles with no dependency on them.
            switch (role.defName)
            {
                case "RecRoom": return 3f;
                case "IndoorGarden": return 2.5f;   // Dubs Skylights
                case "DiningRoom": return 1.5f;
                case "Bedroom": return 0.8f;
                case "Hospital": return 0.5f;

                // Workshop, Laboratory, Kitchen, Storeroom and Barracks stay neutral on purpose:
                // a starter colony often runs all of those out of one big room.
                default: return 1f;
            }
        }

        private static float ImpressivenessFactor(Room room)
        {
            if (room == null) return 1f;

            // Rooms with no stats report 0, and 0 means "not measured" here, not "awful".
            if (room.PsychologicallyOutdoors) return 1.05f;

            RoomRoleDef role = room.Role;
            if (role == null || role == RoomRoleDefOf.None || !room.ProperRoom) return 1f;

            // 0 is awful, 240 is wondrously impressive.
            float impressiveness = Mathf.Clamp(room.GetStat(RoomStatDefOf.Impressiveness), 0f, 240f);
            return 0.7f + (impressiveness / 240f) * 0.6f;
        }

        // Pawns are started one at a time: each started job makes its reservations immediately,
        // so the next pawn's job is built against the real state of the building. takenSpots
        // only matters for the legacy paths that don't reserve through a giver.
        public static bool TryAssignSharedJoy(Pawn inviter, Pawn invitee, Building recBuilding)
        {
            float inviterOriginalJoy = inviter.needs?.joy?.CurLevel ?? 0f;
            List<LocalTargetInfo> takenSpots = new List<LocalTargetInfo>();

            Job inviterJob = MakeJoyJob(inviter, recBuilding, takenSpots);
            if (inviterJob == null || !ForceTakeJob(inviter, inviterJob)) return false;
            LockSpots(inviterJob, takenSpots);

            Job inviteeJob = MakeJoyJob(invitee, recBuilding, takenSpots);
            if (inviteeJob != null && ForceTakeJob(invitee, inviteeJob))
            {
                RecordSharedJoy(PgbDefOf.Blues_InviteHangout, inviter, invitee);
                return true;
            }

            // Invitee bailed at the last second: undo the inviter and refund their joy.
            CancelHostJob(inviter, inviterOriginalJoy);
            return false;
        }

        // One entry point for "a joy job at this building for this pawn", whatever the building is.
        private static Job MakeJoyJob(Pawn pawn, Building b, List<LocalTargetInfo> takenSpots)
        {
            JoyGiverDef joyGiver = GetJoyGiverFor(b.def);
            if (joyGiver != null && joyGiver.jobDef != null)
            {
                return CreateGameJob(pawn, b, joyGiver, takenSpots);
            }

            // Social Relax (Campfire / Table)
            CompGatherSpot spot = b.TryGetComp<CompGatherSpot>();
            if (spot != null && spot.Active && TryGetSocialRelaxSpot(b, pawn, out LocalTargetInfo sitSpot, takenSpots))
            {
                return JobMaker.MakeJob(PgbDefOf.SocialRelax, b, sitSpot);
            }

            return null;
        }

        private static void LockSpots(Job job, List<LocalTargetInfo> takenSpots)
        {
            if (job.targetB.IsValid) takenSpots.Add(job.targetB);
            if (job.targetC.IsValid) takenSpots.Add(job.targetC);
        }

        // The log entry, the mutual memory and the speech bubble: everything that says "this happened".
        private static void RecordSharedJoy(InteractionDef interaction, Pawn initiator, Pawn recipient)
        {
            Find.PlayLog.Add(new PlayLogEntry_Interaction(interaction, initiator, recipient, null));

            initiator.needs?.mood?.thoughts?.memories?.TryGainMemory(PgbDefOf.Blues_SharedJoy_Thought, recipient);
            recipient.needs?.mood?.thoughts?.memories?.TryGainMemory(PgbDefOf.Blues_SharedJoy_Thought, initiator);

            MoteMaker.MakeInteractionBubble(initiator, recipient, ThingDefOf.Mote_Speech, interaction.GetSymbol());
        }

        // Nobody showed up: stop the host and refund the joy ForceTakeJob drained.
        private static void CancelHostJob(Pawn host, float originalJoy)
        {
            host.jobs?.StopAll();
            if (host.needs?.joy != null) host.needs.joy.CurLevel = originalJoy;
        }

        public static bool TryGetSocialRelaxSpot(Building gatheringSpot, Pawn pawn, out LocalTargetInfo sitSpot, List<LocalTargetInfo> spotsToIgnore = null)
        {
            sitSpot = LocalTargetInfo.Invalid;
            Map map = gatheringSpot.Map;
            IntVec3 pos = gatheringSpot.Position;

            // If a chair or cell is in our ignore list
            bool IsIgnored(LocalTargetInfo target) => spotsToIgnore != null && spotsToIgnore.Contains(target);

            if (gatheringSpot.def.surfaceType == SurfaceType.Eat)
            {
                Building chair = gatheringSpot.RandomAdjacentCellCardinal().GetEdifice(map);
                if (IsValidChair(chair, pawn) && !IsIgnored(chair))
                {
                    sitSpot = chair;
                    return true;
                }
            }

            foreach (IntVec3 cell in GenRadial.RadialCellsAround(pos, 3.9f, true))
            {
                Building chair = cell.GetEdifice(map);
                if (IsValidChair(chair, pawn) && !IsIgnored(chair) && GenSight.LineOfSight(pos, chair.Position, map, true))
                {
                    sitSpot = chair;
                    return true;
                }
            }

            for (int i = 0; i < 30; i++)
            {
                IntVec3 cell = pos + GenRadial.RadialPattern[Rand.Range(1, GenRadial.NumCellsInRadius(3.9f))];
                if (pawn.CanReserveAndReach(cell, PathEndMode.OnCell, Danger.None) && !IsIgnored(cell) && cell.GetEdifice(map) == null && GenSight.LineOfSight(pos, cell, map, true))
                {
                    sitSpot = cell;
                    return true;
                }
            }
            return false;
        }

        private static bool IsValidChair(Building chair, Pawn pawn)
        {
            if (chair == null) return false;

            bool isActuallySittable = chair.def.building.isSittable ||
                                      chair.def.defName.ToLower().Contains("bench") ||
                                      chair.def.defName.ToLower().Contains("seat");

            return isActuallySittable && pawn.CanReserveAndReach(chair, PathEndMode.OnCell, Danger.None);
        }

        public static bool IsOnHangoutCooldown(Pawn p)
        {
            if (p.needs?.mood?.thoughts?.memories == null) return false;

            return p.needs.mood.thoughts.memories.Memories.Any(m => m.def == PgbDefOf.Blues_SharedJoy_Thought && m.age < 2500);
        }

        private static bool ForceTakeJob(Pawn pawn, Job job)
        {
            if (pawn?.jobs == null) return false;

            if (pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc) && pawn.CurJob == job)
            {
                DrainJoy(pawn);
                return true;
            }

            return false;
        }

        private static void DrainJoy(Pawn pawn)
        {
            Need_Joy joy = pawn.needs?.joy;
            if (joy != null)
                joy.CurLevel = Math.Min(joy.CurLevel, JoyousSet.JoyDrainTo);
        }

        public static bool TryAssignGroupJoy(IEnumerable<Pawn> pawns, Building recBuilding, bool manual = false)
        {
            Pawn initiator = pawns.FirstOrDefault();
            if (initiator == null) return false;

            // SAVE STATE: Remember the host's joy level in case we have to cancel the party!
            float hostOriginalJoy = initiator.needs?.joy?.CurLevel ?? 0f;

            List<Pawn> successfulInvitees = new List<Pawn>();
            List<LocalTargetInfo> takenSpots = new List<LocalTargetInfo>();

            // An order from the player is worth braving a cold corridor; an idea of their own isn't.
            Danger maxDanger = manual ? Danger.Deadly : Danger.Some;

            foreach (Pawn pawn in pawns)
            {
                if (pawn.DeadOrDowned || !pawn.Awake() || pawn.needs?.joy == null) continue;

                if (manual) { if (pawn.needs.joy.CurLevel >= JoyousSet.MaxRecTrig) continue; }
                else if (JoyousSet.joyDrainDownTo == 20 && pawn.needs.joy.CurLevel >= JoyousSet.MaxRecAuto) continue;

                if (recBuilding.IsForbidden(pawn) || !pawn.CanReach(recBuilding, PathEndMode.ClosestTouch, maxDanger))
                {
                    if (pawn == initiator)
                    {
                        if (manual) Notify("BluesNoRoomForJoy".Translate(recBuilding.LabelShort), recBuilding);
                        return false;
                    }
                    continue;
                }

                // Zoning only binds autonomous events. A manual invite is an order.
                if (!manual && JoyousSet.respectAreas && !recBuilding.Position.InAllowedArea(pawn))
                {
                    if (pawn == initiator) return false;
                    continue;
                }

                bool assigned = false;

                if (pawn.CurJob?.def.joyKind != null && pawn.CurJob.targetA.Thing == recBuilding)
                {
                    // Already there: just stay a while longer.
                    pawn.needs.joy.CurLevel = Math.Min(pawn.needs.joy.CurLevel, JoyousSet.JoyDrainTo);
                    assigned = true;
                }
                else
                {
                    Job job = MakeJoyJob(pawn, recBuilding, takenSpots);
                    if (job != null && ForceTakeJob(pawn, job))
                    {
                        LockSpots(job, takenSpots);
                        assigned = true;
                    }
                }

                if (assigned)
                {
                    if (pawn != initiator) successfulInvitees.Add(pawn);
                }
                else if (pawn == initiator)
                {
                    if (manual) Notify("BluesNoRoomForJoy".Translate(recBuilding.LabelShort), recBuilding);
                    return false;
                }
            }

            // THE VERDICT: Did anyone actually show up?
            if (successfulInvitees.Any())
            {
                foreach (Pawn invitee in successfulInvitees)
                {
                    RecordSharedJoy(PgbDefOf.Blues_GroupHangout, initiator, invitee);
                }
                return true;
            }
            else
            {
                // NO FRIENDS JOINED! Cancel the host's sad, lonely party and refund their joy.
                CancelHostJob(initiator, hostOriginalJoy);

                if (manual) Notify("BluesNoRoomForJoy".Translate(recBuilding.LabelShort), recBuilding);
                return false;
            }
        }


        public static void Notify(string text, LookTargets look = null) => Messages.Message(text, look ?? LookTargets.Invalid, MessageTypeDefOf.RejectInput);


        public static bool IsValidInviteTarget(Pawn inviter, Pawn target)
        {
            if (target.DeadOrDowned || !target.Awake() || target.needs?.joy == null) return false;
            if (inviter.needs?.joy == null) return false;
            if (IsOnHangoutCooldown(target)) return false;
            if (target.HostileTo(Faction.OfPlayer)) return false;
            if (HealthAIUtility.ShouldSeekMedicalRest(target) || HealthAIUtility.ShouldBeTendedNowByPlayer(target)) return false;

            // A pawn under a Lord is mid-ritual, mid-party or loading a caravan. Pulling them out breaks it.
            if (IsUnderLord(target) || IsUnderLord(inviter)) return false;

            if (JoyousSet.joyDrainDownTo == 20 && (inviter.needs.joy.CurLevel >= JoyousSet.MaxRecAuto || target.needs.joy.CurLevel >= JoyousSet.MaxRecAuto)) return false;
            if (inviter.relations.OpinionOf(target) < JoyousSet.NeedFren || target.relations.OpinionOf(inviter) < JoyousSet.NeedFren) return false;

            if (IsBusy(target) || IsBusy(inviter)) return false;

            if (target.needs.food != null && target.needs.food.Starving) return false;
            if (inviter.needs.food != null && inviter.needs.food.Starving) return false;

            Room inviterRoom = inviter.GetRoom(); Room targetRoom = target.GetRoom();
            bool sameRoom = inviterRoom != null && !inviterRoom.PsychologicallyOutdoors && inviterRoom == targetRoom;
            bool closeEnough = JoyousSet.NeedProx != 0 && (JoyousSet.NeedProx > 200 || inviter.Position.DistanceToSquared(target.Position) <= JoyousSet.NeedProx * JoyousSet.NeedProx);
            if (!sameRoom && !closeEnough) return false;
            if (!inviter.CanReach(target, PathEndMode.ClosestTouch, Danger.Deadly)) return false;

            return true;
        }

        public static bool IsUnderLord(Pawn p) => p.GetLord() != null;

        // Jobs we never barge in on, even during recreation hours. Ordinary work, hauling and
        // meals are fair game on purpose: colonies rarely stop working, so excluding those would
        // leave no window for anything to ever happen.
        private static bool IsEmergencyJob(JobDef def)
        {
            return def == JobDefOf.BeatFire || def == JobDefOf.ExtinguishSelf
                || def == JobDefOf.Flee || def == JobDefOf.Vomit
                || def == JobDefOf.TendPatient || def == JobDefOf.FeedPatient
                || def == JobDefOf.Rescue || def == JobDefOf.Capture || def == JobDefOf.Arrest
                || def == JobDefOf.AttackMelee || def == JobDefOf.AttackStatic || def == JobDefOf.Wait_Combat
                || def == JobDefOf.Lovin
                || def == PgbDefOf.BluesCarryLoverToBed || def == PgbDefOf.BluesCarryLoverToPlay;
        }

        // True when a pawn shouldn't be pulled out of what they're doing by anything autonomous.
        public static bool IsBusy(Pawn p)
        {
            if (p.Drafted || p.InMentalState || p.IsBurning()) return true;

            Job job = p.jobs?.curJob;
            if (job == null) return false;
            if (job.playerForced) return true;
            if (!p.jobs.IsCurrentJobPlayerInterruptible()) return true;
            return IsEmergencyJob(job.def);
        }

        // Everything the manual "sweep off feet" order gets for free from the float menu gates,
        // re-checked here because the autonomous paths have no menu in front of them.
        public static bool CanCarryLover(Pawn carrier, Pawn lover) => CanCarryLover(carrier, lover, out _);

        // 'reason' is for dev-mode logging only; it's null when the answer is yes.
        public static bool CanCarryLover(Pawn carrier, Pawn lover, out string reason)
        {
            reason = null;
            if (carrier == null || lover == null || carrier == lover) { reason = "no pawn"; return false; }
            if (!lover.Spawned || lover.Map != carrier.Map) { reason = "lover not on this map"; return false; }
            if (lover.DeadOrDowned || !lover.Awake()) { reason = "lover down or asleep"; return false; }
            if (!LovePartnerRelationUtility.LovePartnerRelationExists(carrier, lover)) { reason = "not lovers"; return false; }

            int opinionOut = carrier.relations.OpinionOf(lover);
            int opinionIn = lover.relations.OpinionOf(carrier);
            if (opinionOut < JoyousSet.NeedLove || opinionIn < JoyousSet.NeedLove)
            {
                reason = $"opinion {opinionOut}/{opinionIn} below romance threshold {JoyousSet.NeedLove}";
                return false;
            }

            if (!carrier.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation)) { reason = "carrier can't manipulate"; return false; }
            if (carrier.carryTracker?.CarriedThing != null) { reason = "carrier's hands are full"; return false; }
            if (IsUnderLord(carrier) || IsUnderLord(lover)) { reason = "busy with a lord (ritual/caravan/party)"; return false; }
            if (IsBusy(carrier)) { reason = $"carrier is busy ({carrier.CurJobDef?.defName ?? "no job"})"; return false; }
            if (IsBusy(lover)) { reason = $"lover is busy ({lover.CurJobDef?.defName ?? "no job"})"; return false; }

            // The carry job reserves the lover on start; a failed reservation logs an error, so check first.
            if (!carrier.CanReserveAndReach(lover, PathEndMode.ClosestTouch, Danger.Deadly)) { reason = "can't reserve or reach lover"; return false; }

            return true;
        }

        // Slider (0-10 => 0-100%) times the weaker of the two opinions. 80 and 70 at 70% is 49%.
        public static float LoverCarryChance(Pawn a, Pawn b, int slider)
        {
            if (slider <= 0) return 0f;
            int opinion = Mathf.Min(a.relations.OpinionOf(b), b.relations.OpinionOf(a));
            if (opinion <= 0) return 0f;
            return (slider / 10f) * (Mathf.Min(opinion, 100) / 100f);
        }

        // Autonomous jobs go through StartJob rather than TryTakeOrderedJob so they aren't
        // flagged playerForced: the drivers use that flag to decide whether to nag the player.
        public static bool StartAutoJob(Pawn pawn, Job job)
        {
            if (pawn?.jobs == null) return false;
            job.count = 1;
            pawn.jobs.StartJob(job, JobCondition.InterruptForced, null, false, true, null, JobTag.Misc);
            return pawn.CurJob == job;
        }

        private static Job CreateGameJob(Pawn pawn, Building b, JoyGiverDef joyGiver, List<LocalTargetInfo> takenSpots)
        {
            // The building's own giver knows where its spots are. If it says no, it's a no.
            if (JoyJobFactory.HasOwnWorker(joyGiver))
            {
                return JoyJobFactory.TryMakeJob(pawn, b, joyGiver);
            }

            // Legacy guesswork, only for givers that don't derive from JoyGiver_InteractBuilding.

            // 1. Watch cells (every building has a default watch strip)
            if (WatchBuildingUtility.TryFindBestWatchCell(b, pawn, joyGiver.desireSit, out IntVec3 cell, out Building chair))
            {
                bool chairTaken = chair != null && takenSpots.Contains(chair);
                bool cellTaken = takenSpots.Contains(cell);

                if (!chairTaken && !cellTaken)
                {
                    return JobMaker.MakeJob(joyGiver.jobDef, b, cell, chair);
                }
            }

            // 2. Gather spots
            if (b.TryGetComp<CompGatherSpot>() != null)
            {
                if (TryGetSocialRelaxSpot(b, pawn, out LocalTargetInfo sitSpot, takenSpots))
                {
                    Building fallbackChair = sitSpot.HasThing ? sitSpot.Thing as Building : null;
                    return JobMaker.MakeJob(joyGiver.jobDef, b, sitSpot.Cell, fallbackChair);
                }
            }

            // 3. Ultimate fallback
            return JobMaker.MakeJob(joyGiver.jobDef, b);
        }

        // How many more pawns this building can take. 'probe' is the pawn whose eyes we borrow
        // to confirm the building is usable at all (a TV facing a wall has 8 slots and no cells).
        public static int GetAvailableSpots(Building b, Pawn probe)
        {
            Map map = b.Map;
            JoyGiverDef joyGiver = GetJoyGiverFor(b.def);

            // 1. Anything with a proper giver: vanilla's own participant cap.
            if (JoyJobFactory.HasOwnWorker(joyGiver))
            {
                if (JoyJobFactory.TryMakeJob(probe, b, joyGiver) == null) return 0;
                return JoyJobFactory.FreeParticipantSlots(b, joyGiver);
            }

            int spots = 0;

            // 2. Gather Spots (campfires and tables have no giver of their own, so this comes
            //    before the giver null check or they'd never count)
            CompGatherSpot spot = b.TryGetComp<CompGatherSpot>();
            if (spot != null && spot.Active)
            {
                float radius = b.def.surfaceType == SurfaceType.Eat ? 1.5f : (b.def.specialDisplayRadius > 0 ? b.def.specialDisplayRadius : 3.9f);
                foreach (IntVec3 cell in GenRadial.RadialCellsAround(b.Position, radius, true))
                {
                    if (!cell.InBounds(map) || !GenSight.LineOfSight(b.Position, cell, map, true)) continue;

                    Building chair = cell.GetEdifice(map);
                    if (chair != null && chair.def.building.isSittable && !map.reservationManager.IsReservedByAnyoneOf(chair, Faction.OfPlayer)) spots++;
                    else if (chair == null && cell.Standable(map) && b.def.surfaceType != SurfaceType.Eat && !map.reservationManager.IsReservedByAnyoneOf(cell, Faction.OfPlayer)) spots++;
                }
                return spots;
            }

            // 3. Legacy givers: free cells in the default watch strip.
            if (joyGiver == null) return 0;
            foreach (IntVec3 cell in WatchBuildingUtility.CalculateWatchCells(b.def, b.Position, b.Rotation, map))
            {
                if (cell.InBounds(map) && cell.Standable(map) && !map.reservationManager.IsReservedByAnyoneOf(cell, Faction.OfPlayer)) spots++;
            }
            return spots;
        }

    }
}
