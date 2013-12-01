﻿/// <reference path="../../lib/melonJS-0.9.10.js" />

game.Dungeon = me.ObjectContainer.extend({

    DUNGEON_WIDTH: 35,
    DUNGEON_HEIGHT: 19,
    Tiles: new Array(35),
    pathGrid: new Array(35),

    wallInGrid: new Array(35),
    wallInCheckX: 1,
    wallInCheckY: 1,

    highestUsedColumn: 1,
    highestCompletedColumn: 0,

    isComplete: false,

    init: function () {
        this.parent();

        this.visible = false;
        this.floating = true;

        this.alwaysUpdate = true;

        this.DUNGEON_WIDTH = me.game.currentLevel.cols;
        this.DUNGEON_HEIGHT = me.game.currentLevel.rows;

        this.name = "dungeon";
     
        for (var x = 0; x < this.DUNGEON_WIDTH; x++) {
            this.Tiles[x] = new Array(this.DUNGEON_HEIGHT);
            this.pathGrid[x] = new Array(this.DUNGEON_HEIGHT);
            this.wallInGrid[x] = new Array(this.DUNGEON_HEIGHT);
            for (var y = 0; y < this.DUNGEON_HEIGHT; y++) {
                var tile = me.game.currentLevel.getLayerByName("foreground").layerData[x][y];
                if (tile != null)
                    this.Tiles[x][y] = tile.tileId-1;
                else
                    this.Tiles[x][y] = -1;
            }
        }

        this.rebuild();
    },

    update: function () {
        
        if (this.highestUsedColumn >= this.DUNGEON_WIDTH - 2) {
            for (var x = 0; x < this.DUNGEON_WIDTH; x++) {
                for (var y = 0; y < this.DUNGEON_HEIGHT; y++) {
                    if (this.Tiles[x][y] == -1) this.Tiles[x][y] = 1;
                }
            }
            this.isComplete = true;
            this.highestUsedColumn = this.DUNGEON_WIDTH - 1;
            this.highestCompletedColumn = 1;
            this.rebuild();
        }

        for (var y = 1; y < this.DUNGEON_HEIGHT - 1; y++) {
            if (this.Tiles[this.wallInCheckX][y] == -1) {
                var path = this.findPath(this.wallInGrid, this.wallInCheckX, y, this.DUNGEON_WIDTH - 1, y);
                if (!path || path.length == 0) {
                    this.Tiles[this.wallInCheckX][y] = 1;
                    this.rebuild();
                }
            }
        }

        

        //this.wallInCheckY += 1;
        //if (this.wallInCheckY > this.DUNGEON_HEIGHT-2) {
            if(this.wallInCheckX<this.DUNGEON_WIDTH-1) this.wallInCheckX++;
        //    this.wallInCheckY = 1;
            if (this.wallInCheckX > this.highestUsedColumn) {
                this.wallInCheckX = this.highestCompletedColumn+1;
            }
        //}

        return true;
    },

    rebuild: function () {

        var wallGrid = [[0, 0, 0],
                        [0, 0, 0],
                        [0, 0, 0]];
     
        for (var x = 0; x < this.DUNGEON_WIDTH; x++) {
            var colUsed = false;
            var colComplete = true;
            for (var y = 0; y < this.DUNGEON_HEIGHT; y++) {
                if ((this.Tiles[x][y] >= PieceHelper.MIN_WALL_TILE && this.Tiles[x][y] <= PieceHelper.MAX_WALL_TILE)) {
                    wallGrid = this.getSurroundingWalls(x, y);
                    var bestTile = 1;
                    var bestScore = 0;
                    for (var i = 0; i < PieceHelper.WallAttributes.length; i++) {
                        var thisScore = 0;
                        for (var ax = 0; ax < 3; ax++) {
                            for (var ay = 0; ay < 3; ay++) {
                                if (PieceHelper.WallAttributes[i][ay][ax] != -1) {
                                    if (PieceHelper.WallAttributes[i][ay][ax] == wallGrid[ax][ay]) thisScore++;
                                    else thisScore--;
                                }
                                else thisScore++;
                            }
                        }
                        if (thisScore > bestScore) {
                            bestTile = PieceHelper.MIN_WALL_TILE + i;
                            bestScore = thisScore;
                        }
                    }
                    this.Tiles[x][y] = bestTile;
                }

                // Build astar
                if (this.Tiles[x][y] == -1 || (this.Tiles[x][y] >= PieceHelper.MIN_WALL_TILE && this.Tiles[x][y] <= PieceHelper.MAX_WALL_TILE))
                    this.pathGrid[x][y] = 0;
                else
                    this.pathGrid[x][y] = 1;

                if (this.Tiles[x][y] == -1)
                    this.wallInGrid[x][y] = 1;
                else
                    this.wallInGrid[x][y] = 0;

                // Calculate used/complete cols
                if(x>=1 && y>=1 && x<=this.DUNGEON_WIDTH-1 && y<=this.DUNGEON_HEIGHT-2) {
                    if (this.Tiles[x][y] > -1) colUsed = true;
                    if (this.Tiles[x][y] == -1) colComplete = false;
                }
            }

            if (colUsed && x > this.highestUsedColumn) this.highestUsedColumn = x;
            if (colComplete && this.highestCompletedColumn == x - 1) this.highestCompletedColumn = x;

        }

        


        var layer = me.game.currentLevel.getLayerByName("foreground");
        for (var x = 0; x < this.DUNGEON_WIDTH; x++) {
            for (var y = 0; y < this.DUNGEON_HEIGHT; y++) {
                if (this.Tiles[x][y] > -1) layer.setTile(x, y, this.Tiles[x][y] + 1);
            }
        }

        
    },

    findPath: function(grid,startx,starty,endx,endy) {
        var graph = new Graph(grid);
        var start = graph.nodes[startx][starty];
        var end = graph.nodes[endx][endy];
        return astar.search(graph.nodes, start, end);
    },

    getSurroundingWalls: function(tx,ty) {
        var returnArray = [[0, 0, 0],
                        [0, 0, 0],
                        [0, 0, 0]];
        for (var x = -1; x < 2; x++) {
            for (var y = -1; y < 2; y++) {
                if (tx + x >= 0 && tx + x < this.DUNGEON_WIDTH && ty + y >= 0 && ty + y < this.DUNGEON_HEIGHT) {
                    if (this.Tiles[tx+x][ty+y] >= PieceHelper.MIN_WALL_TILE && this.Tiles[tx+x][ty+y] <= PieceHelper.MAX_WALL_TILE)
                        returnArray[x + 1][y + 1] = 1;
                }
                else returnArray[x + 1][y + 1] = 0;
            }
        }

        return returnArray;
    },

    draw: function (context) {


    }
});