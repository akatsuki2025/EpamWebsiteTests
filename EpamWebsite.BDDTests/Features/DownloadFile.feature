Feature: Download Code of Ethical Conduct PDF

  As an EPAM website user
  I want to download the Code of Ethical Conduct PDF
  So that I can review EPAM's ethical guidelines

  Background:
    Given I am on the EPAM main page

  Scenario Outline: Downloading the Code of Ethical Conduct PDF
    When I click the Code of Ethical Conduct PDF link in the footer
    Then the file "<fileName>" should be downloaded

    Examples:
      | fileName                   |
      | Code-Of-Conduct_01_26.pdf  |