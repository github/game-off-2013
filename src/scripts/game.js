define('game', function() {
    return function(initialState, gameStateUpdater) {
        this.state = initialState;

        this.update = function(options) {
            this.state = gameStateUpdater.updateGameState(this.state, options);
        };
    }
})