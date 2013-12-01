
/**
 * a HUD container and child items
 */

game.HUD = game.HUD || {};

game.HUD.TextLines = [],
game.HUD.addLine = function (line) {
    game.HUD.TextLines[game.HUD.TextLines.length] = capitaliseFirstLetter(line);
};

game.HUD.Container = me.ObjectContainer.extend({

    itemSprites: new Array(8),
    itemSpriteNames: ["ihead","ichest","ilegs","iarms","ihands","ifeet","isword","ipotion"],

    init: function () {
        // call the constructor
        this.parent();

        // persistent across level change
        this.isPersistent = true;

        // non collidable
        this.collidable = false;

        // make sure our object is always draw first
        this.z = 999;

        // give a name
        this.name = "HUD";

        var textWindow = new game.HUD.TextWindow(500, 608);
        this.addChild(textWindow);

        var bg = new me.SpriteObject(0, 608, me.loader.getImage("hudbg"), 1120, 128);
        bg.floating = true;
        bg.z = -2;
        this.addChild(bg);

        this.itemSpriteNames = ["ihead", "ichest", "ilegs", "iarms", "ihands", "ifeet", "isword", "ipotion"];

        var sx = 84;
        var sy = 698;
        for (var i = 0; i < 8; i++) {
            this.itemSprites[i] = new me.SpriteObject(sx, sy, me.loader.getImage(this.itemSpriteNames[i]), 30, 30);
            this.itemSprites[i].floating = true;
            this.itemSprites[i].z = 0;
            this.itemSprites[i].alpha = 0;
            this.addChild(this.itemSprites[i]);
            sx += 40;
        }

        for (var i = 0; i < 7; i++) game.HUD.addLine("");
        game.HUD.addLine("Welcome to DungeonFall");
        game.HUD.addLine("By Gareth Williams");
        game.HUD.addLine("");
        game.HUD.addLine("Arrows - Move/Rotate Piece");
        game.HUD.addLine("Z - Fast Drop");
        game.HUD.addLine("X - Instant Drop");
        game.HUD.addLine("");




        this.font = new me.BitmapFont("font", { x: 32, y: 32 }, 0.8);
        this.font.set("center");
        this.itemfontgreen = new me.BitmapFont("floatfont-green", { x: 13, y: 14 }, 1);
        this.itemfontgreen.set("center");
        this.itemfontred = new me.BitmapFont("floatfont-red", { x: 13, y: 14 }, 1);
        this.itemfontred.set("center");
        this.itemfontwhite = new me.BitmapFont("floatfont-white", { x: 13, y: 14 }, 1);
        this.itemfontwhite.set("center");
        this.floating = true;


        this.alwaysUpdate = true;
    },

    draw: function (context) {
        this.parent(context);
        try {
            var hero = me.game.world.getEntityByProp("name", "hero")[0];

            this.font.draw(context, hero.Level, 35, 630);

            this.font.draw(context, "Floor "+game.Level, me.game.viewport.width/2, 4);


            context.strokeStyle = "silver";
            context.strokeRect(70, 618, 410, 26);
            context.fillStyle = "#AA0000";
            context.fillRect(72, 620, (406 / hero.HPMax) * hero.HP, 22);

            context.strokeStyle = "silver";
            context.strokeRect(70, 650, 410, 15);
            context.fillStyle = "#AAAA00";
            context.fillRect(72, 652, (406 / (hero.XPTNL)) * (hero.XP), 11);

            for (var i = 0; i < 6; i++) {
                if (hero.Items[i] > 0) {
                    this.itemSprites[i].alpha = 1;
                    this.itemfontgreen.draw(context, "+" + hero.Items[i], this.itemSprites[i].pos.x+15, this.itemSprites[i].pos.y-10);
                } else this.itemSprites[i].alpha = 0;
            } 

            if (hero.Items[6] > 0) {
                this.itemSprites[6].alpha = 1;
                this.itemfontred.draw(context, "+" + hero.Items[6], this.itemSprites[6].pos.x + 15, this.itemSprites[6].pos.y - 10);
            } else this.itemSprites[6].alpha = 0;

            if (hero.Items[7] > 0) {
                this.itemSprites[7].alpha = 1;
                this.itemfontwhite.draw(context, hero.Items[7], this.itemSprites[7].pos.x + 15, this.itemSprites[7].pos.y - 10);
            }
            else this.itemSprites[7].alpha = 0;

            //this.font.draw(context, "HP: " + hero.HP + "/" + hero.HPMax + " Dam:" + hero.DRMax + " Def: " + hero.SRMax, 10, 628);
            //this.font.draw(context, "XP " + hero.XP + "/" + hero.XPTNL, 10, 646);

        }
        catch (e) { }
    }
});

