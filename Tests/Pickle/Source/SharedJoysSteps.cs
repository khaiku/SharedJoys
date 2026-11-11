using System;
using System.Collections.Generic;
using System.Linq;
using Blues;
using RimWorks.Pickle;
using RimWorld;
using Verse;
using Verse.AI;

namespace SharedJoys.PickleSteps
{
    /// <summary>
    /// What Pickle's vanilla steps cannot reach: a building placed next to a colonist, the invitation
    /// itself (the call the float menu makes), the job each colonist ended up with, the reason
    /// Shared Joys gives for a refusal, and the placeless activity menu.
    ///
    /// Every step text starts with "Shared Joys:". Pickle loads the steps of every active suite into
    /// one namespace, and two suites declaring the same text make healthy scenarios fail with
    /// "Ambiguous step". No parentheses or slashes in a step text: Cucumber expressions read them as
    /// optional text and alternatives.
    ///
    /// Nothing here reads a translated word: a refusal is compared with the translation of the key
    /// the mod chose for it, so the scenarios hold in whichever language the pass was staged with.
    /// </summary>
    [PickleSteps]
    public class SharedJoysSteps
    {
        /// <summary>Scenario-local state: what was placed and what the last invitation said.</summary>
        class Stage
        {
            public readonly Dictionary<string, Thing> Things = new Dictionary<string, Thing>();
            public bool? Invited;
            public string Reason;
            public bool? ExtendBefore;
        }

        static Stage StageOf(PickleContext ctx)
        {
            try { return ctx.Get<Stage>(); }
            catch (InvalidOperationException)
            {
                var stage = new Stage();
                ctx.Set(stage);
                return stage;
            }
        }

        // ------------------------------------------------------------------ finding things

        static Map CurrentMap(PickleContext ctx)
        {
            ctx.Require(Current.Game != null && Find.CurrentMap != null, "load a save first");
            return Find.CurrentMap;
        }

        static Pawn Colonist(PickleContext ctx, string nickname)
        {
            Pawn pawn = CurrentMap(ctx).mapPawns.FreeColonists.FirstOrDefault(p =>
                p.Name is NameTriple triple && triple.Nick == nickname);
            ctx.Assert(pawn != null, $"no colonist nicknamed \"{nickname}\"");
            return pawn;
        }

        static Thing Placed(PickleContext ctx, string defName)
        {
            Stage stage = StageOf(ctx);
            ctx.Assert(stage.Things.TryGetValue(defName, out Thing thing) && thing.Spawned,
                $"no \"{defName}\" was placed in this scenario, or it is gone; placed: " +
                string.Join(", ", stage.Things.Keys));
            return thing;
        }

        static Building PlacedBuilding(PickleContext ctx, string defName)
        {
            Building building = Placed(ctx, defName) as Building;
            ctx.Assert(building != null, $"\"{defName}\" is not a building");
            return building;
        }

        // ------------------------------------------------------------------ placing

        /// <summary>
        /// Places a thing on a free standable cell within a few cells of the colonist, owned by the
        /// player. A second thing of the same def is stored under "defName#2", and so on, so a scenario
        /// can ask for two sculptures.
        /// </summary>
        [Given("Shared Joys: a {string} stands next to {string}")]
        public void PlaceNextTo(PickleContext ctx, string defName, string nickname)
        {
            Pawn near = Colonist(ctx, nickname);
            Map map = near.Map;
            ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
            ctx.Assert(def != null, $"no ThingDef named \"{defName}\"");

            IntVec3 cell = CellFinder.RandomClosewalkCellNear(near.Position, map, 4,
                c => c.Standable(map) && c.GetEdifice(map) == null && c.GetThingList(map).Count == 0
                     && GenConstruct.CanBuildOnTerrain(def, c, map, Rot4.North));
            ctx.Assert(cell.IsValid, $"no free cell within 4 cells of {nickname} for \"{defName}\"");

            ThingDef stuff = def.MadeFromStuff ? GenStuff.DefaultStuffFor(def) : null;
            Thing thing = ThingMaker.MakeThing(def, stuff);
            if (thing.def.CanHaveFaction) thing.SetFaction(Faction.OfPlayer);
            GenSpawn.Spawn(thing, cell, map, Rot4.North);

            Stage stage = StageOf(ctx);
            string key = defName;
            for (int i = 2; stage.Things.ContainsKey(key); i++) key = defName + "#" + i;
            stage.Things[key] = thing;
        }

