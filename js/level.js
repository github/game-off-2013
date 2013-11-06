define(['phaser', 'environment'], function(Phaser, Environment) {
  var water,
      waterTop;

  function Level(context) {
    var _this = this

    this.environment = new Environment();
    this.context = context;
    this.heightAboveWater = 0;

    this.waterHeight = function() {
      return this.environment.waterLevel - this.heightAboveWater;
    }

    this.isUnderWater = function(sprite) {
      return sprite.body.y > context.game.world.height - this.waterHeight();
    }

    context.preloadFunctions.push(function(game) {
      game.load.image('water', '../assets/water.png');
    });

    context.createFunctions.push(function(game) {
      water = game.add.sprite(0, game.world.height - _this.waterHeight(), 'water');
      water.scale.x = game.world.width; // This could be wrong (5px * width)
    });

    context.renderFunctions.push(function(game) {
      water.bringToTop(); // Probably don't needed if we group the sprites correctly
      water.y = game.world.height - _this.waterHeight();
      water.scale.y = _this.waterHeight();
    });
  }

  return Level;
});