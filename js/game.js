define(['resources', 'levels/test'], function(resources, TestLevel) {
  function Game() {
    this.preloadFunctions = [];
    this.createFunctions = [];
    this.updateFunctions = [];
    this.renderFunctions = [];

    this.levels = [];
  }

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

    // Start the game.
    me.state.change(me.state.PLAY);
  };

  // Context.prototype.init = function() {
  //   var _this = this;

  //   this.GRAVITY = 10;

  //   var platform;

  //   this.game = new Phaser.Game(800, 608, Phaser.AUTO, '', {
  //     preload: preload,
  //     create: create,
  //     update: update,
  //     render: render
  //   });

  //   function runFunctions(functionArray, context) {
  //     var i;
  //     for (i = 0; i < functionArray.length; i++) {
  //       functionArray[i].call(_this, context);
  //     }
  //   }

  //   function preload(game) {
  //     runFunctions(_this.preloadFunctions, game);
  //   }

  //   function create(game) {
  //     runFunctions(_this.createFunctions, game);
  //   }

  //   function update(game) {
  //     runFunctions(_this.updateFunctions, game);
  //   }

  //   function render(game) {
  //     runFunctions(_this.renderFunctions, game);
  //   }

  //   this.character = new Character(this);
  //   this.levels.push(new Level(this));
  //   this.currentLevel = this.levels[0];
  //   this.updateFunctions.push(function(game) {
  //     // Check if character is dying
  //     if (_this.currentLevel.isUnderWater(_this.character.sprite)) {
  //       this.currentLevel.restart();
  //     }
  //   });
  // };

  return Game;
});