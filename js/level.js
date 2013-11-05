define(['phaser', 'environment'], function(Phaser, Environment) {
  var water,
      waterTop,
      keys;

  function Level(context) {
    var _this = this

    this.environment = new Environment();
    this.context = context;
    this.heightAboveWater = 0;

    this.waterHeight = function() {
      return this.environment.waterLevel - this.heightAboveWater;
    }

    context.preloadFunctions.push(function(game) {
      game.load.image('water', '../assets/water.png');
    });

    context.createFunctions.push(function(game) {
      water = game.add.sprite(0, game.world.height - _this.waterHeight(), 'water');
      water.scale.x = game.world.width;

      keys = {
        raise: game.input.keyboard.addKey(Phaser.Keyboard.A),
        lower: game.input.keyboard.addKey(Phaser.Keyboard.Z)
      };
    });

    context.updateFunctions.push(function(game) {
      if (keys.raise.isDown) {
        _this.raiseWater(1);
      } else if (keys.lower.isDown) {
        _this.lowerWater(1);
      }
    });

    context.renderFunctions.push(function(game) {
      water.bringToTop(); // Probably don't needed if we group the sprites correctly
      water.y = game.world.height - _this.waterHeight();
      water.scale.y = _this.waterHeight();
    });
  }

  // TODO: This will be done by the weapon
  Level.prototype.raiseWater = function(amount) {
    this.environment.waterLevel += amount;
  };

  Level.prototype.lowerWater = function(amount) {
    this.environment.waterLevel -= amount;
  };

  return Level;
});