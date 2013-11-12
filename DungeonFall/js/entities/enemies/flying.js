
game.FlyingEnemy = me.ObjectEntity.extend({
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


        this.updateColRect((settings.spriteheight-80)/2, 80, ((settings.spriteheight-60)/2)-10, 60);

        // walking & jumping speed
        this.setVelocity(5, 0);
        this.setMaxVelocity(5, 15);

        // make it collidable
        this.collidable = true;
        this.spawning = false;
        // make it a enemy object
        this.type = me.game.ENEMY_OBJECT;

        this.health = settings.hp;
        this.dying = false;

        this.canKnockback = false;
        this.knockbackTime = 0;

        this.gravity = 0;

        this.alwaysUpdate = false;

        this.flipX(true);

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
        if (this.inViewport) this.alwaysUpdate = true;
            //return false;

        if (this.alive && !this.dying && !this.spawning) {
           
            //this.flipX(true);
            this.vel.x += -this.accel.x * me.timer.tick;

        } else {
            this.vel.x = 0;
        }

        if (this.knockbackTime > 0) {
            if (this.vel.x > 0) this.vel.x -= 0.1;
            if (this.vel.x < 0) this.vel.x += 0.1;
            //this.vel.x += this.accel.x * me.timer.tick;

            this.knockbackTime -= me.timer.tick;

            if (!this.dying) {
                this.renderable.animationpause = true;
                this.renderable.setAnimationFrame(0);
            }
        }
        else {
            this.maxVel.x = 5;
            if (!this.dying) {
                this.renderable.animationpause = false;
            }
        }

        
        if (!this.spawning) {
            // check and update movement
            var res = this.updateMovement();
            if (res.x != 0) {
                this.die();
            //    if (this.walkLeft) {
            //        this.walkLeft = false;
            //    } else if (!this.walkLeft) {
            //        this.walkLeft = true;
            //    }
            }
        }

        if (this.dying && this.renderable.anim["die"].idx == this.renderable.anim["die"].length-1) {
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
        this.gravity = 0.98;
        this.renderable.setCurrentAnimation("die");
        game.data.score += 5;
    }
});
