define(['phaser', 'level', 'character', 'environment'], function(Phaser, Level, Character, Environment) {
  function Context() {
    this.preloadFunctions = [];
    this.createFunctions = [];
    this.updateFunctions = [];
    this.renderFunctions = [];

    this.levels = [];
  }

  Context.prototype.init = function() {
    var _this = this;

    this.GRAVITY = 10;

    var platform;

    this.game = new Phaser.Game(800, 608, Phaser.AUTO, '', {
      preload: preload,
      create: create,
      update: update,
      render: render
    });

    function runFunctions(functionArray, context) {
      var i;
      for (i = 0; i < functionArray.length; i++) {
        functionArray[i].call(_this, context);
      }
    }

    function preload(game) {
      runFunctions(_this.preloadFunctions, game);
    }

    function create(game) {
      runFunctions(_this.createFunctions, game);
    }	
	
    function update(game) {
      runFunctions(_this.updateFunctions, game);
    }

    function render(game) {
      runFunctions(_this.renderFunctions, game);
    }

    this.character = new Character(this);
    this.levels.push(new Level(this));
    this.currentLevel = this.levels[0];
    this.updateFunctions.push(function(game) {
      // Check if character is dying
      if (_this.currentLevel.isUnderWater(_this.character.sprite)) {
        this.currentLevel.restart();
      } 
    });
  };

  return Context;
});