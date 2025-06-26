# language: en

Feature: PIL Login

  @PIl
  Scenario Outline: Verify login to PIL console with valid credential
    Given the base URI is "<URL>"
    When the user login to PIL console with "<ValidEmailId>" and "<ValidPassword>"
    Then the response status code should be 200
    Then the response body should match the expected JSON response from "<expectedJsonFile>"

    Examples:
      | URL | ValidEmailId              | ValidPassword   | expectedJsonFile   |
      | URL | lokeshrsontakke@eaton.com | Krunal@123      | LoginResponse.json |



  @PIl
  Scenario Outline: Verify login to PIL console with invalid credentials
    Given the base URI is "<URL>"
    When the user login to PIL console with "<InvalidEmailid>" and "<InvalidPassword>"
    Then the response status code should be 401
    Then the response message should say "Login failed with given credentials."

    Examples:
      | URL | InvalidEmailid     | InvalidPassword   |
      | URL | Innvalid@eaton.com | Wrongpass@123     |








#  @PIl
#  Scenario Outline: Verify login with invalid credential
#    Given the base URI is "<URL>"
#    When the user sends a POST request for login to "<Endpoints>" with "<InvalidEmailid>" and "<InvalidPassword>"
#    Then the response status code should be 401
#    Then the response body should match the expected response from "<expectedJsonFile>"
#
#    Examples:
#      | URL | Endpoints   | InvalidEmailid     | InvalidPassword   | expectedJsonFile     |
#      | URL | /login-user | Innvalid@eaton.com | Wrongpass@123     | InvalidResponse.json |






  @PIl
  Scenario Outline: Verify login to PIL console with blank credentials
    Given the base URI is "<URL>"
    When the user login to PIL console with "<BlankEmailID>" and "<BlankEmailPassword>"
    Then the response status code should be 401
    Then the response message should say "Missing credentials"

    Examples:
      | URL | BlankEmailID | BlankEmailPassword |
      | URL |              |                    |





    @PIl
    Scenario Outline: Verify user should get Reset password link
    Given the base URI is "<URL>"
    When the user sends a POST request with GraphQL query from "<graphqlFile>" for SendResetPasswordLink to "<emailid>"
    Then the response status code should be 200
    Then the response body should match the expected JSON response from "<expectedJsonFile>"

    Examples:
   | URL | graphqlFile               | emailid                   | expectedJsonFile              |
   | URL | SendResetpassword.graphql | lokeshrsontakke@eaton.com | SendpasswordLinkResponse.json |



    @PIl
    Scenario Outline: Verify sent Reset password link 
    Given the base URI is "<URL>"
    When the user sends a POST request with GraphQL query from "<graphqlFile>" for SendResetPasswordLink to "<emailid>"
    Then the response status code should be 200

    Examples:
   | URL | graphqlFile               | emailid                   | 
   | URL | SendResetpassword.graphql | lokeshrsontakke@eaton.com | 


