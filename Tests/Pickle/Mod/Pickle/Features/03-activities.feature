# The activity menu: recreation with no building to click.
#
# The menu entry itself is a float menu on a colonist, which a scenario cannot click without a
# physical mouse; what is tested is the call the entry ends in, and that the offered list holds
# what the description promises.
Feature: activities with no place

  Background:
    Given the save "test-colony" is loaded
    And a colonist "Ada" exists
    And a colonist "Bo" exists
    And game speed is ultrafast
    And I set the hour to 15

  @timeout:900
  Scenario: two colonists go for a walk together
    Then Shared Joys: the activity "GoForWalk" is offered to "Ada"
    When Shared Joys: "Ada" and "Bo" start the activity "GoForWalk"
    Then Shared Joys: "Ada" and "Bo" are on the same activity
    And no errors were logged
