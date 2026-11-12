---
mod: Shared Joys
packageId: blues.sharedjoys
repo: SharedJoys
remote: https://github.com/khaiku/SharedJoys.git
visibility: public
detached: yes
workflow_stage: playTests[1.0.0]
licence: open
licence_at: "2026-11-09: MIT, LICENSE added to the repository with Blues' agreement; Blues stays the copyright holder with contributors"
upstream_mod_remotes: N/A
settings_audit: partial
localization: complete
translation_en: complete
translation_fr: complete
dependencies: none - no mod is required or referenced (no modDependencies, no MayRequire, no patches, no LoadFolders.xml, no OptionalCompat.xml: Check-OptionalCompat 0 targets, 0 declared); loadAfter Ludeon.RimWorld only; DLC are optional and guarded in code (ModsConfig.RoyaltyActive in ExtendedBuildingJoy.cs, ModsConfig.IdeologyActive in ExtendedBuildingJoy.cs); supportedVersions 1.6; re-read against the sources and About.xml 2026-11-11 at 4878c19
workshop_required_items: n/a, no mandatory Workshop dependency (About.xml has no modDependencies)
workshop: none
published: no
tested_on: never
remaining:
  - "2026-11-09: unverified: initial diagnostic 20261109-192311-614-3f5a: 4 of 6 scenarios passed (setting off, meditate together, chess without chair, walk); 2 failed because a sculpture (also a meditation focus) took a Meditate job; KindOf fix committed, small ticket 20261109-230537-595-3e61 (the 2 art scenarios) done 2026-11-10: 1 of 2 passed (two sculptures take two colonists), 'one sculpture for two colonists is refused with its own reason' failed: 'no diagnosis was worked out on this tick: Shared Joys would show its generic text' (step: reason given for SculptureSmall is BluesArtAllTaken); fixed 2026-11-11 at 9c826fa (HasAnyTarget ignored reservations wrongly: first participant's own reservation of the anchor made the diagnosis null), ticket 20261111-230126-171-b6e1 failed again (same message): cause is the Pickle step, it read the diagnosis on a later tick than the invitation (the float menu reads it on the same tick); step fixed at 24931fd, small ticket 20261112-003029-712-afea queued, result awaited from the TM: RUN_DONE"
  - "2026-11-09: unverified: Plus features ported into the code (art, graves, meditation foci, activity menu, ground seating, failure diagnosis, three fixes), builds green, never run in game"
  - "2026-11-09: unverified: Pickle suite written (3 features, 6 scenarios, Tests/Pickle), Check-Steps green, played once (4 of 6 green); graves, FreeParticipantSlots fix, IsValidChair fix, settings window and French pass have no scenario"
  - "2026-11-12: unverified: new scenarios (graves x2, television, full table, foreign reservation, chair beside) ticket 20261112-093336-604-74a7 and French pass ticket 20261112-093338-421-5428 queued, results awaited from the TM: RUN_DONE"
  - "2026-11-09: defect: 'Not enough space for everyone' on invitation, reported by two users (2026-10-28, 2026-11-09), not reproduced; FreeParticipantSlots fix is the main suspect"
  - "2026-11-11: unverified: settings in game (first use, effect of each option, persistence over restart and save reload, MainButton shortcut hidden on a clean config, RIMMSQOL listing and reveal, logs), settings scenarios to write at writeTests; Settings audit section"
  - "2026-11-12: /review-translation run twice (ChatGPT page 1 of 1 each), applied: 9 French labels and descriptions, 3 English fixes, 4 more French fixes (partner wording, BluesAlreadySatisfied, BluesEnableAutoDesc, BluesNoSpotBeside), DefInjected added; PUBLICATION.md not written yet; French validated by Virginie 2026-11-12 (6.g); not run in game (unverified)"
  - "2026-11-09: feature: RimWorld 1.7 support asked (2026-10-27)"
  - "2026-11-09: feature: retire SharedJoysPlus (Workshop page, USE_THIS_INSTEAD.md) once this release is published; the local folder was deleted 2026-11-12 on the owner's word (no remote, 4 local commits lost; its README and Art images were not ported)"
protocols_read_sha: c32984627d179fee6df20ce79bf3a883253e2511
updated: 2026-11-12
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

## Translation audit

- **Inventory (6.a, 6.b):** every displayed text comes from Keyed (English and French, 103 keys), Defs (InteractionDef, ThoughtDef, JobDef, MainButtonDef, French DefInjected with log rules) or the activity list (`BluesAct_` + defName, built in `PlacelessActivities.cs`). A bounded read of `Source/*.cs` finds no hard-coded UI string.
- **Coverage (6.c):** Check-Translations PASS (0 finding), Check-DefInjected 0 error over 22 keys, 0 key missing against English.
- **French agreement (6.d):** epicene wording (no gendered participle, "l'autre", "partenaire", "mon amour"), no tu, no vous; no player choice needed (6.e not applicable).
- **Review (6.f):** `TRANSLATION_REVIEW.md` regenerated at a68f88b; ChatGPT `/review-translation` run eight times, 22 recommendations applied, the rest rejected for a stated reason (`[RECIPIENT_possessive]` kept out of the French `Blues_SweepOffFeet` log rule: the token renders "son" or "sa" and would give "sa pieds").
- **Virginie review (6.g):** validated by Virginie on 2026-11-12, revision a68f88b, no correction asked. `PUBLICATION.md` does not exist yet (writeDocs).
- **Unverified until playTests (6.h):** raw keys, fallback text, truncation of the French in game, French DefInjected resolving in game.

## Tests (writeTests)

2026-11-12, revision 103dc6b, `BluesSharedJoys.dll` sha256 a1d41446bc3c...
- **7.a Scenarios:** 14 Pickle scenarios in 4 features (`Tests/Pickle/README.md` lists each behaviour); the promised behaviours of `PUBLICATION.md` cannot be matched, the file is written at writeDocs.
- **7.b Automated tests:** no unit-test project (`Run-UnitTests.ps1`: n/a); reason: the logic is Harmony patches and game state, only provable in game, hence Pickle. Shared checks replayed now: Check-Translations PASS, Check-DefInjected 0 error, Check-Steps green (18 patterns, 94 lines), `dotnet build` Release green; Check-ModBasics reports only the `nelim.` prefix, kept on purpose (owner, 2026-11-09). Scripts kept in the mod: none.
- **7.c XML tests:** n/a, no XML patch; the Defs are checked by Check-DefInjected and Check-Translations.
- **7.d Pickle suites written, not run:** 6 new scenarios (graves x2, television reason, full table, foreign reservation, chair beside, French x2 features); `@requires`: none besides Royalty (companion About.xml). Not covered on purpose: settings window, activity menu clicks, lovers carrying each other, other placeless activities, meditation refusal with no room.
- **7.e/7.f:** the tested sha is the one above; a code commit reopens 7.b.
