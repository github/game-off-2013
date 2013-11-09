define(['../environment'], function(Environment) {
  var TestLevel = me.ScreenObject.extend({
    onResetEvent: function() { // Called when the state changes into this screen
      me.levelDirector.loadLevel('testlevel');
    },
    onDestroyEvent: function() {} // Called when the state leaves this screen
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

//     context.createFunctions.push(function(game) {
//       _this.restart();
//    map = game.add.tilemap('testtile');
//    tileset = game.add.tileset('tileTiles');
//       tileset.setCollisionRange(0, tileset.total - 1, true, true, true, true);
//    layer = game.add.tilemapLayer(0, 0, map.layers[0].width*tileset.tileWidth, 600, tileset, map, 0);
//       layer.resizeWorld();
//       water = game.add.sprite(0, game.world.height - _this.waterHeight(), 'water');
//       water.scale.x = game.world.width; // This could be wrong (5px * width)
//     });

//  context.updateFunctions.push(function(game) {
//    game.physics.collide(_this.context.character.sprite, layer);
//  });

//     context.renderFunctions.push(function(game) {
//       water.bringToTop(); // Probably don't needed if we group the sprites correctly
//       water.y = game.world.height - _this.waterHeight();
//       water.scale.y = _this.waterHeight();
//     });
//   }

//   return Level;
// });