        // ------------------------------------------------------------------ invitations

        /// <summary>
        /// The call "Invite ... to ..." on the float menu ends in: the inviter takes a job at the
        /// building, then the invitee does. The result is read by the "invitation" steps below.
        /// </summary>
        [When("Shared Joys: {string} invites {string} to the {string}")]
        public void Invite(PickleContext ctx, string inviterNick, string inviteeNick, string defName)
        {
            Building building = PlacedBuilding(ctx, defName);
            Pawn inviter = Colonist(ctx, inviterNick);
            Pawn invitee = Colonist(ctx, inviteeNick);
            ctx.Assert(JoyUtil.IsValidJoyBuilding(building, true),
                $"Shared Joys does not accept \"{defName}\" as a recreation building");
            Stage stage = StageOf(ctx);
            stage.Invited = JoyUtil.TryAssignSharedJoy(inviter, invitee, building);

            // The float menu calls Notify right after a failed assignment, on the same tick, and the
            // diagnosis only replaces the generic text on that tick. Read it here, not in a later step.
            string text = "<generic text>";
            stage.Reason = FailureDiagnosis.TryTakeOver(ref text) ? text : null;
        }

        [Then("Shared Joys: the invitation succeeded")]
        public void InvitationSucceeded(PickleContext ctx)
        {
            ctx.Assert(StageOf(ctx).Invited == true, "the invitation failed, or none was made");
        }

        [Then("Shared Joys: the invitation failed")]
        public void InvitationFailed(PickleContext ctx)
        {
            ctx.Assert(StageOf(ctx).Invited == false, "the invitation succeeded, or none was made");
        }

        /// <summary>
        /// The reason Shared Joys gives, read the way its own Notify reads it: the diagnosis worked
        /// out on this tick replaces the generic text. The key is the one the mod chose, translated
        /// with the building's short label, so the step holds in any language.
        /// </summary>
        [Then("Shared Joys: the reason given for the {string} is the message {string}")]
        public void ReasonIs(PickleContext ctx, string defName, string key)
        {
            Building building = PlacedBuilding(ctx, defName);
            string text = StageOf(ctx).Reason;
            ctx.Assert(text != null, "no diagnosis was worked out on the tick of the invitation: Shared Joys would show its generic text");
            string expected = key.Translate(building.LabelShort).ToString();
            ctx.Assert(text == expected, $"the reason was \"{text}\", expected \"{expected}\" ({key})");
        }

        [Then("Shared Joys: the {string} is refused as a recreation building")]
        public void Refused(PickleContext ctx, string defName)
        {
            Building building = PlacedBuilding(ctx, defName);
            ctx.Assert(!JoyUtil.IsValidJoyBuilding(building, true),
                $"Shared Joys accepts \"{defName}\" as a recreation building");
        }

        // ------------------------------------------------------------------ what the colonists did

        [Then("Shared Joys: {string} has taken a recreation job at the {string}")]
        public void HasJobAt(PickleContext ctx, string nickname, string defName)
        {
            Pawn pawn = Colonist(ctx, nickname);
            Building building = PlacedBuilding(ctx, defName);
            Job job = pawn.CurJob;
            ctx.Assert(job != null && job.def.joyKind != null,
                $"{nickname} has no recreation job; job: {job?.def.defName ?? "none"}");
            ctx.Assert(job.targetA.Thing == building || job.targetB.Thing == building || job.targetA.Thing != null && JobTargetsNear(job, building),
                $"{nickname}'s job {job.def.defName} does not target the {defName} or a piece next to it");
        }

