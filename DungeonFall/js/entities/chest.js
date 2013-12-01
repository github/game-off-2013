/// <reference path="../../lib/melonJS-0.9.10.js" />

game.Chest = me.ObjectEntity.extend({

    isOpen: false,
    Type: 0,

    init: function (x, y, settings) {
        // call the constructor
        settings.spritewidth = 32;
        settings.spriteheight = 32;
        this.Type = settings.type;

        settings.image = "chest" + this.Type;


        this.offset = new me.Vector2d(16, 16);

        this.parent(x, y, settings);

        this.renderable.anim = {};
        this.renderable.addAnimation("closed", [0]);
        this.renderable.addAnimation("open", [1]);
        this.renderable.setCurrentAnimation("closed");

        this.setVelocity(0, 0);
        this.setFriction(0, 0);
        this.setMaxVelocity(5, 5);
        
        this.gravity = 0;
      
        this.collidable = false;

        this.alwaysUpdate = true;

        this.name = "chest";

        this.z = 3;;
    },

    update: function () {
        var dungeon = me.game.world.getEntityByProp("name", "dungeon")[0];

        var tx = Math.floor(this.pos.x / 32);
        var ty = Math.floor(this.pos.y / 32);
                   
        this.updateMovement();

        this.parent();
       
        if (dungeon.Tiles[tx][ty] >= PieceHelper.MIN_WALL_TILE && dungeon.Tiles[tx][ty] <= PieceHelper.MAX_WALL_TILE) me.game.remove(this);

        return true;
    },

    open: function () {
        this.isOpen = true;
        this.renderable.setCurrentAnimation("open");
    },

});