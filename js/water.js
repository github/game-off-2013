define([], function() {
  var Water = me.ObjectEntity.extend({
    init: function(x, y) {
      this.parent(x, y, {image: 'water'});
      this.level = 200;
      this.renderable.anchorPoint = new me.Vector2d(0, 1);
      this.z = 1000;
    },
    draw: function(context) {
      this.renderable.scale.x = me.game.world.width;
      this.renderable.scale.y = this.level;
      this.renderable.scaleFlag = true;
      this.parent(context);
    },
    raise: function(amount) {
      this.level =+ amount;
    }
  });

  return Water;
});
