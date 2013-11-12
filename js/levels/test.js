define(['../environment'], function(Environment) {
  var TestLevel = me.ScreenObject.extend({
    init: function() { // Constructor
      var _this = this;
      this.environment = new Environment();
      this.baseHeight = 0;

      me.event.subscribe('/tools/raiseWater', function() {
        _this.environment.waterLevel += 1; // TODO: This could be received as a parameter
        _this.water.updated = true;
      });
    },
    onResetEvent: function() { // Called when the state changes into this screen
      me.levelDirector.loadLevel('testlevel');

      this.water = me.entityPool.newInstanceOf('water', this);
      me.game.world.addChild(this.water);
    },
    waterHeight: function() {
      return this.environment.waterLevel - this.baseHeight;
    }
  });

  return TestLevel;
});
