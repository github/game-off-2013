define(['phaser'], function(Phaser) {
  function Game() {}

  var PLAYER_SPEED = 200,
      JUMP_SPEED = -250;

  var game,
      player,
      keys;

  Game.prototype.run = function() {
    game = new Phaser.Game(800, 600, Phaser.AUTO, 'level-prototype', {
      preload: preload,
      create: create,
      update: update
    });

    function preload() {
      game.load.spritesheet('character', '/assets/spy.png', 30, 41, 1);
    }

    function create() {
      player = game.add.sprite(40, 100, 'character');
      player.body.collideWorldBounds = true;
      player.body.gravity.y = 10;

      keys = game.input.keyboard.createCursorKeys();
    }

    function update() {
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
    }
  };

  return Game;
})