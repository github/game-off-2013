define('game', function() {
    'use strict';
    
    return function(initialState, gameStateUpdater) {
        this.state = initialState;

        this.update = function() {
            this.state = gameStateUpdater.updateGameState(this.state);
        };
    };
});