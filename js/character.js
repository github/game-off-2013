define(['waterTool'], function(WaterTool) {
  var Character = me.ObjectEntity.extend({
    init: function(x, y, settings) {
      this.parent(x, y, settings);
      this.setVelocity(3, 15);
      this.waterTool = new WaterTool();
    },

    update: function() {
      if (me.input.isKeyPressed('left')) {
        this.flipX(true);
        this.vel.x -= this.accel.x * me.timer.tick;
      } else if (me.input.isKeyPressed('right')) {
        this.flipX(false);
        this.vel.x += this.accel.x * me.timer.tick;
      } else {
        this.vel.x = 0;
      }

      if (me.input.isKeyPressed('jump')) {
        if (!this.jumping && !this.falling) {
          this.vel.y = -this.maxVel.y * me.timer.tick;
          this.jumping = true;
        }
      }

      if (me.input.isKeyPressed('waterTool')) {
        this.waterTool.use();
      }

      this.updateMovement();
      return this.vel.x!=0 || this.vel.y!=0;
    }
  });

  return Character;
});
