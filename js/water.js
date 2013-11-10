define([], function() {
  var Water = me.ObjectEntity.extend({
    init: function(x, y, level) {
      this.parent(x, y, {image: 'water'});
      this.level = level;
      this.renderable.anchorPoint = new me.Vector2d(0, 1);
      this.z = 1000;
    },
    draw: function(context) {
      // When raising the water, it's choppy UNLESS the character
      // is moving, so there could be a problem here.
      this.renderable.scale.x = me.game.world.width;
      this.renderable.scale.y = this.level.waterHeight();
      this.renderable.scaleFlag = true;
      this.parent(context);
    },
    raise: function(amount) {
      this.level =+ amount;
    }
  });

  return Water;
});
