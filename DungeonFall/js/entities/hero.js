/// <reference path="../../lib/melonJS-0.9.8.js" />


/*------------------- 
a player entity
-------------------------------- */
game.PlayerEntity = me.ObjectEntity.extend({

    /* -----
 
    constructor
 
    ------ */

    init: function (x, y, settings) {
        // call the constructor
        settings.spritewidth = 220;
        settings.spriteheight = 220;

        this.parent(x, y, settings);

        this.renderable.anim = {};
        this.renderable.addAnimation("walk", [0, 1, 2, 3, 4, 5]);
        this.renderable.addAnimation("attack", [8, 6, 7]);
        this.renderable.addAnimation("jump", [9]);
        this.renderable.addAnimation("die", [10,11,12,13,14,16,17,18]);
        this.renderable.setCurrentAnimation("walk");
        
        //this.renderable.setOffset(0, 110);

        this.setVelocity(0.5, 1);
        this.setFriction(0.25, 0);
        this.setMaxVelocity(5, 15);
        
        this.gravity = 0.5;
      
        this.updateColRect(80, 60, 90, 52);

        this.attacking = false;
        this.alwaysUpdate = true;

        this.dying = false;

        this.facing = 1;

        this.jacks = 0;

        this.vppos = new me.Vector2d(x, y);

        this.spawnPosition = new me.Vector2d(x, y);

        this.deathTimer = 0;
        this.gameOver = false;

        // set the display to follow our position on both axis
        me.game.viewport.follow(this.vppos, me.game.viewport.AXIS.BOTH);
        me.game.viewport.setDeadzone(50, 150);

    },

    /* -----
 
    update the player pos
 
    ------ */
    update: function () {

        if (this.gameOver) return;

        this.vppos.x = this.pos.x + this.renderable.hWidth;
        this.vppos.y = this.pos.y + this.renderable.hHeight;

        if (!this.dying) {
            if (me.input.isKeyPressed('left')) {
                // flip the sprite on horizontal axis
                this.flipX(true);
                this.facing = -1;
                // update the entity velocity
                this.vel.x -= this.accel.x * me.timer.tick;
            } else if (me.input.isKeyPressed('right')) {
                // unflip the sprite
                this.flipX(false);
                this.facing = 1;
                // update the entity velocity
                this.vel.x += this.accel.x * me.timer.tick;
            } else {
                //this.vel.x = 0;
            }
            if (me.input.isKeyPressed('jump')) {
                // make sure we are not already jumping or falling
                if (!this.jumping && !this.falling) {
                    // set current vel to the maximum defined value
                    // gravity will then do the rest
                    this.vel.y = -this.maxVel.y * me.timer.tick;
                    // set the jumping flag
                    this.jumping = true;

                    me.audio.play("jump", false, null, 0.5);
                }

            }
            if (me.input.isKeyPressed("attack")) {
                if (!this.attacking) {
                    this.attacking = true;
                    this.renderable.setCurrentAnimation("attack", (function () {
                        this.renderable.setCurrentAnimation("walk");
                        this.attacking = false;
                    }).bind(this));
                    this.renderable.setAnimationFrame(0);

                    me.audio.play("melee", false, null, 1);
                }
            }
            if (me.input.isKeyPressed("throw") && this.jacks > 0) {
                var bul = new game.ProjectileEntity((this.pos.x + (this.renderable.hWidth - 15)) + (this.facing * 10), this.pos.y+90, { x: this.facing * 15, y: 0 }, { image: "jack", spritewidth: 30, spriteheight: 32 });
                me.game.add(bul);

                this.jacks--;
            }


            if ((this.jumping || this.falling) && !this.attacking) {
                this.renderable.setCurrentAnimation("jump");
            }
            else {
                if (!this.renderable.isCurrentAnimation("walk") && !this.attacking) this.renderable.setCurrentAnimation("walk");
            }

            if (me.input.isKeyPressed("levelskip")) {
                me.levelDirector.nextLevel();
                return false;
            }
        }
        // check & update player movement
        this.updateMovement();

        if (this.pos.y+100 > me.game.currentLevel.rows * me.game.currentLevel.tileheight && !this.dying) {
            this.vel.y = -this.maxVel.y;
            this.vel.x = 0;
            this.die();
        }

        if (this.attacking) {
            this.updateColRect(40, 140, 90, 52);
        }

        var res = me.game.collide(this);

        this.updateColRect(90, 40, 90, 52);

        if (res) {
            //if (res.obj.type == me.game.COLLECTABLE_OBJECT) {

            //}
        }

        game.data.jacks = this.jacks;

        if (this.dying && this.renderable.anim["die"].idx == 7) {
            this.renderable.animationpause = true;
            this.deathTimer -= me.timer.tick;
            if (this.deathTimer <= 0) {
                if (game.data.lives == 0) {
                    this.gameOver = true;
                    // me.state.stop(true);
                    me.state.change(me.state.MENU);
                    return;
                } else {
                    me.game.viewport.fadeIn("#000000", 500, this.reset.bind(this));
                }
            }
            //this.renderable.alpha -= 0.01;
            //if (this.renderable.alpha <= 0.01) me.game.remove(this);
        }

        // update animation if necessary
        if (this.vel.x != 0 || this.vel.y != 0 || this.attacking || this.dying) {
            // update object animation
            this.parent();
            return true;
        }

        // else inform the engine we did not perform
        // any update (e.g. position, animation)
        return false;
    },

    die: function () {
        if (!this.dying) {
            this.dying = true;
            this.renderable.setCurrentAnimation("die");
            this.renderable.setAnimationFrame(0);
            me.audio.play("tilly_die", false, null, 1);

            game.data.lives--;

            this.deathTimer = 100;

        }
    },

    reset: function () {
       
        //this.pos.x = this.spawnPosition.x;
        //this.pos.y = this.spawnPosition.y;
        //this.dying = false;
        //this.attacking = false;
        //this.falling = false;
        //this.jumping = false;
        //this.deathTimer = 0;
        //this.renderable.animationpause = false;
        //this.renderable.setCurrentAnimation("walk");
        //this.renderable.setAnimationFrame(0);
        //this.setVelocity(0.5, 1);
        //this.setFriction(0.25, 0);
        //this.setMaxVelocity(5, 15);
        //me.game.viewport.fadeIn("#FFFFFF", 500);

        this.init(this.spawnPosition.x, this.spawnPosition.y, { image: "girl" });
    }

});