define(['waterTool'], function(WaterTool) {
  var Character = me.ObjectEntity.extend({
    init: function(x, y, settings) {
      this.parent(x, y, settings);
      this.setVelocity(3, 15);
      this.waterTool = new WaterTool();

      //  Add animation sets
      this.renderable.addAnimation('anStill', [0, 1, 2, 3, 4, 5, 6, 7]);
      this.renderable.addAnimation('anRight', [8, 9, 10, 11, 12, 13]);
      this.renderable.addAnimation('anLeft', [16, 17, 18, 19, 20, 21]);
      this.renderable.addAnimation('anJump', [24, 25, 25, 26, 27, 28, 29, 30]);
        
      this.direction = 'right';
      //this.jumping = 'false';

      // We need it so when the character falls too quickly,
      // the death by water check can still be done.
      this.alwaysUpdate = true;
    },
    is: function(Animation){
      if(! this.renderable.isCurrentAnimation(Animation) ){
        this.renderable.setCurrentAnimation(Animation);
      }
    },
    updateAnimation: function(){
        if( this.direction == 'right' && !this.renderable.isCurrentAnimation('anRight')){
            this.renderable.setCurrentAnimation('anRight');
        } else if( this.direction == 'left' && !this.renderable.isCurrentAnimation('anLeft')){
            this.renderable.setCurrentAnimation('anLeft');
        } //else if ( this.vel.x == 0 && !this.renderable.isCurrentAnimation('anStill'){
           // this.renderable.setCurrentAnimation('anStill');
    //    }
     //   if (this.jumping){
      //      this.renderable.setCurrentAnimation('anJump');
    },
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
