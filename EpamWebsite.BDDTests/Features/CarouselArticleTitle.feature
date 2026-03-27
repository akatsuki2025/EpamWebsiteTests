Feature: Carousel article title matches article page title
 
  As an EPAM website user
  I want the carousel article title to match the article page title
  So that I can trust the navigation and content consistency

Background:
    Given I am on the EPAM main page

Scenario Outline: Carousel article title matches article page title after swiping
	Given I navigate to the Insights page
	When I swipe the carousel <swipeCount> times
    And I click the Read More button on the active carousel article
	Then the article title should contain all words from the carousel title

	Examples:
      | swipeCount |
      | 2          |
      | 1          |
      | 0          |
      | 3          |