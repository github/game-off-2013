/// <reference path="../../lib/melonJS-0.9.10.js" />

game.Mob = me.ObjectEntity.extend({

    Type: 0,

    DUNGEON_WIDTH: 35,
    DUNGEON_HEIGHT: 19,
    target: new me.Vector2d(0, 0),
    walkTween: null,
    isTravelling: false,
    isFollowingPath: false,
    currentPath: null,
    currentPathStep: 0,

    init: function (x, y, settings) {
        // call the constructor
        settings.spritewidth = 32;
        settings.spriteheight = 32;
        

        this.Type = settings.type;
        settings.image = "mob" + this.Type;

        //x = -32;
        //y = 288 + 16;
        this.target = new me.Vector2d(x, y);

        this.offset = new me.Vector2d(16, 16);

        this.parent(x, y, settings);

        this.renderable.anim = {};
        this.renderable.addAnimation("walk", [0]);
        this.renderable.setCurrentAnimation("walk");

        this.setVelocity(5, 5);
        this.setFriction(0, 0);
        this.setMaxVelocity(5, 5);
        
        this.gravity = 0;
      
        this.collidable = false;

        this.alwaysUpdate = true;

        this.z = 3;

        //this.walkTween = new me.Tween(this.pos).to(this.target, 100).onComplete(this.targetReached.bind(this));
        //this.walkTween.easing(me.Tween.Easing.Linear.None);
        //this.walkTween.start();
        //this.isTravelling = true;
    },

    scanX: 0,
    scanY: 0,
    update: function () {
        var dungeon = me.game.world.getEntityByProp("name", "dungeon")[0];

        var tx = Math.floor(this.pos.x / 32);
        var ty = Math.floor(this.pos.y / 32);

       
                        //this.currentPath = path;
                        //this.currentPathStep = 0;
                        //this.target = new me.Vector2d((this.currentPath[this.currentPathStep].x * 32), (this.currentPath[this.currentPathStep].y * 32));
                        //this.walkTween = new me.Tween(this.pos).to(this.target, 100).onComplete(this.targetReached.bind(this));
                        //this.walkTween.easing(me.Tween.Easing.Linear.None);
                        //this.walkTween.start();
                        //this.isTravelling = true;
                        //this.isFollowingPath = true;
              
        
        

        this.updateMovement();

        this.parent();
       
        return true;
    },

    targetReached: function() {
        this.isTravelling = false;
        if (this.isFollowingPath) {
            if (this.currentPathStep < this.currentPath.length - 1) {
                this.currentPathStep++;
                this.target = new me.Vector2d((this.currentPath[this.currentPathStep].x * 32), (this.currentPath[this.currentPathStep].y * 32));
                this.walkTween = new me.Tween(this.pos).to(this.target, 100).onComplete(this.targetReached.bind(this));
                this.walkTween.easing(me.Tween.Easing.Linear.None);
                this.walkTween.start();
                this.isTravelling = true;
            }
            else {
                this.isFollowingPath = false;
            }
        }
    },

    die: function () {
        
    },

    reset: function () {

        this.init(this.spawnPosition.x, this.spawnPosition.y, { image: "girl" });
    }

});