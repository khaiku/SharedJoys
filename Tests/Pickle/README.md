# Shared Joys: Pickle tests

In-game functional tests, run by [Pickle](https://github.com/RimWorks/Rimworld-Pickle) through the shared
headless launcher. Development only, never published.

## What is tested, and what is not

Only what needs a running game: whether the loaded game takes art and meditation spots as recreation
spots, what a colonist ends up doing after an invitation, and the reason given when a group cannot be placed.
The menu entries themselves are float menus a scenario cannot click; the scenarios call what the entries
end in (`JoyUtil.TryAssignSharedJoy`, `PlacelessActivities.TryStart`).

| Feature | Behaviour | Evidence |
|---|---|---|
| `01-extended-buildings` | two sculptures take two colonists, each on a piece of their own; one sculpture for two is refused with `BluesArtAllTaken`; two graves take two colonists, one grave for two is refused with `BluesGraveAllTaken`; the setting off refuses art; two colonists meditate around one spot | job of each colonist, reason text |
| `02-seating` | a chess table with no chair is played on the ground; with a chair beside it, played; a reservation held for another job takes no place; a full table refuses a second pair with `BluesSeatsOnly` and its count; a television with no chair is refused with `BluesNeedsChairToWatch` | job of each colonist, reason text |
| `03-activities` | two colonists go for a walk together through the placeless activity list | same job for both |
| `04-french` | the French labels of the interactions and the translated keys resolve in game (played with `-Language French`, filter `04-french`) | label text, key lookup |

Not covered by a scenario, on purpose: the settings window and the activity menu entries (float menus and a settings dialog need a mouse, not scriptable here), lovers carrying each other, the other placeless activities, the meditation refusal with no room. They stay `unverified` in `STATUS.md`, not passed. Scenarios written 2026-11-12 for graves, the foreign reservation (`FreeParticipantSlots`), the chair beside a table (`IsValidChair`), the television and full-table reasons and the French pass have never run.

## Layout

- `Source/`: the C# project of the steps (`SharedJoys.PickleSteps.csproj`), built to `Mod/Pickle/Assemblies/`.
  Every step text starts with "Shared Joys:".
- `Mod/`: the test companion (`nelim.sharedjoys.pickletests`), with `Pickle/Features/`.

## Run

Through the ticket queue only, never by launching the game: file a ticket with
`Rimworld-Ticket-Dispatcher/scripts/Submit-PickleRun.ps1 -Mod SharedJoys` (see `docs/PICKLE.md` of the
protocols repository). The suite needs Royalty (the meditation spot) and the fixture `test-colony` of Pickle.
Build the steps first: `dotnet build Tests/Pickle/Source -c Release`.
