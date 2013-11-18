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

        for (var x = 0; x < 3; x++) {
            this.Sprites[x] = new Array(3);
            for (var y = 0; y < 3; y++) {
                this.Sprites[x][y] = new me.AnimationSheet(x * 32, y * 32, me.loader.getImage("tiles"), 32, 32);
                this.Sprites[x][y].setOpacity(0);
                this.Sprites[x][y].animationpause = true;
                this.Sprites[x][y].z = 2;

                this.addChild(this.Sprites[x][y]);
            }
        }

        this.floating = true;

        this.alwaysUpdate = true;

        this.reset();
    },

    reset: function () {
        var dungeon = me.game.world.getEntityByProp("name", "dungeon")[0];

        this.pos = new me.Vector2d(32 * 32, 8 * 32);

        var floor = PieceHelper.randomBool();
        var wall = PieceHelper.randomBool();
        if (floor == false && wall == false) floor = true;

        this.Tiles = [[-1, -1, -1],
                      [-1, -1, -1],
                      [-1, -1, -1]];

        if (!dungeon.isComplete) {
            if (floor) {
                var piece = Math.floor(Math.random() * PieceHelper.FloorPieces.length);
                for (var x = 0; x < 3; x++) {
                    for (var y = 0; y < 3; y++) {
                        if (PieceHelper.FloorPieces[piece][y][x] > -1)
                            this.Tiles[x][y] = PieceHelper.FloorPieces[piece][y][x];
                    }
                }
            }
            if (wall) {
                var piece = Math.floor(Math.random() * PieceHelper.WallPieces.length);
                for (var x = 0; x < 3; x++) {
                    for (var y = 0; y < 3; y++) {
                        if (PieceHelper.WallPieces[piece][y][x] > -1)
                            this.Tiles[x][y] = PieceHelper.WallPieces[piece][y][x];
                    }
                }
            }
        }

        // Place random stuff!
        var ran;
        var stairsPlaced = false;
        var chestPlaced = false;
        for (var x = 0; x < 3; x++) {
            for (var y = 0; y < 3; y++) {
                if (this.Tiles[x][y] == 0) {

                    // Stairs
                    if (!dungeon.stairsOK) {
                        ran = Math.floor(Math.random() * (35 - dungeon.highestUsedColumn));
                        if (ran == 0 && !stairsPlaced) {
                            this.Tiles[x][y] = PieceHelper.STAIRS_TILE;
                            stairsPlaced = true;
                        }
                    }

                    // Chests
                    ran = Math.floor(Math.random() * (50 - dungeon.highestUsedColumn));
                    if (ran == 0 && !chestPlaced) {
                        this.Tiles[x][y] = PieceHelper.CHEST_TILE;
                        chestPlaced = true;
                    }

                    // Mobs (placement will be determined by hero's level i guess)
                    ran = Math.floor(Math.random() * (10));
                    if (ran == 0) {
                        this.Tiles[x][y] = PieceHelper.MIN_MOB_TILE;
                    }

                }
            }
        }

        this.moveTimer = me.timer.getTime();
        this.moveTimerTarget = 500;

        this.keyTimer = me.timer.getTime();
        this.keyMoveTimerTarget = 100;
        this.keyRotTimerTarget = 200;
    },

    update: function () {

        if (me.input.isKeyPressed("rotleft")) this.rotateLeft();
        if (me.input.isKeyPressed("rotright")) this.rotateRight();
        if (me.input.isKeyPressed("moveup")) this.moveUp();
        if (me.input.isKeyPressed("movedown")) this.moveDown();
        if (me.input.isKeyPressed("push")) this.push();

        if (me.timer.getTime() >= this.moveTimer + this.moveTimerTarget) {
            this.moveTimer = me.timer.getTime();
            if (this.checkMove(this.pos.x - 32, this.pos.y)) this.pos.x -= 32;
            else this.lockPiece();
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

    lockPiece: function () {
        var posx = this.pos.x / 32;
        var posy = this.pos.y / 32;

        var dungeon = me.game.world.getEntityByProp("name", "dungeon")[0];

        for (var x = 0; x < 3; x++) {
            for (var y = 0; y < 3; y++) {
                var tx = posx + x;
                var ty = posy + y;

                if (this.Tiles[x][y] > -1) dungeon.Tiles[tx][ty] = this.Tiles[x][y];

                // Chest spawn
                if (this.Tiles[x][y] == PieceHelper.CHEST_TILE) { }

                // Mob spawn
                if (this.Tiles[x][y] >= PieceHelper.MIN_MOB_TILE && this.Tiles[x][y] <= PieceHelper.MAX_MOB_TILE) {
                    var newMob = new game.Mob(tx * 32, ty * 32, { type: this.Tiles[x][y] - PieceHelper.MIN_MOB_TILE });
                    me.game.add(newMob);
                    dungeon.Tiles[tx][ty] = 0;
                }
            }
        }

        dungeon.rebuild();
        this.reset();
    },

    rotateLeft: function () {
        if (me.timer.getTime() < this.keyTimer + this.keyRotTimerTarget) return;
        this.keyTimer = me.timer.getTime();

        if (this.checkRotate(this.pos.x, this.pos.y, PieceHelper.rotateLeft(this.Tiles))) this.Tiles = PieceHelper.rotateLeft(this.Tiles);
        else if (this.checkRotate(this.pos.x, this.pos.y - 32, PieceHelper.rotateLeft(this.Tiles))) {
            this.pos.y -= 32; 
            this.Tiles = PieceHelper.rotateLeft(this.Tiles);
        }
        else if (this.checkRotate(this.pos.x, this.pos.y + 32, PieceHelper.rotateLeft(this.Tiles))) {
            this.pos.y += 32;
            this.Tiles = PieceHelper.rotateLeft(this.Tiles);
        }
    },

    rotateRight: function () {
        if (me.timer.getTime() < this.keyTimer + this.keyRotTimerTarget) return;
        this.keyTimer = me.timer.getTime();

        if (this.checkRotate(this.pos.x, this.pos.y, PieceHelper.rotateRight(this.Tiles))) this.Tiles = PieceHelper.rotateRight(this.Tiles);
        else if (this.checkRotate(this.pos.x, this.pos.y - 32, PieceHelper.rotateRight(this.Tiles))) {
            this.pos.y -= 32;
            this.Tiles = PieceHelper.rotateRight(this.Tiles);
        }
        else if (this.checkRotate(this.pos.x, this.pos.y + 32, PieceHelper.rotateRight(this.Tiles))) {
            this.pos.y += 32;
            this.Tiles = PieceHelper.rotateRight(this.Tiles);
        }
    },

    moveUp: function () {
        if (me.timer.getTime() < this.keyTimer + this.keyMoveTimerTarget) return;
        this.keyTimer = me.timer.getTime();

        if(this.checkMove(this.pos.x, this.pos.y-32)) this.pos.y -= 32;
    },

    moveDown: function () {
        if (me.timer.getTime() < this.keyTimer + this.keyMoveTimerTarget) return;
        this.keyTimer = me.timer.getTime();

        if (this.checkMove(this.pos.x, this.pos.y + 32)) this.pos.y += 32;
    },

    push: function () {
        if (me.timer.getTime() < this.keyTimer + this.keyMoveTimerTarget) return;
        this.keyTimer = me.timer.getTime();
        this.moveTimer = me.timer.getTime();

        if (this.checkMove(this.pos.x - 32, this.pos.y)) this.pos.x -= 32;
    },

    checkMove: function (posx,posy) {
        posx = posx / 32;
        posy = posy / 32;

        var dungeon = me.game.world.getEntityByProp("name", "dungeon")[0];

        for (var x = 0; x < 3; x++) {
            for (var y = 0; y < 3; y++) {
                var tx = posx + x;
                var ty = posy + y;

                if(this.Tiles[x][y]>-1) {
                    if (dungeon.Tiles[tx][ty] > -1) return false;
                }
            }
        }

        return true;
    },

    checkRotate: function (posx, posy, rotatedTiles) {
        posx = posx / 32;
        posy = posy / 32;

        var dungeon = me.game.world.getEntityByProp("name", "dungeon")[0];

        for (var x = 0; x < 3; x++) {
            for (var y = 0; y < 3; y++) {
                var tx = posx + x;
                var ty = posy + y;

                
                if (rotatedTiles[x][y] > -1) {
                    if (dungeon.Tiles[tx][ty] > -1) return false;
                }
            }
        }
        return true;
    },

   


});