define('gameStateUpdater', function() {
    'use strict';
    
    return function(map) {
        this.updateGameState = function(currentState) {
            var newYear = incrementYear();
            var newSeaLevel = updateSeaLevel();
            var newLandArea = map.calculateRemainingLandArea();
            var newPollution = updatePollution();
            var newFood = null;
            var newPopulation = currentState.population;
            updateFoodStarvingPeopleIfNecessary();

            return {
                year: newYear,
                seaLevel: newSeaLevel,
                pollution: newPollution,
                food: newFood,
                population: newPopulation
            };

            function incrementYear() {
                return currentState.year + 1;
            }

            function updateSeaLevel() {
                var updatedSeaLevel = currentState.seaLevel + currentState.pollution;
                map.updateSeaLevel(updatedSeaLevel);
                return updatedSeaLevel;
            }

            function updatePollution() {
                return currentState.pollution - calculatePollutionAbsorbedByForests() +
                    getPollutionProducedByFacilities();
            
                function calculatePollutionAbsorbedByForests() {
                    return newLandArea * 0.0001;
                }

                // can be negative because facilities can also reduce pollution
                function getPollutionProducedByFacilities() {
                    return 0;
                }
            }

            function updateFoodStarvingPeopleIfNecessary() {
                if ( peopleWillStarve() ) {
                    newFood = 0;
                    var foodDeficit = calculateFoodConsumedByPopulation() -
                        (currentState.food + getFoodProducedByFacilities() );
                    newPopulation = currentState.population - foodDeficit;
                }
                else {
                    newFood = currentState.food + getFoodProducedByFacilities() -
                    calculateFoodConsumedByPopulation();
                }

                function getFoodProducedByFacilities() {
                    return 0;
                }

                function calculateFoodConsumedByPopulation() {
                    return currentState.population * 0.1;
                }

                function peopleWillStarve() {
                    return ( currentState.food + getFoodProducedByFacilities() ) <
                        calculateFoodConsumedByPopulation();
                }
            }

        };
    };
});