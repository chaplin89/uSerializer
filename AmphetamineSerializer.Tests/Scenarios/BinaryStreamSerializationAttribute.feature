Feature: BinaryStreamSerializationAttribute
	Serialization and deserialization of the same class
	in order to ensure that results are consistent.
	
Scenario: Serializing and deserializing an element containing 1D arrays data produce the same result
	Given The instance Instance1 of type TestProperty1DArray filled with random data
	And I serialize the instance Instance1 in Stream1
	When I deserialize the instance Instance2 from Stream1
	Then Instance1 and Instance2 are identical

@ignore
Scenario: Serializing and deserializing complex nested types produce the same result
	Given The instance Instance1 of type TestPropertyFull filled with random data
	And I serialize the instance Instance1 in Stream1
	When I deserialize the instance Instance2 from Stream1
	Then Instance1 and Instance2 are identical

@ignore
Scenario: Serializing and deserializing complex nested types with version produce the same result
	Given The instance Instance1 of type TestPropertyFullVersion filled with random data
	And I serialize the instance Instance1 in Stream1
	When I deserialize the instance Instance2 from Stream1
	Then Instance1 and Instance2 are identical

Scenario: Serializing and deserializing containing jagged array produce the same result
	Given The instance Instance1 of type TestPropertyJaggedArray filled with random data
	And I serialize the instance Instance1 in Stream1
	When I deserialize the instance Instance2 from Stream1
	Then Instance1 and Instance2 are identical

Scenario: Serializing and deserializing trivial types produce the same result
	Given The instance Instance1 of type TestPropertyTrivialTypes filled with random data
	And I serialize the instance Instance1 in Stream1
	When I deserialize the instance Instance2 from Stream1
	Then Instance1 and Instance2 are identical