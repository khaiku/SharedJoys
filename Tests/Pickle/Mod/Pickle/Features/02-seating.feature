# Boards without a chair, and a table that other jobs have reserved.
#
# Vanilla's board games want a chair and nothing in the base game turns that off, so a board with
# no chair around it used to refuse everyone with "not enough space". It is now played sitting on
# the ground, up to the number of players the game allows.
Feature: seating for shared recreation

  Background:
    Given the save "test-colony" is loaded
    And a colonist "Ada" exists
    And a colonist "Bo" exists
    And game speed is ultrafast

  @timeout:900
  Scenario: a chess table with no chair is played on the ground
    Given Shared Joys: a "ChessTable" stands next to "Ada"
    When Shared Joys: "Ada" invites "Bo" to the "ChessTable"
    Then Shared Joys: the invitation succeeded
    And Shared Joys: "Ada" has taken a recreation job at the "ChessTable"
    And Shared Joys: "Bo" has taken a recreation job at the "ChessTable"
    And no errors were logged
