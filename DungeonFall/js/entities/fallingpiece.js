/// <reference path="../../lib/melonJS-0.9.10.js" />


game.FallingPiece = me.ObjectContainer.extend({

    init: function () {
        // call the constructor
        this.parent();

        // persistent across level change
        this.isPersistent = true;

        // non collidable
        this.collidable = false;

        // make sure our object is always draw first
        this.z = Infinity;

        // give a name
        this.name = "FallingPiece";

        this.Tiles = new Array(3);
        this.Sprites = new Array(3);

        this.Tiles = [[1, 1, -1],
                      [-1, 1, -1],
                      [-1, 1, -1]];

        for (var x = 0; x < 3; x++) {
            this.Sprites[x] = new Array(3);
            for (var y = 0; y < 3; y++) {
                this.Sprites[x][y] = new me.AnimationSheet(x * 32, y * 32, me.loader.getImage("tiles"), 32, 32);
                if (this.Tiles[x][y] > -1) {
                    this.Sprites[x][y].setOpacity(1);
                    this.Sprites[x][y].setAnimationFrame(this.Tiles[x][y]);
                }
                else this.Sprites[x][y].setOpacity(0);
                this.Sprites[x][y].animationpause = true;
                this.Sprites[x][y].z = 2;

                this.addChild(this.Sprites[x][y]);
            }
        }

        this.floating = true;

        this.pos = new me.Vector2d(32 * 32, 8 * 32);

        this.alwaysUpdate = true;

        this.moveTimer = me.timer.getTime();
        this.moveTimerTarget = 10;

        this.keyTimer = me.timer.getTime();
        this.keyMoveTimerTarget = 100;
        this.keyRotTimerTarget = 200;
    },

    update: function () {

        if (me.input.isKeyPressed("rotleft")) this.rotateLeft();
        if (me.input.isKeyPressed("rotright")) this.rotateRight();
        if (me.input.isKeyPressed("moveup")) this.moveUp();
        if (me.input.isKeyPressed("movedown")) this.moveDown();
        if (me.input.isKeyPressed("push")) this.pos.x -= 5;

        if (me.timer.getTime() >= this.moveTimer + this.moveTimerTarget) {
            this.moveTimer = me.timer.getTime();
            this.pos.x -= 1;
        }

        for (var x = 0; x < 3; x++) {
            for (var y = 0; y < 3; y++) {
                if (this.Tiles[x][y] > -1) {
                    this.Sprites[x][y].setOpacity(1);
                    this.Sprites[x][y].setAnimationFrame(this.Tiles[x][y]);
                }
                else this.Sprites[x][y].setOpacity(0);
            }
        }

        return true;
    },

    rotateLeft: function () {
        if (me.timer.getTime() < this.keyTimer + this.keyRotTimerTarget) return;
        this.keyTimer = me.timer.getTime();

        this.Tiles = PieceHelper.rotateLeft(this.Tiles);
    },

    rotateRight: function () {
        if (me.timer.getTime() < this.keyTimer + this.keyRotTimerTarget) return;
        this.keyTimer = me.timer.getTime();

        this.Tiles = PieceHelper.rotateRight(this.Tiles);
    },

    moveUp: function () {
        if (me.timer.getTime() < this.keyTimer + this.keyMoveTimerTarget) return;
        this.keyTimer = me.timer.getTime();

        this.pos.y -= 32;
    },

    moveDown: function () {
        if (me.timer.getTime() < this.keyTimer + this.keyMoveTimerTarget) return;
        this.keyTimer = me.timer.getTime();

        this.pos.y += 32;
    }


});