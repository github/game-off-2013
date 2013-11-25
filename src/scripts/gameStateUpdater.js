define('gameStateUpdater', function() {
    'use strict';
    
    return function(map) {
        this.updateGameState = function(currentState) {
            var newYear = incrementYear();
            var newSeaLevel = updateSeaLevel();
            var newLandArea = map.calculateRemainingLandArea();
            var newPollution = updatePollution();

            return {
                year: newYear,
                seaLevel: newSeaLevel,
                pollution: newPollution
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
                return currentState.pollution - calculateModulusPollutionDecreaseFromForests() + getPollutionDeltaFromFacilities();
            
                function calculateModulusPollutionDecreaseFromForests() {
                    return newLandArea * 0.00001;
                }

                function getPollutionDeltaFromFacilities() {
                    return 0;
                }

            }

        };
    };
});