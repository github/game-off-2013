/// <reference path="../../lib/melonJS-0.9.10.js" />
/*----------------------
 
    A title screen
 
    ----------------------*/

game.TitleScreen = me.ScreenObject.extend({
    // constructor
    init: function () {
        this.parent(true);

        this.bg = null;

        this.ts1 = null;
        this.ts2 = null;
        this.ts3 = null;
        
    },

    // reset function
    onResetEvent: function () {
        this.bg = me.loader.getImage("ts_bg");

        me.input.bindKey(me.input.KEY.ENTER, "start", true);
        me.input.bindKey(me.input.KEY.X, "start", true);
        me.input.bindKey(me.input.KEY.Z, "start", true);
        me.input.bindKey(me.input.KEY.SPACE, "start", true);

        this.ts1 = new me.SpriteObject(-190, 255, me.loader.getImage("ts1"));
        this.ts2 = new me.SpriteObject(200, -280, me.loader.getImage("ts2"));
        this.ts3 = new me.SpriteObject(1067, 380, me.loader.getImage("ts3"));

        this.startText = new game.TitleScreen.StartText(me.game.viewport.width/2, 500);
        //this.addChild(this.ts1);

        //195, 255
        //200, 280
        //600, 380

        var tween = new me.Tween(this.ts1.pos).to({ x: 195 }, 500).onComplete(null);
        tween.easing(me.Tween.Easing.Quadratic.Out);
        tween.start();
        tween = new me.Tween(this.ts3.pos).to({ x: 600 }, 500).onComplete(null);
        tween.easing(me.Tween.Easing.Quadratic.Out);
        tween.start();
        tween = new me.Tween(this.ts2.pos).to({ y: 280 }, 1000).onComplete(null);
        tween.easing(me.Tween.Easing.Bounce.Out);
        tween.start();
        
        this.textin();
    },

    textin: function(){
        var tween = new me.Tween(this.startText).to({ textScale: 0.8 }, 1000).onComplete(this.textout.bind(this));
        tween.easing(me.Tween.Easing.Quadratic.InOut);
        tween.start();
    },
    textout: function(){
        var tween = new me.Tween(this.startText).to({ textScale: 0.9 }, 1000).onComplete(this.textin.bind(this));
        tween.easing(me.Tween.Easing.Quadratic.InOut);
        tween.start();
    },


    // update function
    update: function () {
        if (me.input.isKeyPressed('start')) {
            me.state.change(me.state.PLAY);
           // me.game.viewport.fadeOut(this.fade, this.duration);
        }

        this.startText.update();
        return true;

    },

    // draw function
    draw: function (context) {
        context.drawImage(this.bg, 0, 0);
        this.ts2.draw(context);
        this.ts3.draw(context);
        this.ts1.draw(context);
        this.startText.draw(context);
    },

    // destroy function
    onDestroyEvent: function () {
        me.input.bindKey(me.input.KEY.X, "attack", true);
        me.input.bindKey(me.input.KEY.Z, "throw", true);

        game.data.score = 0;
        game.data.jacks = 0;
        game.data.lives = 3;
    }

});

game.TitleScreen.StartText = me.Renderable.extend({
    init: function (x, y) {
        this.parent(new me.Vector2d(x, y), 10, 10);

        this.font = new me.BitmapFont("font", { x: 32, y: 32 });
        this.font.alignText = "bottom";
        this.font.set("center", 1);

        this.textScale = 1;

        this.floating = true;
    },

    update: function () {
        this.font.resize(this.textScale);

        return true;
    },

    draw: function (context) {
        this.font.draw(context, "Press SPACE to start!", this.pos.x, this.pos.y);

    }
});

