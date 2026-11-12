# The French pass: played with the language French, never in English (an English run fails the
# interaction labels on purpose: they are the French ones).
#
# What the loaded game says in French for the Defs and the keys that no code path of a scenario
# reaches: every DefInjected path resolves to the right field, and no key shows as itself.
# Run it alone: Submit-PickleRun.ps1 -Mod SharedJoys -Language French -Filter '04-french'
Feature: French texts

  Background:
    Given the save "test-colony" is loaded
    And game speed is ultrafast

  @timeout:300
  Scenario: the French interaction labels resolve
    Then Shared Joys: the interaction "Blues_InviteHangout" is labelled "Invitation à se détendre"
    And Shared Joys: the interaction "Blues_GroupHangout" is labelled "Rassemblement organisé"
    And Shared Joys: the interaction "Blues_SweepOffFeet" is labelled "Transport dans les bras"

  @timeout:300
  Scenario: the French keys do not show as themselves
    Then Shared Joys: the key "BluesNoJoySpot" is translated
    And Shared Joys: the key "BluesArtAllTaken" is translated
    And Shared Joys: the key "BluesGraveAllTaken" is translated
    And Shared Joys: the key "BluesNeedsChairToWatch" is translated
    And Shared Joys: the key "BluesCarryLoverTo" is translated
    And Shared Joys: the key "BluesAutoOffNote" is translated
    And no errors were logged
