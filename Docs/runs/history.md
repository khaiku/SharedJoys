# Pickle runs

One line per run: date, ticket, tested revision, result. Evidence stays on disk (gitignored).

- 2026-11-12 ticket 20261112-093336-604-74a7 (new scenarios): RED 3 of 6; graves x2 failed on the test step (corpse not in the grave), reservation scenario failed on the test step (stackCount All).
- 2026-11-12 ticket 20261112-093338-421-5428 (French pass): GREEN 2 of 2.
- 2026-11-12 ticket 20261112-140851-695-8e80 at 63a6e36 (the 3 failed): RED 0 of 3; test steps still wrong (corpse still spawned, reservation maxPawns mismatch).
- 2026-11-12 ticket 20261112-141906-117-17e3 at 6a42a90 (the 3 failed): graves x2 GREEN, reservation RED (test step reserved with stackCount 1, the play job uses 0).
- 2026-11-12 ticket 20261112-143719-928-6639 at d26075e (reservation scenario alone): RED, same message; cause is in the mod, GroundSeating.CanReserve used stackCount -1 against the recreation reservations (stackCount 0).
- 2026-11-12 ticket 20261112-154005-013-6eee at 4be456d (02-seating, after the GroundSeating fix): RED 4 of 5; only the foreign-reservation scenario fails, same message.
- 2026-11-12 ticket 20261112-180034-111-ad16 at 2dfc611: RED; Ada gets no job because Bo cannot join: Cy Wait reservation (max2 stack0) plus Ada fills the RimWorld cap of 2. Scenario rewritten to check the free-place count.
- 2026-11-12 ticket 20261112-180450-703-ee35 at 1901dec (foreign reservation, free-place count): GREEN 1 of 1.
- 2026-11-12 ticket 20261112-184649-623-6bfa at 1901dec (whole suite, 14 scenarios, run in English): 13 of 14 GREEN; the only failure is "the French interaction labels resolve", expected in an English run (04-french must be played with -Language French, GREEN on 5428). Folder deleted after reading.
