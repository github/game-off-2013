/// <reference path="../../lib/melonJS-0.9.10.js" />


/*------------------- 
a player entity
-------------------------------- */
game.Hero = me.ObjectEntity.extend({

    DUNGEON_WIDTH: 35,
    DUNGEON_HEIGHT: 19,
    DiscoveredTiles: new Array(35),
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

        x = -32;
        //y = 288 + 16;
        this.target = new me.Vector2d(x + (32*3), y);

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

        for (var x = 0; x < this.DUNGEON_WIDTH; x++) {
            this.DiscoveredTiles[x] = new Array(this.DUNGEON_HEIGHT);
            for (var y = 0; y < this.DUNGEON_HEIGHT; y++) {
                this.DiscoveredTiles[x][y] = -1;
            }
        }

        this.walkTween = new me.Tween(this.pos).to(this.target, 2000).onComplete(this.targetReached.bind(this));
        this.walkTween.easing(me.Tween.Easing.Linear.None);
        this.walkTween.start();
        this.isTravelling = true;
    },

    scanX: 0,
    scanY: 0,
    update: function () {
        var dungeon = me.game.world.getEntityByProp("name", "dungeon")[0];

        var tx = Math.floor(this.pos.x / 32);
        var ty = Math.floor(this.pos.y / 32);

        for (var x = 0; x < this.DUNGEON_WIDTH; x++) {
            for (var y = 0; y < this.DUNGEON_HEIGHT; y++) {
                if (this.DiscoveredTiles[x][y] == -1) {
                    if (dungeon.Tiles[x][y] == -1 || (dungeon.Tiles[x][y] >= PieceHelper.MIN_WALL_TILE && dungeon.Tiles[x][y] <= PieceHelper.MAX_WALL_TILE))
                        this.DiscoveredTiles[x][y] = -1;
                    else
                        this.DiscoveredTiles[x][y] = 0;
                }
            }
        }

        for (var x = tx-1; x <= tx+1; x++) {
            for (var y = ty - 1; y <= ty + 1; y++) {
                if (x >= 1 && y >= 1 && x <= this.DUNGEON_WIDTH - 1 && y <= this.DUNGEON_HEIGHT - 2) this.DiscoveredTiles[x][y] = 1;
            }
        }

        for(this.scanY=1;this.scanY<this.DUNGEON_HEIGHT-2;this.scanY++)
        {
            if (!this.isTravelling && !this.isFollowingPath) {
                if (this.DiscoveredTiles[this.scanX][this.scanY] == 0) {
                    var path = dungeon.findPath(dungeon.pathGrid, tx, ty, this.scanX,this.scanY);
                    if (path.length > 0) {
                        this.currentPath = path;
                        this.currentPathStep = 0;
                        this.target = new me.Vector2d((this.currentPath[this.currentPathStep].x * 32), (this.currentPath[this.currentPathStep].y * 32));
                        this.walkTween = new me.Tween(this.pos).to(this.target, 100).onComplete(this.targetReached.bind(this));
                        this.walkTween.easing(me.Tween.Easing.Linear.None);
                        this.walkTween.start();
                        this.isTravelling = true;
                        this.isFollowingPath = true;
                    }
                }
                if (dungeon.Tiles[this.scanX][this.scanY] == PieceHelper.STAIRS_TILE) {
                    var path = dungeon.findPath(dungeon.pathGrid, tx, ty, this.scanX,this.scanY);
                    if (path.length > 0) {
                        dungeon.stairsOK = true;
                    } else {
                        dungeon.stairsOK = false;
                    }
                }
            }
        }

        
        this.scanX++;
        if (this.scanX == this.DUNGEON_WIDTH - 1) {
            this.scanX = 1;
        }
       

        this.updateMovement();

        this.parent();
       
        return true;
    },

    targetReached: function () {
        var dungeon = me.game.world.getEntityByProp("name", "dungeon")[0];

        this.isTravelling = false;
        if (this.isFollowingPath) {
            if (this.currentPathStep < this.currentPath.length - 1) {
                var path = dungeon.findPath(dungeon.pathGrid, this.pos.x / 32, this.pos.y / 32, this.currentPath[this.currentPath.length - 1].x, this.currentPath[this.currentPath.length - 1].y);
                if (path.length > 0) {
                    this.currentPathStep++;
                    this.target = new me.Vector2d((this.currentPath[this.currentPathStep].x * 32), (this.currentPath[this.currentPathStep].y * 32));
                    this.walkTween = new me.Tween(this.pos).to(this.target, 100).onComplete(this.targetReached.bind(this));
                    this.walkTween.easing(me.Tween.Easing.Linear.None);
                    this.walkTween.start();
                    this.isTravelling = true;
                } else {
                    this.isFollowingPath = false;
                }
            }
            else {
                this.isFollowingPath = false;
            }
        }
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

        this.init(this.spawnPosition.x, this.spawnPosition.y, { image: "girl" });
    }

});