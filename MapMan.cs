using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Blues
{
    public class JoyousMapManager : MapComponent
    {
        private int remainingJoysToday;
        private int lastTrackedLogicalDay = -1;
        private int currentHourTracked = -1;
        private int attemptsLeftThisHour;
        private int ticksToNextAttempt = -1;

        public JoyousMapManager(Map map) : base(map)
        {
            remainingJoysToday = JoyousSet.groupJoysPerDay;
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            if (!JoyousSet.enableAuto) return;

            if (Find.TickManager.TicksGame % 1250 == 0) CheckDailyReset();
            if (Find.TickManager.TicksGame % 20 == 0) TryBedCarries();

            int currentHour = GenLocalDate.DayTick(map) / GenDate.TicksPerHour;
            if (currentHour != currentHourTracked)
            {
                currentHourTracked = currentHour;
                BeginHour(currentHour);
            }

            if (remainingJoysToday <= 0 || attemptsLeftThisHour <= 0 || ticksToNextAttempt < 0) return;

            ticksToNextAttempt--;
            if (ticksToNextAttempt <= 0)
            {
                if (!HasDanger() && AttemptJoyEvent())
                {
                    remainingJoysToday--;
                    attemptsLeftThisHour--;
                    ScheduleNextAttempt();
                }
                else { ticksToNextAttempt = 1200; }
            }
        }

        private void BeginHour(int currentHour)
        {
            attemptsLeftThisHour = 0;
            ticksToNextAttempt = -1;

            if (remainingJoysToday <= 0) return;
            if (!JoyousSet.hourlyToggles[currentHour]) return;

            int enabledLeft = 0;
            for (int h = currentHour; h < 24; h++)
                if (JoyousSet.hourlyToggles[h]) enabledLeft++;
            if (enabledLeft == 0) return;

            attemptsLeftThisHour = Mathf.CeilToInt((float)remainingJoysToday / enabledLeft);
            ScheduleNextAttempt();
        }

        private void ScheduleNextAttempt()
        {
            if (attemptsLeftThisHour <= 0) { ticksToNextAttempt = -1; return; }

            int tickInHour = GenLocalDate.DayTick(map) % GenDate.TicksPerHour;
            int ticksLeftInHour = GenDate.TicksPerHour - tickInHour;
            if (ticksLeftInHour <= 0) { ticksToNextAttempt = -1; return; }

            int spacing = Mathf.Max(1, ticksLeftInHour / attemptsLeftThisHour);
            int jitter = Rand.Range(0, spacing / 4);
            ticksToNextAttempt = Mathf.Max(60, spacing - jitter);
        }

        private void CheckDailyReset()
        {
            int currentDay = GenLocalDate.DayOfYear(map);
            if (currentDay != lastTrackedLogicalDay)
            {
                lastTrackedLogicalDay = currentDay;
                remainingJoysToday = JoyousSet.groupJoysPerDay;
                currentHourTracked = -1; // forces BeginHour next tick
            }
        }

        public void ForceResetJoysToday()
        {
            remainingJoysToday = JoyousSet.groupJoysPerDay;
            currentHourTracked = -1;
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref remainingJoysToday, "remainingJoysToday", 3);
            Scribe_Values.Look(ref lastTrackedLogicalDay, "lastTrackedLogicalDay", -1);
            Scribe_Values.Look(ref currentHourTracked, "currentHourTracked", -1);
            Scribe_Values.Look(ref attemptsLeftThisHour, "attemptsLeftThisHour", 0);
            Scribe_Values.Look(ref ticksToNextAttempt, "ticksToNextAttempt", -1);
            Scribe_Collections.Look(ref bedCarryRolledJob, "bedCarryRolledJob", LookMode.Value, LookMode.Value);
            if (bedCarryRolledJob == null) bedCarryRolledJob = new Dictionary<int, int>();
        }


        // 'strict' also counts a colonist with any untended wound: fine for cancelling a party,
        // too much for bedtime, where only real emergencies should get in the way.
        public bool HasDanger(bool strict = true)
        {
            if (map.dangerWatcher.DangerRating != StoryDanger.None) return true;
            if (map.fireWatcher.LargeFireDangerPresent) return true;

            // Indexed loops on purpose: the mapPawns lists are lazy caches that can be rebuilt
            // mid-enumeration (vanilla events or other mods' patches reacting to the tend checks),
            // and a foreach over them throws "Collection was modified".
            List<Pawn> colonists = map.mapPawns.FreeColonistsSpawned;
            for (int i = 0; i < colonists.Count; i++)
            {
                if (strict ? HealthAIUtility.ShouldBeTendedNowByPlayer(colonists[i]) : HealthAIUtility.ShouldBeTendedNowByPlayerUrgent(colonists[i])) return true;
            }

            List<Pawn> slaves = map.mapPawns.SlavesOfColonySpawned;
            for (int i = 0; i < slaves.Count; i++) if (HealthAIUtility.ShouldBeTendedNowByPlayerUrgent(slaves[i])) return true;

            List<Pawn> prisoners = map.mapPawns.PrisonersOfColonySpawned;
            for (int i = 0; i < prisoners.Count; i++) if (HealthAIUtility.ShouldBeTendedNowByPlayerUrgent(prisoners[i])) return true;

            List<Pawn> animals = map.mapPawns.SpawnedColonyAnimals;
            for (int i = 0; i < animals.Count; i++) if (HealthAIUtility.ShouldBeTendedNowByPlayerUrgent(animals[i])) return true;

            return false;
        }

        private bool AttemptJoyEvent()
        {
            // Snapshot before filtering: the filter's health checks can make other mods rebuild
            // the live mapPawns caches mid-enumeration (see HasDanger).
            List<Pawn> pool = new List<Pawn>(map.mapPawns.FreeColonistsSpawned);
            pool.AddRange(map.mapPawns.SlavesOfColonySpawned);

            List<Pawn> candidates = pool
                .Where(p => !p.Drafted &&
                            !p.DeadOrDowned &&
                            p.Awake() &&
                            IsPawnAllowedToRelax(p) &&
                            !JoyUtil.IsOnHangoutCooldown(p))
                .ToList();

            if (candidates.Count < 2) return false;

            candidates = candidates
                .InRandomOrder()
                .OrderByDescending(p => p.CurJob?.def.joyKind != null ? 1 : 0)
                .ToList();

            foreach (Pawn inviter in candidates)
            {
                List<Pawn> validFriends = candidates
                    .Where(target => target != inviter && JoyUtil.IsValidInviteTarget(inviter, target))
                    .ToList();

                if (validFriends.Count > 0)
                {
                    int desiredFriends = Rand.RangeInclusive(1, Mathf.Min(3, validFriends.Count));
                    List<Pawn> groupToInvite = new List<Pawn> { inviter };

                    // Now and then a lover gets swept off their feet instead of invited. When that
                    // happens they're guaranteed in the group; the rest fill the remaining seats.
                    Pawn lover = null;
                    if (JoyousSet.loverCarryToPlay > 0)
                    {
                        Pawn candidate = validFriends.FirstOrDefault(f => JoyUtil.CanCarryLover(inviter, f));
                        if (candidate != null && Rand.Chance(JoyUtil.LoverCarryChance(inviter, candidate, JoyousSet.loverCarryToPlay)))
                        {
                            lover = candidate;
                            groupToInvite.Add(lover);
                            desiredFriends--;

                            // The others form their own group, which needs at least two of them.
                            // A lone extra would just be dropped, so either find a second or invite none.
                            if (desiredFriends == 1) desiredFriends = validFriends.Count >= 3 ? 2 : 0;
                        }
                    }
                    groupToInvite.AddRange(validFriends.Where(f => f != lover).InRandomOrder().Take(desiredFriends));

                    Building activeJoySpot = null;
                    if (inviter.CurJob?.def.joyKind != null && inviter.CurJob.targetA.Thing is Building b)
                    {
                        // Same gate as the spot search, or hosts who happen to already be in the
                        // prison would drag the whole group in behind them.
                        if (JoyUtil.IsValidJoyBuilding(b, false))
                        {
                            activeJoySpot = b;
                        }
                    }

                    Building recBuilding = activeJoySpot ?? FindBestGroupJoySpot(groupToInvite, inviter);

                    if (recBuilding != null)
                    {
                        if (lover != null)
                        {
                            if (TryStartLoverCarry(inviter, lover, groupToInvite, recBuilding)) { return true; }
                        }
                        else if (JoyUtil.TryAssignGroupJoy(groupToInvite, recBuilding)) { return true; }
                    }
                }
            }
            return false;
        }

        // The couple arrives by carry; the drop toil then seats them both via TryAssignSharedJoy.
        // Anyone else in the group walks over first, so the place is already humming when they land.
        private bool TryStartLoverCarry(Pawn carrier, Pawn lover, List<Pawn> group, Building recBuilding)
        {
            List<Pawn> others = group.Where(p => p != carrier && p != lover).ToList();
            bool othersWent = others.Count >= 2 && JoyUtil.TryAssignGroupJoy(others, recBuilding);

            Job carry = JobMaker.MakeJob(PgbDefOf.BluesCarryLoverToPlay, lover, recBuilding);
            bool carrying = JoyUtil.StartAutoJob(carrier, carry);

            return carrying || othersWent;
        }

        // Once per bedtime: a pawn heading for a bed they share with a lover may turn around and
        // go fetch them instead. Chance is the slider times their mutual opinion.
        // "Once per bedtime" is tracked by the sleep job's id, so it can't go stale across reloads.
        private Dictionary<int, int> bedCarryRolledJob = new Dictionary<int, int>();

        // Pawns fall asleep on the very first tick of lying down, so the only window we get is
        // the walk to the bed. Polled often for that reason, and the roll is only spent once a
        // carryable lover is actually in front of us, never on a wasted tick.
        private void TryBedCarries()
        {
            if (JoyousSet.loverCarryToBed <= 0) return;
            if (HasDanger(strict: false)) { LogBedCarryDanger(); return; }

            // Indexed for the same reason as HasDanger, and because starting a carry job inside
            // the loop can make other mods' patches poke the pawn caches.
            List<Pawn> colonists = map.mapPawns.FreeColonistsSpawned;
            for (int i = 0; i < colonists.Count; i++)
            {
                Pawn p = colonists[i];
                Job job = p.CurJob;
                if (job == null || job.def != JobDefOf.LayDown || !p.Awake()) continue;
                if (bedCarryRolledJob.TryGetValue(p.thingIDNumber, out int rolledJob) && rolledJob == job.loadID) continue;

                Building_Bed bed = job.targetA.Thing as Building_Bed;
                string reason = null;
                if (job.playerForced) reason = "sent to bed by the player";
                else if (bed == null) reason = "not heading for a bed";
                else if (bed.Medical || HealthAIUtility.ShouldSeekMedicalRest(p)) reason = "medical rest";
                else if (bed.SleepingSlotsCount < 2 || !bed.IsOwner(p)) reason = $"{bed.LabelShort} isn't a shared bed they own";
                else if (JoyUtil.IsUnderLord(p) || p.InMentalState) reason = "busy with a lord or a mental state";

                Pawn lover = null;
                if (reason == null)
                {
                    reason = "nobody else owns this bed";
                    foreach (Pawn o in bed.OwnersForReading)
                    {
                        if (o == p) continue;
                        if (o.InBed()) { reason = $"{o.LabelShort} is already in bed"; continue; }
                        if (!JoyUtil.CanCarryLover(p, o, out reason)) continue;
                        if (o.timetable?.CurrentAssignment == TimeAssignmentDefOf.Work) { reason = $"{o.LabelShort} is on work hours"; continue; }
                        lover = o;
                        break;
                    }
                }

                if (lover == null)
                {
                    LogBedCarry(p, job, reason, once: true);
                    continue;
                }

                // One roll per bedtime, whichever way it lands.
                bedCarryRolledJob[p.thingIDNumber] = job.loadID;

                float chance = JoyUtil.LoverCarryChance(p, lover, JoyousSet.loverCarryToBed);
                bool success = Rand.Chance(chance);
                LogBedCarry(p, job, $"rolled {chance:P0} for {lover.LabelShort}: {(success ? "carrying" : "walking alone")}", once: false);
                if (!success) continue;

                bool started = JoyUtil.StartAutoJob(p, JobMaker.MakeJob(PgbDefOf.BluesCarryLoverToBed, lover, bed));
                if (!started) LogBedCarry(p, job, "carry job refused to start", once: false);
            }
        }

        // Dev-mode only. 'once' messages print a single time per sleep job so the walk doesn't spam.
        private readonly Dictionary<int, int> bedCarryLoggedJob = new Dictionary<int, int>();
        private int lastDangerLogTick = -99999;

        private void LogBedCarry(Pawn p, Job job, string what, bool once)
        {
            if (!Prefs.DevMode || what == null) return;
            if (once)
            {
                if (bedCarryLoggedJob.TryGetValue(p.thingIDNumber, out int logged) && logged == job.loadID) return;
                bedCarryLoggedJob[p.thingIDNumber] = job.loadID;
            }
            Log.Message($"[Shared Joys] bed carry, {p.LabelShort}: {what}");
        }

        private void LogBedCarryDanger()
        {
            if (!Prefs.DevMode) return;
            int now = Find.TickManager.TicksGame;
            if (now - lastDangerLogTick < GenDate.TicksPerHour) return;
            lastDangerLogTick = now;
            Log.Message("[Shared Joys] bed carry: skipped, map has danger or someone needs tending");
        }

        private bool IsPawnAllowedToRelax(Pawn p)
        {
            if (p.needs?.joy == null) return false;

            if (JoyousSet.joyDrainDownTo == 20 && p.needs.joy.CurLevel >= JoyousSet.MaxRecAuto) return false;

            if (HealthAIUtility.ShouldSeekMedicalRest(p) || HealthAIUtility.ShouldBeTendedNowByPlayer(p)) return false;

            if (JoyUtil.IsBusy(p)) return false;
            if (p.needs.food != null && p.needs.food.Starving) return false;
            if (JoyUtil.IsUnderLord(p)) return false;

            TimeAssignmentDef schedule = p.timetable?.CurrentAssignment;
            if (JoyousSet.scheduleJoy && schedule == TimeAssignmentDefOf.Joy) return true;
            if (JoyousSet.scheduleAll && schedule == TimeAssignmentDefOf.Anything) return true;
            return false;
        }

        private struct JoySpotCandidate
        {
            public Building building;
            public int freeSpots;
            public float weight;
        }

        private Building FindBestGroupJoySpot(List<Pawn> group, Pawn inviter)
        {
            var candidates = new List<JoySpotCandidate>();
            var roomWeights = new Dictionary<Room, float>();

            foreach (Building b in map.listerBuildings.allBuildingsColonist)
            {
                if (!JoyUtil.IsValidJoyBuilding(b, false) || b.IsBurning()) continue;

                // Step 1: at least 2 people must be able to physically get there, and be allowed to.
                // Cheap rejects go first because the spot count below is the expensive part.
                int reachableCount = 0;
                foreach (Pawn p in group)
                {
                    if (b.IsForbidden(p)) continue;
                    if (JoyousSet.respectAreas && !b.Position.InAllowedArea(p)) continue;
                    if (p.CanReach(b, PathEndMode.ClosestTouch, Danger.Some)) reachableCount++;
                    if (reachableCount >= 2) break;
                }
                if (reachableCount < 2) continue;

                // Step 2: how many physical chairs/spots does it actually have?
                int freeSpots = JoyUtil.GetAvailableSpots(b, inviter);
                if (freeSpots < 2) continue; // Must hold at least the Host + 1 friend

                // Step 3: how nice is the room? Memoized because a room often holds several of these.
                Room room = b.GetRoom();
                float weight = 1f;
                if (room != null && !roomWeights.TryGetValue(room, out weight))
                {
                    weight = JoyUtil.GroupJoyWeight(room, inviter);
                    roomWeights[room] = weight;
                }

                candidates.Add(new JoySpotCandidate { building = b, freeSpots = freeSpots, weight = weight });
            }

            if (!candidates.Any()) return null;

            // Step 4: TIER 1 is the buildings that fit everyone. If none do, TIER 2 is everything
            // tied at the best effort size. Capacity still wins outright; the room weights only
            // decide who gets picked inside the winning tier.
            bool anyPerfectFit = candidates.Any(c => c.freeSpots >= group.Count);
            int tierSize = anyPerfectFit ? group.Count : candidates.Max(c => c.freeSpots);

            if (candidates.TryRandomElementByWeight(
                    c => c.freeSpots >= tierSize ? Mathf.Max(c.weight, 0.000001f) : 0f,
                    out JoySpotCandidate picked))
            {
                return picked.building;
            }

            // Can't happen while the weights are floored above, but never leave it to chance.
            return candidates.First(c => c.freeSpots >= tierSize).building;
        }
    }
}