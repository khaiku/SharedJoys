---
mod: Shared Joys
packageId: blues.sharedjoys
repo: SharedJoys
remote: https://github.com/khaiku/SharedJoys.git
visibility: public
detached: yes
workflow_stage: localize[1.0.0]
licence: open
licence_at: "2026-11-09: MIT, LICENSE added to the repository with Blues' agreement; Blues stays the copyright holder with contributors"
upstream_mod_remotes: N/A
settings_audit: partial
localization: unchecked
translation_en: unchecked
translation_fr: partial
dependencies: none - no mod is required or referenced (no modDependencies, no MayRequire, no patches, no LoadFolders.xml, no OptionalCompat.xml: Check-OptionalCompat 0 targets, 0 declared); loadAfter Ludeon.RimWorld only; DLC are optional and guarded in code (ModsConfig.RoyaltyActive in ExtendedBuildingJoy.cs, ModsConfig.IdeologyActive in ExtendedBuildingJoy.cs); supportedVersions 1.6; re-read against the sources and About.xml 2026-11-11 at 4878c19
workshop_required_items: n/a, no mandatory Workshop dependency (About.xml has no modDependencies)
workshop: none
published: no
tested_on: never
remaining:
  - "2026-11-09: unverified: initial diagnostic 20261109-192311-614-3f5a: 4 of 6 scenarios passed (setting off, meditate together, chess without chair, walk); 2 failed because a sculpture (also a meditation focus) took a Meditate job; KindOf fix committed, small ticket 20261109-230537-595-3e61 (the 2 art scenarios) done 2026-11-10: 1 of 2 passed (two sculptures take two colonists), 'one sculpture for two colonists is refused with its own reason' failed: 'no diagnosis was worked out on this tick: Shared Joys would show its generic text' (step: reason given for SculptureSmall is BluesArtAllTaken); fixed 2026-11-11 at 9c826fa (HasAnyTarget ignored reservations wrongly: first participant's own reservation of the anchor made the diagnosis null), ticket 20261111-230126-171-b6e1 failed again (same message): cause is the Pickle step, it read the diagnosis on a later tick than the invitation (the float menu reads it on the same tick); step fixed at 24931fd, small ticket 20261112-003029-712-afea queued, result awaited from the TM: RUN_DONE"
  - "2026-11-09: unverified: Plus features ported into the code (art, graves, meditation foci, activity menu, ground seating, failure diagnosis, three fixes), builds green, never run in game"
  - "2026-11-09: unverified: Pickle suite written (3 features, 6 scenarios, Tests/Pickle), Check-Steps green, played once (4 of 6 green); graves, FreeParticipantSlots fix, IsValidChair fix, settings window and French pass have no scenario"
  - "2026-11-09: defect: 'Not enough space for everyone' on invitation, reported by two users (2026-10-28, 2026-11-09), not reproduced; FreeParticipantSlots fix is the main suspect"
  - "2026-11-11: unverified: settings in game (first use, effect of each option, persistence over restart and save reload, MainButton shortcut hidden on a clean config, RIMMSQOL listing and reveal, logs), settings scenarios to write at writeTests; Settings audit section"
  - "2026-11-12: French written: DefInjected for every Blues def (InteractionDef with log rules, ThoughtDef, JobDef, MainButtonDef), 0 coverage finding; Check-Translations still FAIL on 12 false BluesAct_* unused-key reports (keys read by dynamic lookup); BluesCarryLoverTo token mismatch removed by dropping the English objective pronoun (EN: Sweep {0} off their feet to..., code passes one argument)"
  - "2026-11-12: /review-translation run twice (ChatGPT page 1 of 1 each), applied: 9 French labels and descriptions, 3 English fixes, 4 more French fixes (partner wording, BluesAlreadySatisfied, BluesEnableAutoDesc, BluesNoSpotBeside), DefInjected added; PUBLICATION.md not written yet; waits for Virginie validation of the French (6.g), then translation_fr complete; not run in game (unverified)"
  - "2026-11-09: feature: RimWorld 1.7 support asked (2026-10-27)"
  - "2026-11-09: feature: retire SharedJoysPlus (Workshop, repository, USE_THIS_INSTEAD.md) once this release is published"
