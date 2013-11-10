define(['../environment'], function(Environment) {
  var TestLevel = me.ScreenObject.extend({
    init: function() { // Constructor
      this.environment = new Environment();
      this.baseHeight = 0;
    },
    onResetEvent: function() { // Called when the state changes into this screen
      me.levelDirector.loadLevel('testlevel');

      this.water = me.entityPool.newInstanceOf('water', 1, me.game.world.height - 1, this);
      me.game.world.addChild(this.water);
    },
    waterHeight: function() {
      return this.environment.waterLevel - this.baseHeight;
    }
  });

  return TestLevel;
});

// define(['phaser', 'environment'], function(Phaser, Environment) {
//   var water,
//       waterTop;

//   var map;
//   var tileset;
//   var layer;

//   function Level(context) {
//     var _this = this

//     this.context = context;
//     this.heightAboveWater = 0;
//     this.playerStart = {
//       x: 160,
//       y: 100
//     };

//     this.restart = function() {
//       var characterSprite = this.context.character.sprite;
//       this.environment = new Environment();
//       characterSprite.reset(this.playerStart.x, this.playerStart.y);
//     }

//     this.waterHeight = function() {
//       return this.environment.waterLevel - this.heightAboveWater;
//     };

//     this.isUnderWater = function(sprite) {
//       return sprite.body.y > context.game.world.height - this.waterHeight();
//     };

//     context.preloadFunctions.push(function(game) {
//       game.load.image('water', 'assets/water.png');
//      game.load.tilemap('testtile', 'assets/test1.json', null, Phaser.Tilemap.TILED_JSON);
//    game.load.tileset('tileTiles', 'assets/tile.png',32,32);
//     });

//     context.renderFunctions.push(function(game) {
//       water.bringToTop(); // Probably don't needed if we group the sprites correctly
//       water.y = game.world.height - _this.waterHeight();
//       water.scale.y = _this.waterHeight();
//     });
//   }

//   return Level;
// });