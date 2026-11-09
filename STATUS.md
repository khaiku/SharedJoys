---
mod: Shared Joys
packageId: blues.sharedjoys
repo: SharedJoys
remote: https://github.com/khaiku/SharedJoys.git
visibility: public
detached: yes
workflow_stage: setupRepo
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
  - "2026-11-09: unverified: 1.a duplicate search not run (this is the original mod itself)"
  - "2026-11-09: defect: Art/ missing (ModIcon-source.png, Preview-source.png), ModIcon.png is 256x256 instead of 128x128, Preview.png is 640x360 instead of 896x504"
  - "2026-11-09: defect: preview.psd at the repository root is off the closed list (STYLE_RIMWORLD.md), ask Audit where it goes"
  - "2026-11-09: unverified: Plus features ported into the code (art, graves, meditation foci, activity menu, ground seating, failure diagnosis, three fixes), builds green, never run in game"
  - "2026-11-09: defect: 'Not enough space for everyone' on invitation, reported by two users (2026-10-28, 2026-11-09), not reproduced; FreeParticipantSlots fix is the main suspect"
  - "2026-11-09: defect: French DefInjected missing for the Blues defs and a keyed token mismatch on BluesCarryLoverTo (Check-Translations)"
  - "2026-11-09: feature: RimWorld 1.7 support asked (2026-10-27)"
  - "2026-11-09: feature: retire SharedJoysPlus (Workshop, repository, USE_THIS_INSTEAD.md) once this release is published"
protocols_read_sha: e4083f55fe1ea09b1a6471ea20068fa0994b81bb
updated: 2026-11-09
---

# Shared Joys: status

Current state only: the front matter above is the source of truth.

Original mod by Blues, taken over with Blues' agreement on 2026-11-09 and pushed to Blues' own
repository. `SharedJoysPlus` (companion mod) is folded into this one.

Decision, owner, 2026-11-09: `packageId` stays `blues.sharedjoys` (published identifier, existing
saves depend on it), so `Check-ModBasics` keeps reporting the missing `nelim.` prefix on purpose.
