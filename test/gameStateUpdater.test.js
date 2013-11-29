define(function (require) {
    'use strict';

    var GameStateUpdater = require('gameStateUpdater');
    var gameStateUpdater;

    var mockMap = {
        updateSeaLevel: function() {},
        calculateRemainingLandArea: function () {}
    };

    var mockFacilityList;

    var facilityStub = {
                buildableLandArea: 450000,
                pollutionDelta: 0,
                foodDelta: 0
        };

    describe('game state updater', function() {

        beforeEach(function() {
            mockFacilityList = jasmine.createSpyObj('facilityList', ['update']);
            mockFacilityList.update.andReturn(facilityStub);

            gameStateUpdater = new GameStateUpdater(mockMap, mockFacilityList);

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
            var nextState = gameStateUpdater.updateGameState(currentState);

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
            var nextState = gameStateUpdater.updateGameState(currentState);

            // Assert
            expect(updatedSeaLevel).toBe(currentSeaLevel + currentPollution);
        });

        it ('increases pollution based on facilities', function() {
            // Arrange
            var currentPollution = 500;

            var currentState = {
                pollution: currentPollution
            };

            var facilityStub = {
                buildableLandArea: 0,
                pollutionDelta: 50,
                foodDelta: 0
            };
            
            mockFacilityList.update.andReturn(facilityStub);

            // Act
            var nextState = gameStateUpdater.updateGameState(currentState);

            // Assert
            expect(nextState.pollution).toBe(currentPollution + facilityStub.pollutionDelta);
        });

        // Assuming all land is forest except for that used by facilities
        it ('decreases pollution based on land area', function () {
            // Arrange
            var currentPollution = 500;

            var currentState = {
                pollution: currentPollution
            };

            var newUnfloodedLandArea = 500;
            mockMap.calculateRemainingLandArea = function() {
                return newUnfloodedLandArea;
            };

            // Act
            var nextState = gameStateUpdater.updateGameState(currentState);

            // Assert
            expect(nextState.pollution).toBeLessThan(currentPollution);
        });

        xit ('increases food based on facilities', function() {
            throw new Error('Test not implemented');
        });

        it ('decreases food based on population', function() {
            // Arrange
            var currentFood = 500;
            var currentPopulation = 200;

            var currentState = {
                food: currentFood,
                population: currentPopulation
            };

            //Act
            var nextState = gameStateUpdater.updateGameState(currentState);

            // Assert
            expect(nextState.food).toBeLessThan(currentFood);
        });

        it('prevents food from becoming negative but starves people instead', function() {
            // Arrange
            var currentFood = 500;
            var currentPopulation = 20000;

            var currentState = {
                food: currentFood,
                population: currentPopulation
            };

            // Act
            var nextState = gameStateUpdater.updateGameState(currentState);

            // Assert
            expect(nextState.food).toBe(0);
            expect(nextState.population).toBeLessThan(currentPopulation);
        });

        xit('increases the population if not limited by food or land area', function() {
            throw new Error('Test not implemented');
        });

        xit('limits/reduces the population if insufficient land area', function() {
            throw new Error('Test not implemented');
        });

        it('increments the tick', function() {
            // Arrange
            var currentTick = 5;
            var currentState = {
                tick: currentTick
            };

            // Act
            var nextState = gameStateUpdater.updateGameState(currentState);

            // Assert
            expect(nextState.tick).toBe(currentTick + 1);
        })

        xit('updates the facilities module with the total land area', function() {
            throw new Error('Test not implemented');
        });
    });
});