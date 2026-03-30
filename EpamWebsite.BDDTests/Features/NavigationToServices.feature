Feature: Navigation to services section

  As an EPAM website user
  I want to navigate to a specific service category from the main navigation menu
  So that I can view information about the service and related expertise

Background: 
    Given I am on the EPAM main page

Scenario Outline: Validate navigation to Services section and related expertise
	When I hover over the Services menu item in the main navigation
	And I select the "<ServiceCategory>" category from the dropdown
    Then the page contains the "<ServiceCategory>" title
    And the Our Related Expertise section is displayed
        
    Examples:
      | ServiceCategory   |
      | Generative AI     |
      | Responsible AI    |