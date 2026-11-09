---
mod: Shared Joys
packageId: blues.sharedjoys
repo: SharedJoys
remote: https://github.com/khaiku/SharedJoys.git
visibility: public
detached: yes
workflow_stage: makeSources
licence: open
licence_at: "2026-11-09: MIT, LICENSE added to the repository with Blues' agreement; Blues stays the copyright holder with contributors"
upstream_mod_remotes: N/A
settings_audit: unchecked
localization: unchecked
translation_en: unchecked
translation_fr: unchecked
dependencies: unchecked
workshop: none
published: no
tested_on: never
remaining:
  - "2026-11-09: unverified: Pickle ticket 20261109-192311-614-3f5a (initial diagnostic, English, 6 scenarios) queued, result awaited from the TM: RUN_DONE; defNames and nicknames of the suite not checked before"
  - "2026-11-09: defect: Art/ sources taken (ModIcon-source.png from SharedJoysPlus, with a PLUS ribbon; Preview-source.png = original preview, 640x360 with the SHARED JOYS title baked in); owner decides on 2026-11-10 whether the PLUS stays, then ModIcon.png (128x128) and Preview.png (896x504) are rendered with Render-Preview.cjs and a new Art/Preview.config.json"
  - "2026-11-09: unverified: Art/preview.psd (Blues layered source) kept in Art/ by owner order 2026-11-09, outside the closed Art/ set: Check-ModBasics WARN, ask Audit to allow it"
  - "2026-11-09: unverified: Plus features ported into the code (art, graves, meditation foci, activity menu, ground seating, failure diagnosis, three fixes), builds green, never run in game"
  - "2026-11-09: unverified: Pickle suite written (3 features, 6 scenarios, Tests/Pickle), Check-Steps green, never played; graves, FreeParticipantSlots fix, IsValidChair fix, settings window and French pass have no scenario"
  - "2026-11-09: defect: 'Not enough space for everyone' on invitation, reported by two users (2026-10-28, 2026-11-09), not reproduced; FreeParticipantSlots fix is the main suspect"
  - "2026-11-09: defect: French DefInjected missing for the Blues defs and a keyed token mismatch on BluesCarryLoverTo (Check-Translations)"
  - "2026-11-09: feature: RimWorld 1.7 support asked (2026-10-27)"
  - "2026-11-09: feature: retire SharedJoysPlus (Workshop, repository, USE_THIS_INSTEAD.md) once this release is published"
protocols_read_sha: 7af1af6c0153644a77d687acce91d29bbe4e5ce2
updated: 2026-11-09
---

# Shared Joys: status

Current state only: the front matter above is the source of truth.

Original mod by Blues, taken over with Blues' agreement on 2026-11-09 and pushed to Blues' own
repository. `SharedJoysPlus` (companion mod) is folded into this one.

Decision, owner, 2026-11-09: 1.a (duplicate search) is not applicable: this is the original mod itself.

Decision, owner, 2026-11-09: `packageId` stays `blues.sharedjoys` (published identifier, existing
saves depend on it), so `Check-ModBasics` keeps reporting the missing `nelim.` prefix on purpose.
