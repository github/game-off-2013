define(function (require) {
    'use strict';

    var GameStateUpdater = require('gameStateUpdater');
    var gameStateUpdater;

    var mockMap = {
        updateSeaLevel: function() {},
        calculateRemainingLandArea: function () {}
    };

    describe('game state updater', function() {

        beforeEach(function() {
            gameStateUpdater = new GameStateUpdater(mockMap);
        });

        it('increases sea level based on pollution', function() {
            // Arrange
            var currentSeaLevel = 10;
            var currentPollution = 5;
            var currentState = {
                seaLevel: currentSeaLevel,
                pollution: currentPollution
            };

            // Act
            var nextState = gameStateUpdater.updateGameState(currentState, {});

            // Assert
            expect(nextState.seaLevel).toBe(currentSeaLevel + currentPollution);
        });

        it ('updates map sea level with current sea level', function() {
            // Arrange
            var currentSeaLevel = 10;
            var currentPollution = 5;
            var currentState = {
                seaLevel: currentSeaLevel,
                pollution: currentPollution
            };

            var updatedSeaLevel = null;
            mockMap.updateSeaLevel = function(newSeaLevel) {
                updatedSeaLevel = newSeaLevel;
            };

            // Act
            var nextState = gameStateUpdater.updateGameState(currentState, {});

            // Assert
            expect(updatedSeaLevel).toBe(currentSeaLevel + currentPollution);
        });

        it ('increases pollution based on agriculture level', function() {
            // Arrange
            var currentPollution = 10;
            var currentAgricultureLevel = 5;
            var currentState = {
                pollution: currentPollution,
                agricultureLevel: currentAgricultureLevel
            };

            // Act
            var nextState = gameStateUpdater.updateGameState(currentState, {});

            // Assert
            expect(nextState.pollution).toBe(currentPollution + currentAgricultureLevel);
        });

        it ('increases agriculture level based on options', function() {
            // Arrange
            var currentAgricultureLevel = 12;
            var agricultureIncrease = 1;
            var currentState = {
                agricultureLevel: currentAgricultureLevel
            };
            var options = {
                agricultureIncrease: agricultureIncrease
            };

            // Act
            var nextState = gameStateUpdater.updateGameState(currentState, options);

            // Assert
            expect(nextState.agricultureLevel).toBe(currentAgricultureLevel + agricultureIncrease);
        });

        it ('increases food based on agriculture level, land area and population', function() {
            // Arrange
            var currentFood = 500;
            var currentAgricultureLevel = 20;
            var currentPopulation = 2000;
            var agricultureIncrease = 1;
            var currentState = {
                food: currentFood,
                agricultureLevel: currentAgricultureLevel,
                population: currentPopulation
            };
            var options = {
                agricultureIncrease: agricultureIncrease
            };

            var newLandArea = 100;
            var wasSeaLevelUpdated = false;
            mockMap.updateSeaLevel = function() {
                wasSeaLevelUpdated = true;
            };
            mockMap.calculateRemainingLandArea = function() {
                if (wasSeaLevelUpdated) {
                    return newLandArea;
                }
            };

            // Act
            var nextState = gameStateUpdater.updateGameState(currentState, options);

            // Assert
            expect(nextState.food).toBe(currentFood + ((currentAgricultureLevel + agricultureIncrease) * newLandArea) - currentPopulation);
            expect(nextState.population).toBe(currentPopulation);
            expect(nextState.deathsFromStarvation).toBe(0);
        });

        it('prevents food from becoming negative but starves people instead', function() {
            // Arrange
            var currentFood = 500;
            var currentAgricultureLevel = 10;
            var currentPopulation = 2000;
            var agricultureIncrease = 1;
            var currentState = {
                food: currentFood,
                agricultureLevel: currentAgricultureLevel,
                population: currentPopulation
            };
            var options = {
                agricultureIncrease: agricultureIncrease
            };

            var newLandArea = 100;
            mockMap.calculateRemainingLandArea = function() {
                return newLandArea;
            };

            // Act
            var nextState = gameStateUpdater.updateGameState(currentState, options);

            // Assert
            var foodDeficit = -1 * (currentFood + ((currentAgricultureLevel + agricultureIncrease) * newLandArea) - currentPopulation);
            expect(foodDeficit).toBeGreaterThan(0);
            expect(nextState.food).toBe(0);
            expect(nextState.population).toBe(currentPopulation - foodDeficit);
            expect(nextState.deathsFromStarvation).toBe(foodDeficit);
        });

        it('increments the year', function() {
            // Arrange
            var currentYear = 2020;
            var currentState = {
                year: currentYear
            };

            // Act
            var nextState = gameStateUpdater.updateGameState(currentState, {});

            // Assert
            expect(nextState.year).toBe(currentYear + 1);
        })
    });
});