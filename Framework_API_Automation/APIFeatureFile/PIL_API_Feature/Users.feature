Feature: users

A short summary of the feature

# language: en

  @PIl
  Scenario Outline:Verify Create a new publisher user using GraphQL query and delete it
    Given the base graphql URI is "<URL>"
    When the user sends a POST request with GraphQL query from "<graphqlFile>" and variables: firstName="<firstName>", lastName="<lastName>", email="<email>", role="<role>"
    Then the response status code should be 200
    Then the response body should match the expected JSON response from "<expectedUserResponseFile>"
    When the user sends a POST request with GraphQL query for delete a user
    Then the response status code should be 200
    Then the response body should match the expected response from "<expectedDeleteResponseFile>"


    Examples:
    | URL | graphqlFile          | firstName | lastName | email             | role      | expectedUserResponseFile   | expectedDeleteResponseFile   |
    | URL | AddUserQuery.graphql | TestAPI   | TestAPI  | TestAPI@gmail.com | PUBLISHER | AddUserResponse.json       | DeleteUserResponse.json      |


  @PIl
  Scenario Outline:Verify Create a new ADMIN user using GraphQL query and delete it
    Given the base graphql URI is "<URL>"
    When the user sends a POST request with GraphQL query for create a admin user from "<graphqlFile>" and variables: firstName="<firstName>", lastName="<lastName>", email="<email>", role="<role>"
    Then the response status code should be 200
    Then the response body should match the expected JSON response from "<expectedUserResponseFile>"
    When the user sends a POST request with GraphQL query for delete a admin user
    Then the response status code should be 200
    Then the response body should match the expected response from "<expectedDeleteResponseFile>"


    Examples:
    | URL | graphqlFile          | firstName      | lastName      | email                  | role  | expectedUserResponseFile   | expectedDeleteResponseFile   |
    | URL | AddUserQuery.graphql | TestAPIAdmin   | TestAPIAdmin  | TestAPIAdmin@gmail.com | ADMIN | AddUserResponse.json       | DeleteUserResponse.json      |

 
 @PIl
  Scenario Outline:Verify system prevents creation of new user account matching existing user
    Given the base graphql URI is "<URL>"
    When the user sends a POST request with GraphQL query from "<graphqlFile>" and variables: firstName="<firstName>", lastName="<lastName>", email="<email>", role="<role>"
    Then the response status code should be 200
    Then the response body should match the expected JSON response from "<expectedUserResponseFile>"


    Examples:
    | URL | graphqlFile          | firstName   | lastName | email                     | role  | expectedUserResponseFile   | 
    | URL | AddUserQuery.graphql | Lokesh      | Sontake  | lokeshrsontakke@eaton.com | ADMIN | AddSameUserResponse.json   |
