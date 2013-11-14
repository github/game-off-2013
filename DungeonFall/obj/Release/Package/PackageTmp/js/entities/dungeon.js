/// <reference path="../../lib/melonJS-0.9.10.js" />

game.Dungeon = me.ObjectContainer.extend({

    DUNGEON_WIDTH: 35,
    DUNGEON_HEIGHT: 19,
    Tiles: new Array(35),

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
            for (var y = 0; y < this.DUNGEON_HEIGHT; y++) {
                var tile = me.game.currentLevel.getLayerByName("foreground").layerData[x][y];
                if (tile != null)
                    this.Tiles[x][y] = tile.tileId-1;
                else
                    this.Tiles[x][y] = -1;
            }
        }
    },

    update: function () {
        var layer = me.game.currentLevel.getLayerByName("foreground");
        for (var x = 0; x < this.DUNGEON_WIDTH; x++) {
            for (var y = 0; y < this.DUNGEON_HEIGHT; y++) {
                if(this.Tiles[x][y]>-1) layer.setTile(x,y,this.Tiles[x][y]+1);
            }
        }

        return true;
    },

    draw: function (context) {


    }
});