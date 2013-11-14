define(function (require) {

    var Game = require('game');

    describe('game', function() {
        it('initializes state from initial state', function() {
            // Arrange
            var initialState = {};
            var game = new Game(initialState);

            // Act
            var currentState = game.state;

            // Assert
            expect(currentState).toBe(initialState);
        });

        it('updates game state when game is updated', function() {
            // Arrange
            var initialState = {};
            var nextState = {};
            var gameStateUpdater = {
                updateGameState: function(currentState) {
                    if (currentState === initialState) {
                        return nextState;
                    } else {
                        return null;
                    }
                }
            };

            var game = new Game(initialState, gameStateUpdater);

            // Act
            game.update();

            // Assert
            expect(game.state).toBe(nextState);
        });

        it('updates game state with options', function() {
            // Arrange
            var optionsUsedForUpdate = null;
            var gameStateUpdater = {
                updateGameState: function(currentState, options) {
                    optionsUsedForUpdate = options;
                }
            };

            var game = new Game({}, gameStateUpdater);
            var options = {};

            // Act
            game.update(options);

            // Assert
            expect(optionsUsedForUpdate).toBe(options);
        })
    });

});