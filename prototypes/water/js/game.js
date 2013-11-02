define(['phaser', 'level'], function(Phaser, Level) {
  function Game() {
    this.preloadFunctions = [];
    this.createFunctions = [];
    this.updateFunctions = [];
    this.renderFunctions = [];

    this.levels = [];
  }

  var PLAYER_SPEED = 200,
      JUMP_SPEED = -250;

  var game,
      player,
      keys;

  Game.prototype.run = function() {
    var _this = this;

    game = new Phaser.Game(800, 600, Phaser.AUTO, 'level-prototype', {
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

    this.levels.push(new Level(_this));

    this.preloadFunctions.push(function() {
      game.load.spritesheet('character', '../../assets/spy.png', 30, 41, 1);
    });

    this.createFunctions.push(function() {
      player = game.add.sprite(40, 100, 'character');
      player.body.collideWorldBounds = true;
      player.body.gravity.y = 10;

      keys = game.input.keyboard.createCursorKeys();
    });

    this.updateFunctions.push(function() {
      if (keys.left.isDown) {
        player.body.velocity.x = PLAYER_SPEED * -1;
      } else if (keys.right.isDown) {
        player.body.velocity.x = PLAYER_SPEED;
      } else {
        player.body.velocity.x = 0;
      }

      // Why doesn't player.body.touching.down works?
      if (player.body.bottom == game.world.height && keys.up.isDown) {
        player.body.velocity.y = JUMP_SPEED;
      }
    });
  };

  return Game;
})