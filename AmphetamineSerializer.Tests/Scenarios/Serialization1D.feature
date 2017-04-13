﻿Feature: Serialization1D
	Serialization and deserialization of the same class
	in order to ensure that results are consistent.
	
@mytag
Scenario: Serializing and deserializing an element containing 1D arrays data produce the same result
	Given The instance Instance1 of type Test1DArray filled with random data
	And I serialize the instance Instance1 in Stream1
	When I deserialize the instance Instance2 from Stream1
	Then Instance1 and Instance2 are identical