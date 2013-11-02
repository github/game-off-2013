define(['phaser'], function(Phaser) {
  var waterLevelLine,
      keys;

  function Level(context) {
    var _this = this

    this.context = context;

    this.waterLevel = 0.5;

    context.createFunctions.push(function(game) {
      keys = {
        raise: game.input.keyboard.addKey(Phaser.Keyboard.A),
        lower: game.input.keyboard.addKey(Phaser.Keyboard.Z)
      };
    });

    context.renderFunctions.push(function(game) {
      if (waterLevelLine) {
        waterLevelLine.clear();
      }

      waterLevelLine = game.add.graphics(0, _this.waterLevel * game.world.height);
      waterLevelLine.beginFill(0x0000FF);
      waterLevelLine.lineStyle(1, 0x0000FF, 1);
      waterLevelLine.moveTo(0, _this.waterLevel * game.world.height);
      waterLevelLine.lineTo(game.world.width, _this.waterLevel * game.world.height);
      waterLevelLine.endFill();

      if (keys.raise.isDown) {
        _this.raiseWater(0.01);
      } else if (keys.lower.isDown) {
        _this.lowerWater(0.01);
      }
    });
  }

  Level.prototype.raiseWater = function(amount) {
    this.setWaterLevel(this.waterLevel + amount);
    this.waterLevel = this.waterLevel + amount > 1 ? 1 : this.waterLevel + amount;
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