protocols_read_sha: b04475c28fcbf27bd47f4ee877f08147e180efb9
updated: 2026-11-11
---

# Shared Joys: status

Current state only: the front matter above is the source of truth.

Original mod by Blues, taken over with Blues' agreement on 2026-11-09 and pushed to Blues' own
repository. `SharedJoysPlus` (companion mod) is folded into this one.

Decision, owner, 2026-11-09: 1.a (duplicate search) is not applicable: this is the original mod itself.

Decision, owner, 2026-11-09: `packageId` stays `blues.sharedjoys` (published identifier, existing
saves depend on it), so `Check-ModBasics` keeps reporting the missing `nelim.` prefix on purpose.

Decision, owner, 2026-11-09: 2.e (palette check of the source illustration) waived: `Art/Preview-source.png` is Blues' original art with the title baked in, Preview render validated by the owner.

## Settings audit

2026-11-11, revision `ae750e7` (files: `Source/Mod.cs`, `Source/MainButtonWorker_Settings.cs`, `Mod/Defs/BluesMainButton.xml`, Keyed EN and FR).

- **Inventory (5.a):** 19 persisted options in `JoyousSet` (Scribe, global `ModSettings`, so scope is global, not per save): menus on/off, autonomous events on/off, recreation hours and anything hours, daily events (0 to 24), active hours table (24 toggles), joy drain floor, manual and automatic joy ceilings, friendship and romance thresholds, range, room pickiness, respect allowed areas, lover carry chances (play, bed), extend buildings (art, graves, meditation), activity menu on/off, list of disabled activities. Each has a concrete use read in code; no decorative option.
- **Decision (5.b):** settings are useful, all kept.
- **Primary access (5.c):** `BluesJoys : Mod` with `SettingsCategory()` and `DoSettingsWindowContents`: Mod options, Shared Joys, no XML edit, no RIMMSQOL needed.
- **Shortcut (5.d):** was missing (defect). Added `Mod/Defs/BluesMainButton.xml` (`Blues_SharedJoysSettings`, `buttonVisible` false, `validWithoutMap` true) and `Blues.MainButtonWorker_Settings` opening `Dialog_ModSettings` on the same mod. Opening the dialog from a revealed button is RIMMSQOL's domain, not checked on purpose (MOD_SETTINGS.md section 2).
- **Usage review (5.e):** every slider and checkbox has a label and a tooltip; window scrolls (`SettingsHeight` 870). Fixed in this pass: no reset (added "Reset to defaults", applies on window close, global), dependent controls vanished without a word when autonomous events are off (grey note added), hour tooltip was English-only (`BluesHourTip`). Timing: values apply immediately in memory; `WriteSettings` on close resets the daily joy counters of loaded maps. Inputs are sliders and checkboxes with fixed bounds; `hourlyToggles` is repaired to 24 entries on load; `settingsVersion` migration exists (1 and 2).
- **Tests (5.f):** compile green (`dotnet build` Release, 0 warnings); Check-ModBasics (only the `nelim.` prefix, owner decision), Check-Steps green; Check-Translations 26 findings, all already tracked under the French DefInjected defect and the false `BluesAct_*` unused-key reports, plus the two new MainButtonDef French fields. Not run: no unit-test project for settings, no Pickle scenario for the settings window. In-game rows of MOD_SETTINGS.md section 4 (first use, actual effect of each option, persistence across restart and save reload, shortcut hidden on a clean config, RIMMSQOL listing and reveal, logs) are `unverified`, for `playTests`.
- **Result (5.g):** `settings_audit: partial`.

Stage code, 2026-11-09: 3.a no open `feature` for 1.0.0 (1.7 support and retiring SharedJoysPlus come after the 1.6 playTests and the publication); 3.b `dotnet build` Release green, `Mod/Assemblies/BluesSharedJoys.dll` sha256 identical after rebuild; 3.c `CHANGELOG.md` `[Unreleased]` states the changes.
