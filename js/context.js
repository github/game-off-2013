define(['phaser', 'level', 'character'], function(Phaser, Level, Character) {
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

    this.game = new Phaser.Game(800, 600, Phaser.AUTO, '', {
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
      // TMP
      game.load.image('platform', '../assets/water.png');

      runFunctions(_this.preloadFunctions, game);
    }

    function create(game) {
      // TMP
      platform = game.add.sprite(0, 400, 'platform');
      platform.scale.x = 50;
      platform.body.immovable = true;

      runFunctions(_this.createFunctions, game);
    }

    function update(game) {
      runFunctions(_this.updateFunctions, game);
      // TMP
      game.physics.collide(platform, _this.character.sprite);
    }

    function render(game) {
      runFunctions(_this.renderFunctions, game);
    }

    this.levels.push(new Level(this));
    this.character = new Character(this);
    this.currentLevel = this.levels[0];

    this.updateFunctions.push(function(game) {
      // Check if character is dying
      if (_this.currentLevel.isUnderWater(_this.character.sprite)) {
        // The character died, reset or something
      }
    });
  };

  return Context;
});