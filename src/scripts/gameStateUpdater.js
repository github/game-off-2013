define('gameStateUpdater', function() {
    'use strict';
    
    return function(map) {
        this.updateGameState = function(currentState, options) {
            var newYear = currentState.year + 1;
            var newAgricultureLevel = currentState.agricultureLevel + options.agricultureIncrease;

            var newSeaLevel = currentState.seaLevel + currentState.pollution;
            map.updateSeaLevel(newSeaLevel);

            var newLandArea = map.calculateRemainingLandArea();
            var foodProduction = newLandArea * newAgricultureLevel;
            var foodConsumption = currentState.population;

            var newFood, newPopulation, deathsFromStarvation;
            if (foodProduction + currentState.food > foodConsumption) {
                newFood = currentState.food + foodProduction - foodConsumption;
                newPopulation = currentState.population;
                deathsFromStarvation = 0;
            } else {
                newFood = 0;
                deathsFromStarvation = -1 * (currentState.food + foodProduction - foodConsumption);
                newPopulation = currentState.population - deathsFromStarvation;
            }

            var newPollution = currentState.pollution + currentState.agricultureLevel;

            return {
                year: newYear,
                seaLevel: newSeaLevel,
                pollution: newPollution,
                agricultureLevel: newAgricultureLevel,
                population: newPopulation,
                food: newFood,
                deathsFromStarvation: deathsFromStarvation
            };
        };
    };
});