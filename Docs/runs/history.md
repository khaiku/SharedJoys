# Pickle runs

One line per run: date, ticket, tested revision, result. Evidence stays on disk (gitignored).

- 2026-11-12 ticket 20261112-093336-604-74a7 (new scenarios): RED 3 of 6; graves x2 failed on the test step (corpse not in the grave), reservation scenario failed on the test step (stackCount All).
- 2026-11-12 ticket 20261112-093338-421-5428 (French pass): GREEN 2 of 2.
- 2026-11-12 ticket 20261112-140851-695-8e80 at 63a6e36 (the 3 failed): RED 0 of 3; test steps still wrong (corpse still spawned, reservation maxPawns mismatch).
- 2026-11-12 ticket 20261112-141906-117-17e3 at 6a42a90 (the 3 failed): graves x2 GREEN, reservation RED (test step reserved with stackCount 1, the play job uses 0).
