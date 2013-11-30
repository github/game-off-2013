define(function (require) {

    var FacilityList = require('facilityList');

    var availableFacilities = {
        'Farm': {
            name: 'Farm',
            landCost: 2,
            buildDuration: 1,
            buildDelta: {
                energy: -16,
                pollution: 32,
                food: 0
            },
            normalDelta: {
                energy: -2,
                pollution: 16,
                food: 32
            }
        },
        'Coal Power Plant': {
            name: 'Coal Power Plant',
            landCost: 4,
            buildDuration: 3,
            buildDelta: {
                energy: -16,
                pollution: 80,
                food: 0
            },
            normalDelta: {
                energy: 128,
                pollution: 64,
                food: 0
            }
        },
        'Pollution Sponge': {
            name: 'Pollution Sponge',
            landCost: 7,
            buildDuration: 3,
            buildDelta: {
                energy: -16,
                pollution: 50,
                food: 0
            },
            normalDelta: {
                energy: 64,
                pollution: -32,
                food: 0
            }
        }
    };

    describe('facility list', function() {

        describe('addFacility', function() {
            it('adds a facility', function() {
                // Arrange
                var facilityList = new FacilityList(availableFacilities);
                var farm = availableFacilities['Farm'];

                // Act
                facilityList.addFacility('Farm', 1);
                var resultCount = facilityList.getFacilityCount();
                var result = facilityList.getFacility(0);

                // Assert
                expect(resultCount).toEqual(1);
                expect(result).toEqual(farm);
            });

            it("doesn't add a facility if there is insufficient energy", function () {
                // Arrange

                // Act

                // Assert
            });

            it("doesn't add a facility if there is insufficient land");
        });

        describe('removeFacility', function() {
           it('removes a facility', function() {
               // Arrange
               var facilityList = new FacilityList(availableFacilities);
               facilityList.addFacility('Farm');
               facilityList.addFacility('Coal Power Plant');
               var farm = facilityList.getFacility(0);
               var powerPlant = facilityList.getFacility(1);

               // Act
               facilityList.removeFacility(farm);
               var resultCount = facilityList.getFacilityCount();
               var result = facilityList.getFacility(0);

               // Assert
               expect(resultCount).toEqual(1);
               expect(result).toEqual(powerPlant);
           });
        });

        describe('update', function() {
            it('returns buildable land area');
            it('returns pollution delta');
            it('returns food delta');
            it('updates remaining energy');
            it('completes construction of facilities');
        });

//        describe('annualPollutionDifference', function() {
//            it('returns sum of pollution generated by each facility', function() {
//                // Arrange
//                var facilityList = new FacilityList(availableFacilities);
//                var farm = availableFacilities['Farm'];
//                var powerPlant = availableFacilities['Coal Power Plant'];
//                var sponge = availableFacilities['Pollution Sponge'];
//                facilityList.addFacility('Farm');
//                facilityList.addFacility('Coal Power Plant');
//                facilityList.addFacility('Pollution sponge');
//
//                // Act
//                var pollutionDifference = facilityList.getPollutionDelta();
//
//                // Assert
//                expect(pollutionDifference).toBe(48);
//            });
//        });

    });
});
