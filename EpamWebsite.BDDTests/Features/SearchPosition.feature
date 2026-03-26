Feature: Search for job position

  As a job seeker
  I want to search for job positions by keyword, location, and workplace type
  So that I can find relevant job opportunities

Background:
    Given I am on the EPAM main page

Scenario Outline: Search for a job position and verify the result contains the keyword
	Given I navigate to the Careers page
	And I start a job search
	When I enter "<keyword>" as the job keyword
	And I select "<location>" as the location
	And I select "<workplaceType>" as the workplace type
	And I perform the job search
	Then the last job card should contain the keyword "<keyword>"

	Examples: 
	      | keyword | location      | workplaceType |
	      | Java    | All Locations | Remote        |
	      | Python  | Croatia       | Office        | 
