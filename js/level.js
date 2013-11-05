define(['phaser'], function(Phaser) {
  var water,
      waterTop,
      keys;

  function Level(context) {
    var _this = this

    this.context = context;
    this.waterLevel = 0.5;

    context.preloadFunctions.push(function(game) {
      game.load.image('water', '../assets/water.png');
    });

    context.createFunctions.push(function(game) {
      water = game.add.sprite(0, game.world.height * (1 - _this.waterLevel), 'water');
      water.scale.x = game.world.width;

      keys = {
        raise: game.input.keyboard.addKey(Phaser.Keyboard.A),
        lower: game.input.keyboard.addKey(Phaser.Keyboard.Z)
      };
    });

    context.updateFunctions.push(function(game) {
      if (keys.raise.isDown) {
        _this.raiseWater(0.01);
      } else if (keys.lower.isDown) {
        _this.lowerWater(0.01);
      }
    });

    context.renderFunctions.push(function(game) {
      water.bringToTop(); // Probably don't needed if we group the sprites correctly
      water.y = game.world.height * (1 - _this.waterLevel);
      water.scale.y = game.world.height * _this.waterLevel;
    });
  }

  Level.prototype.raiseWater = function(amount) {
    this.setWaterLevel(this.waterLevel + amount);
  };

  Level.prototype.lowerWater = function(amount) {
    this.setWaterLevel(this.waterLevel - amount);
  };

  Level.prototype.setWaterLevel = function(value) {
    if (value > 1) {
      this.waterLevel = 1;
    } else if (value < 0) {
      this.waterLevel = 0;
    } else {
      this.waterLevel = value;
    }
  }

  return Level;
});