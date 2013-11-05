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

    var game = new Phaser.Game(800, 600, Phaser.AUTO, '', {
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

    this.levels.push(new Level(this));
    this.character = new Character(this);
  };

  return Context;
});