        static bool JobTargetsNear(Job job, Building anchor)
        {
            Thing target = job.targetA.Thing;
            return target != null && target.Map == anchor.Map && target.Position.DistanceTo(anchor.Position) <= 12f;
        }

        [Then("Shared Joys: {string} and {string} are doing the job {string}")]
        public void BothDoing(PickleContext ctx, string nickA, string nickB, string jobDefName)
        {
            foreach (string nick in new[] { nickA, nickB })
            {
                Pawn pawn = Colonist(ctx, nick);
                string actual = pawn.CurJob?.def.defName ?? "none";
                ctx.Assert(actual == jobDefName, $"{nick} is doing \"{actual}\", expected \"{jobDefName}\"");
            }
        }

        [Then("Shared Joys: {string} and {string} stand on different targets")]
        public void DifferentTargets(PickleContext ctx, string nickA, string nickB)
        {
            Job a = Colonist(ctx, nickA).CurJob;
            Job b = Colonist(ctx, nickB).CurJob;
            ctx.Assert(a != null && b != null, "someone has no job");
            ctx.Assert(a.targetA != b.targetA || a.targetA.IsValid && a.targetB.IsValid && a.targetB != b.targetB,
                $"both colonists aim at the same target: {a.targetA}");
        }

        // ------------------------------------------------------------------ settings

        [Given("Shared Joys: the setting to extend buildings is {string}")]
        public void ExtendSetting(PickleContext ctx, string onOff)
        {
            Stage stage = StageOf(ctx);
            if (stage.ExtendBefore == null) stage.ExtendBefore = JoyousSet.extendBuildings;
            JoyousSet.extendBuildings = onOff == "on";
        }

        [AfterScenario]
        public void Restore(PickleContext ctx)
        {
            try
            {
                Stage stage = ctx.Get<Stage>();
                if (stage.ExtendBefore != null) JoyousSet.extendBuildings = stage.ExtendBefore.Value;
            }
            catch (InvalidOperationException) { }
        }

        // ------------------------------------------------------------------ placeless activities

        [Then("Shared Joys: the activity {string} is offered to {string}")]
        public void ActivityOffered(PickleContext ctx, string giverName, string nickname)
        {
            JoyGiverDef giver = PlacelessActivities.All.FirstOrDefault(d => d.defName == giverName);
            ctx.Assert(giver != null,
                $"\"{giverName}\" is not a placeless activity; the list holds: " +
                string.Join(", ", PlacelessActivities.All.Select(d => d.defName)));
            Pawn pawn = Colonist(ctx, nickname);
            ctx.Assert(PlacelessActivities.CanActuallyStart(giver, pawn), $"{nickname} cannot start \"{giverName}\" right now");
        }

        [When("Shared Joys: {string} and {string} start the activity {string}")]
        public void StartActivity(PickleContext ctx, string hostNick, string guestNick, string giverName)
        {
            JoyGiverDef giver = PlacelessActivities.All.FirstOrDefault(d => d.defName == giverName);
            ctx.Assert(giver != null, $"\"{giverName}\" is not a placeless activity");
            var group = new List<Pawn> { Colonist(ctx, hostNick), Colonist(ctx, guestNick) };
            bool started = PlacelessActivities.TryStart(group, giver, out string reason);
            StageOf(ctx).Invited = started;
            ctx.Assert(started, $"the activity did not start: {reason}");
        }

        [Then("Shared Joys: {string} and {string} are on the same activity")]
        public void SameActivity(PickleContext ctx, string nickA, string nickB)
        {
            JobDef a = Colonist(ctx, nickA).CurJob?.def;
            JobDef b = Colonist(ctx, nickB).CurJob?.def;
            ctx.Assert(a != null && a == b, $"{nickA} does \"{a?.defName ?? "none"}\", {nickB} does \"{b?.defName ?? "none"}\"");
        }

    }
}