game.HUD.FloatyTextContainer = me.ObjectContainer.extend({

    init: function () {
        // call the constructor
        this.parent();

        // persistent across level change
        this.isPersistent = true;

        // non collidable
        this.collidable = false;

        // make sure our object is always draw first
        this.z = 10;

        // give a name
        this.name = "FloatyTextContainer";

        this.alwaysUpdate = true;
    },

    clear: function() {
        for (var i = this.children.length, obj; i--, obj = this.children[i];) {
                this.removeChild(obj);
        }
    }

});

game.HUD.addFloatyText = function (pos, text, col, size) {
    var ft = new game.HUD.FloatyText(pos.x, pos.y, text, col, size);
    me.game.world.getEntityByProp("name", "FloatyTextContainer")[0].addChild(ft);
}

game.HUD.FloatyText = me.ObjectContainer.extend({
    init: function (x, y, text, col, size) {
        if (!size) size = 1;
        this.parent(x, y,100,100);

        this.text = text;

        this.font = new me.BitmapFont("floatfont-" + col, { x: 13, y: 14 }, size);
        this.font.alignText = "top";

        this.floating = true;
        this.z = 10;

        this.setOpacity(1);

        var posTween = new me.Tween(this.pos).to({ y: y - 100 }, 2000 * size).onComplete(this.remove.bind(this));
        posTween.easing(me.Tween.Easing.Linear.None);
        posTween.start();

        var alphaTween = new me.Tween(this).to({ alpha: 0 }, 1800 * size);
        alphaTween.easing(me.Tween.Easing.Linear.None);
        alphaTween.start();

        this.alwaysUpdate = true;
    },

    remove: function() {
        me.game.world.getEntityByProp("name", "FloatyTextContainer")[0].removeChild(this);
    },

    draw: function (context) {
        context.globalAlpha = this.alpha;
        this.font.draw(context, this.text, this.pos.x, this.pos.y);
        context.globalAlpha = 1;
    }
});

game.HUD.TextWindow = me.ObjectContainer.extend({ 
    init: function (x, y) {
        this.parent(x,y,620,128);

        this.font = new me.BitmapFont("textfont", { x: 21, y:22 }, 0.6);
        this.font.alignText = "top";

        this.floating = true;
        this.z = -1;
    },

    update: function () {


        return false;
    },

    draw: function (context) {
        var y = 113;
        for (var i = game.HUD.TextLines.length-1; i > game.HUD.TextLines.length-10; i--) {
            this.font.draw(context, game.HUD.TextLines[i], this.pos.x, this.pos.y + y);
            y-=14
        }
    }
});

function capitaliseFirstLetter(string) {
    return string.charAt(0).toUpperCase() + string.slice(1);
}

//game.HUD.ScoreItem = me.Renderable.extend({
//    init: function (x, y) {
//        this.parent(new me.Vector2d(x, y), 10, 10);

//        this.font = new me.BitmapFont("font", { x: 32, y: 32 });
//        this.font.alignText = "bottom";
//        this.font.set("center", 1);

//        this.score = 0;

//        this.floating = true;
//    },

//    update: function () {

//        if (this.score !== game.data.score) {
//            this.score = game.data.score;
//            return true;
//        }
//        return false;
//    },

//    draw: function (context) {
//        this.font.draw(context, game.data.score, this.pos.x, this.pos.y);

//    }
//});

//game.HUD.LivesItem = me.Renderable.extend({
//    init: function (x, y) {
//        this.parent(new me.Vector2d(x, y), 10, 10);

//        this.font = new me.BitmapFont("font", { x: 32, y: 32 });
//        this.font.alignText = "bottom";
//        this.font.set("left", 1);

//        this.lives = 0;

//        this.floating = true;
//    },

//    update: function () {

//        if (this.lives !== game.data.lives) {
//            this.score = game.data.lives;
//            return true;
//        }
//        return false;
//    },

//    draw: function (context) {
//        this.font.draw(context, "x" + game.data.lives, this.pos.x, this.pos.y);

//    }
//});