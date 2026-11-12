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

  # A television wants a free chair to watch from, one per viewer: the refusal says so.
  @timeout:600
  Scenario: a television with no chair is refused with its own reason
    Given Shared Joys: a "TubeTelevision" stands next to "Ada"
    When Shared Joys: "Ada" invites "Bo" to the "TubeTelevision"
    Then Shared Joys: the invitation failed
    And Shared Joys: the reason given for the "TubeTelevision" is the message "BluesNeedsChairToWatch"

  # A table that takes two players refuses a second pair, and says how many it takes.
  @timeout:900
  Scenario: a full chess table says how many it takes
    Given a colonist "Cy" exists
    And a colonist "Di" exists
    And Shared Joys: a "ChessTable" stands next to "Ada"
    When Shared Joys: "Ada" invites "Bo" to the "ChessTable"
    Then Shared Joys: the invitation succeeded
    When Shared Joys: "Cy" invites "Di" to the "ChessTable"
    Then Shared Joys: the invitation failed
    And Shared Joys: the reason given for the "ChessTable" is the message "BluesSeatsOnly" with 2 places

  # A reservation held for another job, a hauler passing through, is not a player at the table.
  @timeout:900
  Scenario: a reservation for another job does not take a place at the table
    Given a colonist "Cy" exists
    And Shared Joys: a "ChessTable" stands next to "Ada"
    And Shared Joys: "Cy" holds a reservation on the "ChessTable" for another job
    When Shared Joys: "Ada" invites "Bo" to the "ChessTable"
    Then Shared Joys: the invitation succeeded
    And Shared Joys: "Ada" has taken a recreation job at the "ChessTable"
    And Shared Joys: "Bo" has taken a recreation job at the "ChessTable"

  # A chair beside the table is a seat: the game plays on it, not on the ground.
  @timeout:900
  Scenario: a chess table with a chair beside it is played
    Given Shared Joys: a "ChessTable" stands next to "Ada"
    And Shared Joys: a "DiningChair" stands next to "Ada"
    When Shared Joys: "Ada" invites "Bo" to the "ChessTable"
    Then Shared Joys: the invitation succeeded
    And Shared Joys: "Ada" has taken a recreation job at the "ChessTable"
    And Shared Joys: "Bo" has taken a recreation job at the "ChessTable"
    And no errors were logged
