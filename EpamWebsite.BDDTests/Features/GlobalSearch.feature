Feature: Global search

  As an EPAM website user
  I want to use the global search on the main page
  So that I can find relevant information

Background:
    Given I am on the EPAM main page

Scenario Outline: Global search returns results containing the keyword
    When I open the global search
    And I enter "<keyword>" in the global search field
    And I submit the global search
    Then all global search result links should contain the keyword "<keyword>"

    Examples:
      | keyword     |
      | BLOCKCHAIN  |
      | Cloud       |
      | Automation  |
