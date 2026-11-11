using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Blues
{
    [DefOf]
    public static class PgbDefOf
    {
        public static JobDef BluesCarryLoverToBed;
        public static JobDef BluesCarryLoverToPlay;
        public static JobDef SocialRelax;
        public static ThoughtDef Blues_SharedJoy_Thought;
        public static InteractionDef Blues_InviteHangout;
        public static InteractionDef Blues_SweepOffFeet;
        public static InteractionDef Blues_GroupHangout;

        // Two base game JobDefs that RimWorld.JobDefOf does not expose: their vanilla givers reach
        // them through def.jobDef, never by name.
        public static JobDef ViewArt;
        public static JobDef VisitGrave;

        static PgbDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(PgbDefOf));
        }
    }

    public class JoyousSet : ModSettings
    {
        public static int joyDrainDownTo = 12;
        public static int maxRecTrigger = 19;
        public static int maxRecAutoTrigger = 15;
        public static int loverNeedLove = 10;
        public static int needFriendship = 6;

        // 0-10, in 10% steps. Multiplied by the lovers' mutual opinion, see JoyUtil.LoverCarryChance.
        public static int loverCarryToPlay = 2;
        public static int loverCarryToBed = 3;

        public static bool enableMenus = true;
        public static bool enableAuto = true;
        public static bool scheduleJoy = true;
        public static bool scheduleAll = true;

        public static int groupJoysPerDay = 3;

        public static List<bool> hourlyToggles = Enumerable.Repeat(true, 24).ToList();

        public static int requireProximity = 2;

        public static int roomPickiness = 5;
        public static bool respectAreas = true;

        public static int settingsVersion = 0;

        // Art pieces, graves and meditation foci as recreation spots (see ExtendedBuildingJoy).
        public static bool extendBuildings = true;

        // "Invite ... to an activity" for recreation with no building to click (see PlacelessActivities).
        public static bool enableActivityMenu = true;
        public static List<string> disabledActivities = new List<string>();

        public static bool IsActivityDisabled(JoyGiverDef d) => disabledActivities.Contains(d.defName);

        public static void SetActivityDisabled(JoyGiverDef d, bool disabled)
        {
            if (disabled)
            {
                if (!disabledActivities.Contains(d.defName)) disabledActivities.Add(d.defName);
            }
            else disabledActivities.Remove(d.defName);
        }

        /// <summary>Back to the documented defaults, the same ones ExposeData falls back on.</summary>
        public static void ResetToDefaults()
        {
            joyDrainDownTo = 12;
            maxRecTrigger = 19;
            maxRecAutoTrigger = 15;
            loverNeedLove = 10;
            needFriendship = 6;
            loverCarryToPlay = 2;
            loverCarryToBed = 3;
            enableMenus = true;
            enableAuto = true;
            scheduleJoy = true;
            scheduleAll = true;
            groupJoysPerDay = 3;
            hourlyToggles = Enumerable.Repeat(true, 24).ToList();
            requireProximity = 2;
            roomPickiness = 5;
            respectAreas = true;
            extendBuildings = true;
            enableActivityMenu = true;
            disabledActivities = new List<string>();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref joyDrainDownTo, "joyDrainDownTo", 12);
            Scribe_Values.Look(ref maxRecTrigger, "maxRecTrigger", 19);
            Scribe_Values.Look(ref maxRecAutoTrigger, "maxRecAutoTrigger", 15);
            Scribe_Values.Look(ref loverNeedLove, "loverNeedLove", 10);
            Scribe_Values.Look(ref needFriendship, "needFriendship", 6);
            Scribe_Values.Look(ref loverCarryToPlay, "loverCarryToPlay", 2);
            Scribe_Values.Look(ref loverCarryToBed, "loverCarryToBed", 3);

            Scribe_Values.Look(ref requireProximity, "requireProximity", 2);

            Scribe_Values.Look(ref enableMenus, "enableMenus", true);
            Scribe_Values.Look(ref enableAuto, "enableAuto", true);
            Scribe_Values.Look(ref scheduleJoy, "scheduleJoy", true);
            Scribe_Values.Look(ref scheduleAll, "scheduleAll", true);

            Scribe_Values.Look(ref roomPickiness, "roomPickiness", 5);
            Scribe_Values.Look(ref respectAreas, "respectAreas", true);

            Scribe_Values.Look(ref groupJoysPerDay, "groupJoysPerDay", 3);

            Scribe_Collections.Look(ref hourlyToggles, "hourlyToggles", LookMode.Value);
            if (hourlyToggles == null || hourlyToggles.Count != 24)
            {
                hourlyToggles = Enumerable.Repeat(true, 24).ToList();
            }

            Scribe_Values.Look(ref extendBuildings, "extendBuildings", true);
            Scribe_Values.Look(ref enableActivityMenu, "enableActivityMenu", true);
            Scribe_Collections.Look(ref disabledActivities, "disabledActivities", LookMode.Value);
            if (disabledActivities == null) disabledActivities = new List<string>();

            Scribe_Values.Look(ref settingsVersion, "settingsVersion", 0);

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (settingsVersion < 1)
                {
                    requireProximity = 2;
                    settingsVersion = 1;
                }
                if (settingsVersion < 2)
                {
                    // Room pickiness and area respect ship on for existing colonies too:
                    // this is a bug fix, not a new toy to go hunting for in the menu.
                    settingsVersion = 2;
                }
            }
        }

        public static float JoyDrainTo => joyDrainDownTo * 0.05f;
        public static float MaxRecTrig => maxRecTrigger * 0.05f;
        public static float MaxRecAuto => maxRecAutoTrigger * 0.05f;
        public static int NeedLove => loverNeedLove * 5;
        public static int NeedFren => needFriendship * 5;
        public static int NeedProx => requireProximity * 10;
        public static float Picky => roomPickiness * 0.2f;
    }

    public class BluesJoys : Mod
    {
        public BluesJoys(ModContentPack mod) : base(mod)
        {
            GetSettings<JoyousSet>();
            JoyJobFactory.ReportReflectionHealth();
        }

        private Vector2 scroll;

        // Content height of the settings window, taller than most screens' window: it scrolls.
        private const float SettingsHeight = 870f;

        public override string SettingsCategory() { return "BluesJoyMain".Translate(); }

        public override void WriteSettings()
        {
            base.WriteSettings();
            if (Current.ProgramState == ProgramState.Playing)
            {
                foreach (Map map in Find.Maps)
                {
                    JoyousMapManager mapMan = map.GetComponent<JoyousMapManager>();
                    mapMan?.ForceResetJoysToday();
                }
            }
        }

        // A label with a tooltip over a slider. Every setting gets a description this way.
        private static void LabeledSlider(Rect r, TaggedString label, string descKey, ref int value, int min, int max)
        {
            Widgets.Label(r.TopPart(0.5f), label);
            TooltipHandler.TipRegion(r.TopPart(0.5f), descKey.Translate());
            value = (int)Widgets.HorizontalSlider(r.BottomPart(0.4f), value, min, max);
        }

        private static void LabeledCheckbox(Rect r, string labelKey, string descKey, ref bool value)
        {
            Widgets.CheckboxLabeled(r, labelKey.Translate(), ref value, placeCheckboxNearText: true);
            TooltipHandler.TipRegion(r, descKey.Translate());
        }

        private static void SplitRow(Listing_Standard listing, float height, out Rect left, out Rect right)
        {
            Rect row = listing.GetRect(height);
            left = row.LeftHalf().ContractedBy(4f);
            right = row.RightHalf().ContractedBy(4f);
        }

        public override void DoSettingsWindowContents(Rect rect)
        {
            base.DoSettingsWindowContents(rect);
            Rect viewRect = new Rect(0f, 0f, rect.width - 20f, SettingsHeight);
            Widgets.BeginScrollView(rect, ref scroll, viewRect);
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(viewRect);
            listing.Gap(12f);

            // Applies on closing the window, for the current game and every later one (settings are global).
            SplitRow(listing, 30f, out Rect left, out Rect right);
            if (Widgets.ButtonText(right, "BluesResetDefaults".Translate()))
                JoyousSet.ResetToDefaults();
            TooltipHandler.TipRegion(right, "BluesResetDefaultsDesc".Translate());
            listing.Gap(6f);

            // --- Row 1: Menus | Drain ---
            SplitRow(listing, 60f, out left, out right);
            Widgets.CheckboxLabeled(left.TopPart(0.5f), "BluesEnableMenus".Translate(), ref JoyousSet.enableMenus, placeCheckboxNearText: true);

            TaggedString drainage = JoyousSet.joyDrainDownTo == 20 ? "BluesDontDrain".Translate() : "BluesJoyDrainTo".Translate(JoyousSet.joyDrainDownTo * 5);
            LabeledSlider(right, drainage, "BluesJoyDrainToDesc", ref JoyousSet.joyDrainDownTo, 0, 20);
            listing.Gap(6f);

            // --- Row 2: Manual threshold (left only) ---
            SplitRow(listing, 60f, out left, out right);
            LabeledSlider(left, "BluesMaxRecToTrigger".Translate(JoyousSet.maxRecTrigger * 5), "BluesMaxRecToTriggerDesc", ref JoyousSet.maxRecTrigger, 0, 20);
            listing.Gap(6f);

            // --- Row 3: Friendship | Romance ---
            SplitRow(listing, 60f, out left, out right);
            LabeledSlider(left, "BluesNeedFren".Translate(JoyousSet.NeedFren), "BluesNeedFrenDesc", ref JoyousSet.needFriendship, 0, 20);
            LabeledSlider(right, "BluesNeedLove".Translate(JoyousSet.NeedLove), "BluesNeedLoveDesc", ref JoyousSet.loverNeedLove, 0, 20);
            listing.Gap(12f);

            // --- Extensions: more places and activities to share ---
            Rect extendRow = listing.GetRect(30f).ContractedBy(4f);
            LabeledCheckbox(extendRow, "BluesExtendBuildings", "BluesExtendBuildingsDesc", ref JoyousSet.extendBuildings);
            listing.Gap(6f);

            SplitRow(listing, 30f, out left, out right);
            LabeledCheckbox(left, "BluesEnableActivityMenu", "BluesEnableActivityMenuDesc", ref JoyousSet.enableActivityMenu);
            if (JoyousSet.enableActivityMenu && Widgets.ButtonText(right, "BluesChooseActivities".Translate()))
                Find.WindowStack.Add(new Dialog_ChooseActivities());
            listing.Gap(12f);

            // --- Master switch for everything colonists do on their own ---
            Rect autoRow = listing.GetRect(30f).ContractedBy(4f);
            LabeledCheckbox(autoRow, "BluesEnableAuto", "BluesEnableAutoDesc", ref JoyousSet.enableAuto);
            listing.Gap(6f);

            if (JoyousSet.enableAuto)
            {
                // --- Row 4: Recreation hours | Anything hours ---
                SplitRow(listing, 30f, out left, out right);
                LabeledCheckbox(left, "BluesScheduleJoy", "BluesScheduleJoyDesc", ref JoyousSet.scheduleJoy);
                LabeledCheckbox(right, "BluesScheduleAll", "BluesScheduleAllDesc", ref JoyousSet.scheduleAll);
                listing.Gap(6f);

                // --- Row 5: Daily events | Auto threshold ---
                SplitRow(listing, 60f, out left, out right);
                LabeledSlider(left, "BluesGroupJoysPerDay".Translate(JoyousSet.groupJoysPerDay), "BluesGroupJoysPerDayDesc", ref JoyousSet.groupJoysPerDay, 0, 24);
                LabeledSlider(right, "BluesMaxRecAutoTrigger".Translate(JoyousSet.maxRecAutoTrigger * 5), "BluesMaxRecAutoTriggerDesc", ref JoyousSet.maxRecAutoTrigger, 0, 20);
                listing.Gap(6f);

                // --- Row 6: Active hours (full width) ---
                Rect hoursLabel = listing.GetRect(Text.LineHeight);
                Widgets.Label(hoursLabel, "BluesActiveHours".Translate());
                TooltipHandler.TipRegion(hoursLabel, "BluesActiveHoursDesc".Translate());
                Rect tableRect = listing.GetRect(30f);
                float hourWidth = tableRect.width / 24f;
                Color activeColor = new Color(0.2f, 0.8f, 0.2f);
                Color inactiveColor = new Color(0.3f, 0.3f, 0.3f);

                for (int i = 0; i < 24; i++)
                {
                    Rect hourRect = new Rect(tableRect.x + (i * hourWidth), tableRect.y, hourWidth, tableRect.height);
                    if (JoyousSet.hourlyToggles[i]) Widgets.DrawBoxSolid(hourRect, activeColor);
                    else Widgets.DrawBoxSolid(hourRect, inactiveColor);
                    Widgets.DrawBox(hourRect);

                    Text.Anchor = TextAnchor.MiddleCenter;
                    Text.Font = GameFont.Tiny;
                    Widgets.Label(hourRect, i.ToString());
                    Text.Anchor = TextAnchor.UpperLeft;
                    Text.Font = GameFont.Small;

                    if (Mouse.IsOver(hourRect))
                    {
                        Widgets.DrawHighlight(hourRect);
                        if (Input.GetMouseButton(0)) JoyousSet.hourlyToggles[i] = true;
                        else if (Input.GetMouseButton(1)) JoyousSet.hourlyToggles[i] = false;
                    }
                    TooltipHandler.TipRegion(hourRect, "BluesHourTip".Translate(i, i + 1));
                }
                listing.Gap(6f);

                // --- Row 7: Range | Room pickiness ---
                SplitRow(listing, 60f, out left, out right);
                TaggedString proximity = JoyousSet.requireProximity == 21 ? "BluesInfiniteRange".Translate() : JoyousSet.requireProximity == 0 ? "BluesNoRange".Translate() : "BluesRequireProximity".Translate(JoyousSet.NeedProx);
                LabeledSlider(left, proximity, "BluesRequireProximityDesc", ref JoyousSet.requireProximity, 0, 21);

                TaggedString pickiness = JoyousSet.roomPickiness == 0 ? "BluesRoomPickinessOff".Translate() : "BluesRoomPickiness".Translate(JoyousSet.roomPickiness * 10);
                LabeledSlider(right, pickiness, "BluesRoomPickinessDesc", ref JoyousSet.roomPickiness, 0, 10);

                // --- Row 8: Allowed areas ---
                SplitRow(listing, 30f, out left, out right);
                LabeledCheckbox(left, "BluesRespectAreas", "BluesRespectAreasDesc", ref JoyousSet.respectAreas);
                listing.Gap(6f);

                // --- Row 9: Lovers (Hangouts | Bed) ---
                SplitRow(listing, 60f, out left, out right);
                TaggedString carryPlay = JoyousSet.loverCarryToPlay == 0 ? "BluesLoverCarryPlayOff".Translate() : "BluesLoverCarryPlay".Translate(JoyousSet.loverCarryToPlay * 10);
                LabeledSlider(left, carryPlay, "BluesLoverCarryPlayDesc", ref JoyousSet.loverCarryToPlay, 0, 10);

                TaggedString carryBed = JoyousSet.loverCarryToBed == 0 ? "BluesLoverCarryBedOff".Translate() : "BluesLoverCarryBed".Translate(JoyousSet.loverCarryToBed * 10);
                LabeledSlider(right, carryBed, "BluesLoverCarryBedDesc", ref JoyousSet.loverCarryToBed, 0, 10);
            }
            else
            {
                // The controls below depend on this switch: say why they are not shown.
                Rect noteRect = listing.GetRect(Text.LineHeight * 2f);
                GUI.color = Color.gray;
                Widgets.Label(noteRect, "BluesAutoOffNote".Translate());
                GUI.color = Color.white;
            }
            listing.End();
            Widgets.EndScrollView();
        }
    }

    // Which placeless activities the "Invite ... to an activity" menu offers.
    public class Dialog_ChooseActivities : Window
    {
        private Vector2 scroll;

        public Dialog_ChooseActivities()
        {
            doCloseX = true;
            doCloseButton = true;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = true;
        }

        public override Vector2 InitialSize => new Vector2(420f, 560f);

        public override void DoWindowContents(Rect inRect)
        {
            Rect titleRect = new Rect(0f, 0f, inRect.width, 30f);
            Widgets.Label(titleRect, "BluesActivityList".Translate());

            List<JoyGiverDef> activities = PlacelessActivities.All;
            Rect listRect = new Rect(0f, 36f, inRect.width, inRect.height - 36f - Window.CloseButSize.y - 8f);
            Rect viewRect = new Rect(0f, 0f, listRect.width - 20f, activities.Count * 26f);

            Widgets.BeginScrollView(listRect, ref scroll, viewRect);
            float y = 0f;
            foreach (JoyGiverDef d in activities)
            {
                Rect row = new Rect(0f, y, viewRect.width, 24f);
                bool enabled = !JoyousSet.IsActivityDisabled(d);
                bool before = enabled;
                Widgets.CheckboxLabeled(row, PlacelessActivities.LabelFor(d), ref enabled);
                if (enabled != before) JoyousSet.SetActivityDisabled(d, !enabled);
                y += 26f;
            }
            Widgets.EndScrollView();
        }
    }
}
