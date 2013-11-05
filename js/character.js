define(['phaser', 'waterTool'], function(Phaser, WaterTool) {
  function Character(context) {
    var _this = this;

    this.waterTool = new WaterTool();

    context.preloadFunctions.push(function(game) {
      game.load.image('character', '../assets/verdure.png');
    });

    context.createFunctions.push(function(game) {
      _this.sprite = game.add.sprite(40, 100, 'character');
      _this.sprite.body.collideWorldBounds = true;
      _this.sprite.body.gravity.y = context.GRAVITY;

      _this.keys = game.input.keyboard.createCursorKeys();
      _this.keys.waterTool = game.input.keyboard.addKey(Phaser.Keyboard.A);
    });

    context.updateFunctions.push(function(game) {
      // Movement
      if (_this.keys.left.isDown) {
        _this.sprite.body.velocity.x = Character.PLAYER_SPEED * -1;
      } else if (_this.keys.right.isDown) {
        _this.sprite.body.velocity.x = Character.PLAYER_SPEED;
      } else {
        _this.sprite.body.velocity.x = 0;
      }

      // Why doesn't _this.sprite.body.touching.down works?
      if (_this.sprite.body.bottom == game.world.height && _this.keys.up.isDown) {
        _this.sprite.body.velocity.y = Character.JUMP_SPEED;
      }

      // Tools
      if (_this.keys.waterTool.isDown) {
        _this.waterTool.use();
      }
    });
  }

  Character.PLAYER_SPEED = 200;
  Character.JUMP_SPEED = -250;

  return Character;
});