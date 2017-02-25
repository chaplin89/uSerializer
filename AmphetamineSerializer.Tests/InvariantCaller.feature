Feature: InvariantCaller
	Invoking a method passing 
	some parameters, no matter their order,
	as long as there's no ambiguity.

@mytag
Scenario: Add two numbers
	Given I have entered 50 into the calculator
	And I have entered 70 into the calculator
	When I press add
	Then the result should be 120 on the screen
