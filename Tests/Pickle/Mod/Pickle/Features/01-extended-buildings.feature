# What only a running game can say about the buildings Shared Joys did not know before.
#
# Everything provable without the game stays out of here (the XML contract, the translation keys).
# What is left is whether the loaded game takes art and meditation spots as recreation spots,
# gives each colonist a piece of their own, and says the true reason when it cannot.
#
# Colonists "Ada" and "Bo" come from the fixture through Pickle's own step. The buildings are
# placed next to Ada by this suite: nothing here depends on a coordinate of the fixture.
Feature: art and meditation as shared recreation

  Background:
    Given the save "test-colony" is loaded
    And a colonist "Ada" exists
    And a colonist "Bo" exists
    And game speed is ultrafast

  # Two pieces of art, one for each: the group spreads over the neighbouring sculptures.
  @timeout:600
  Scenario: two sculptures take two colonists
    Given Shared Joys: a "SculptureSmall" stands next to "Ada"
    And Shared Joys: a "SculptureSmall" stands next to "Ada"
    When Shared Joys: "Ada" invites "Bo" to the "SculptureSmall"
    Then Shared Joys: the invitation succeeded
    And Shared Joys: "Ada" has taken a recreation job at the "SculptureSmall"
    And Shared Joys: "Bo" has taken a recreation job at the "SculptureSmall"
    And Shared Joys: "Ada" and "Bo" are doing the job "ViewArt"
    And Shared Joys: "Ada" and "Bo" stand on different targets
    And no errors were logged

  # An artwork admires one pawn at a time. The refusal used to say "not enough space"; it now says why.
  @timeout:600
  Scenario: one sculpture for two colonists is refused with its own reason
    Given Shared Joys: a "SculptureSmall" stands next to "Ada"
    When Shared Joys: "Ada" invites "Bo" to the "SculptureSmall"
    Then Shared Joys: the invitation failed
    And Shared Joys: the reason given for the "SculptureSmall" is the message "BluesArtAllTaken"

  # The setting is the switch for all of it: off, the building is not even accepted.
  Scenario: the setting off takes art out of the invitations
    Given Shared Joys: the setting to extend buildings is "off"
    And Shared Joys: a "SculptureSmall" stands next to "Ada"
    Then Shared Joys: the "SculptureSmall" is refused as a recreation building

  # Meditation is the only one of the three where a whole group shares the same focus.
  @timeout:900
  Scenario: two colonists meditate around the same spot
    Given Shared Joys: a "MeditationSpot" stands next to "Ada"
    When Shared Joys: "Ada" invites "Bo" to the "MeditationSpot"
    Then Shared Joys: the invitation succeeded
    And Shared Joys: "Ada" and "Bo" are doing the job "Meditate"
    And Shared Joys: "Ada" and "Bo" stand on different targets
    And no errors were logged
