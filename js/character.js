define(['waterTool'], function(WaterTool) {
  var Character = me.ObjectEntity.extend({
    init: function(x, y, settings) {
      this.parent(x, y, settings);
      this.setVelocity(3, 15);
      this.updateColRect(8, 10, -1, 5);        
      this.waterTool = new WaterTool();

      //  Add animation sets
      this.renderable.addAnimation('anStill', [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 3, 4, 5]);
      this.renderable.addAnimation('anRight', [6, 7, 8, 9, 10, 11]);
      this.renderable.addAnimation('anJump', [12, 13, 14, 15, 16, 17]);
                
      this.direction = 'right';

      // We need it so when the character falls too quickly,
      // the death by water check can still be done.
      this.alwaysUpdate = true;
    },
    updateAnimation: function(){
        if(this.vel.x != 0){
            if( this.direction == 'right' && !this.renderable.isCurrentAnimation('anRight')){
                this.renderable.setCurrentAnimation('anRight');
                this.flipX(false);
            } else if( this.direction == 'left' && !this.renderable.isCurrentAnimation('anRight')){
                this.renderable.setCurrentAnimation('anRight');
                this.flipX(true);
            }
        } else if (!this.renderable.isCurrentAnimation('anStill')){
            this.renderable.setCurrentAnimation('anStill');
        }
        
       if (this.jumping){
       this.renderable.setCurrentAnimation('anJump');
    }},
    update: function() {
      if (this.isDead()) {
        me.state.current().reset();
        return;
      }

      if (me.input.isKeyPressed('left')) {
        this.direction = 'left';
        this.vel.x -= this.accel.x * me.timer.tick;
      } else if (me.input.isKeyPressed('right')) {
        this.direction = 'right';
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

      this.updateAnimation();
      this.updateMovement();
      this.parent();

      return true;
    },
    isDead: function() {
      // Check for each possible death condition here

      // is under water
      if (me.state.current().water.isOver(this)) {
        return true;
      }
    }
  });

  return Character;
});
