define(['resources', 'levels/test', 'character', 'water'], function(resources, TestLevel, Character, Water) {
  function Game() { }

  Game.prototype.run = function() {
    if (!me.video.init('screen', 640, 480, true, 'auto')) {
      alert('Your browser does not support HTML5 canvas.');
      return;
    }

    if (document.location.hash === "#debug") {
      window.onReady(function () {
        me.plugin.register.defer(debugPanel, "debug");
      });
    }

    // Callback when everything is loaded
    me.loader.onload = this.loaded;

    // Load the resources
    me.loader.preload(resources);

    // Initialize melonJS and display a loading screen.
    me.state.change(me.state.LOADING);
  };

  Game.prototype.loaded = function() {
    me.state.set(me.state.PLAY, new TestLevel());

    me.entityPool.add('water', Water);
    me.entityPool.add('character', Character);

    me.input.bindKey(me.input.KEY.LEFT, 'left');
    me.input.bindKey(me.input.KEY.RIGHT, 'right');
    me.input.bindKey(me.input.KEY.UP, 'jump', true);

    // Start the game.
    me.state.change(me.state.PLAY);
  };

  return Game;
});