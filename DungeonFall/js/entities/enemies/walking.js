
game.WalkingEnemy = me.ObjectEntity.extend({
    init: function (x, y, settings) {
        // define this here instead of tiled
        //settings.image = "furball";
        //settings.spritewidth = 110;
        //settings.spriteheight 

        // call the parent constructor
        this.parent(x, y, settings);

        this.renderable.anim = {};

        var walkarray = new Array();
        for (var i = 0; i < settings.walkframes; i++) walkarray[i] = i;

        var diearray = new Array();
        for (var i = 0; i < settings.dieframes; i++) diearray[i] = settings.walkframes + i;

        // 0-4
        // 5-12

        this.renderable.addAnimation("walk", walkarray);
        this.renderable.addAnimation("die", diearray);
        this.renderable.setCurrentAnimation("walk");

        this.startX = x;

        if (settings.width) {
            this.endX = x + settings.width - settings.spritewidth;
            this.pos.x = x + settings.width - settings.spritewidth;
            this.constrained = true;
            this.walkLeft = true;
        } else {
            this.constrained = false;
            this.walkLeft = false;
        }

        this.updateColRect((settings.spriteheight-80)/2, 80, (settings.spriteheight-100), 90);

        // walking & jumping speed
        this.setVelocity(2, 6);

        // make it collidable
        this.collidable = true;
        this.spawning = false;
        // make it a enemy object
        this.type = me.game.ENEMY_OBJECT;

        this.health = settings.hp;
        this.dying = false;

        this.canKnockback = true;
        this.knockbackTime = 0;

        

        this.alwaysUpdate = true;

    },

    // call by the engine when colliding with another object
    // obj parameter corresponds to the other object (typically the player) touching this one
    onCollision: function (res, obj) {

        // res.y >0 means touched by something on the bottom
        // which mean at top position for this one
        if (this.alive && !this.renderable.isFlickering() && !this.dying && this.knockbackTime <= 0) {
            if ((obj.attacking || obj instanceof game.ProjectileEntity)) {
                if (obj instanceof game.ProjectileEntity) me.game.remove(obj);
                if (this.canKnockback) this.knockback((this.pos.x + this.renderable.hWidth) - (obj.pos.x + obj.renderable.hWidth));
                this.health--;
                if (this.health > 0) {
                    this.renderable.flicker(45);
                }
                else {
                    this.die();
                }
            }
            else if (obj instanceof game.PlayerEntity) {
                obj.die();
            }
        }
    },

    // manage the enemy movement
    update: function () {
        // do nothing if not in viewport
        //if (!this.inViewport)
          //  return false;

        if (this.alive && !this.dying && !this.knockbackTime > 0 && !this.spawning) {
            if (this.walkLeft && this.pos.x <= this.startX && this.constrained) {
                this.walkLeft = false;
            } else if (!this.walkLeft && this.pos.x >= this.endX && this.constrained) {
                this.walkLeft = true;
            }
            // make it walk
            this.flipX(this.walkLeft);
            this.vel.x += (this.walkLeft) ? -this.accel.x * me.timer.tick : this.accel.x * me.timer.tick;

        } else {
            if (!this.knockbackTime > 0) this.vel.x = 0;
        }

        if (this.knockbackTime > 0) {
            if (this.vel.x > 0) this.vel.x -= 0.1;
            if (this.vel.x < 0) this.vel.x += 0.1;
            //this.vel.x += this.accel.x * me.timer.tick;

            if (this.falling) this.constrained = false;

            this.knockbackTime -= me.timer.tick;

            if (!this.dying) {
                this.renderable.animationpause = true;
                this.renderable.setAnimationFrame(0);
            }
        }
        else {
            this.maxVel.x = 2;
            if (!this.dying) {
                this.renderable.animationpause = false;
            }
        }

        
        if (!this.spawning) {
            // check and update movement
            var res = this.updateMovement();
            if (res.x != 0) {
                if (this.walkLeft) {
                    this.walkLeft = false;
                } else if (!this.walkLeft) {
                    this.walkLeft = true;
                }
            }
        }

        if (this.dying && this.renderable.anim["die"].idx == this.renderable.anim["die"].length - 1) {
            this.renderable.animationpause = true;
            this.renderable.alpha -= 0.01;
            if (this.renderable.alpha <= 0.01) me.game.remove(this);
        }

        // update animation if necessary
        if (this.vel.x != 0 || this.vel.y != 0 || this.dying || this.knockback>0 || this.spawning) {
            // update object animation
            this.parent();
            return true;
        }

        

        return false;
    },

    knockback: function (dir) {
        if (dir > 0) dir = 1;
        if (dir < 0) dir = -1;
        this.maxVel.x = 20;
        this.vel.x = 5 * dir;
        this.knockbackTime = 45;
    },

    die: function () {
        this.dying = true;
        this.renderable.setCurrentAnimation("die");
        game.data.score += 5;
    }
});
