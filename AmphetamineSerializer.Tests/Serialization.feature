Feature: Serialization
	Serialization and deserialization of the same class
	in order to ensure that results are consistent.

@mytag
Scenario: Serializing and deserializing data produce the same result
	Given The instance Instance1 of type Test filled with random data
	And I serialize the instance Instance1 of type Test in d:\test.bin
	When I deserialize the instance Instance2 of type Test from d:\test.bin
	Then Instance1 and Instance2 are identical