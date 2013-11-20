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

        it ('increases pollution based on facilities', function() {
            throw new Error('Test not implemented');
        });

        //Assuming all land is forest
        it ('decreases pollution based on land area', function () {
            throw new Error('Test not implemented');
        });

        it ('increases food based on facilities and land area', function() {
            throw new Error('Test not implemented');
        });

        it ('decreases food based on population', function() {
            throw new Error('Test not implemented');
        });

        it('prevents food from becoming negative but starves people instead', function() {
            throw new Error('Test not implemented');
        });

        it('increases the population if not limited by food or land area', function() {
            throw new Error('Test not implemented');
        });

        it('limits/reduces the population if insufficient food', function() {
            throw new Error('Test not implemented');
        });

        it('limits/reduces the population if insufficient land area', function() {
            throw new Error('Test not implemented');
        });

        it ('updates energy based on facility production/consumption', function () {
            throw new Error('Test not implemented');
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