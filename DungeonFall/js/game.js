/// <reference path="../lib/melonJS-0.9.8.js" />

/* Game namespace */
var game = {

    // Run on page load.
    "onload" : function () {
        // Initialize the video.
        if (!me.video.init("screen", 1067, 600, true)) {
            alert("Your browser does not support HTML5 canvas.");
            return;
        }
		
		// add "#debug" to the URL to enable the debug Panel
		if (document.location.hash === "#debug") {
			window.onReady(function () {
				me.plugin.register.defer(debugPanel, "debug");
			});
		}

        // Initialize the audio.
        me.audio.init("mp3,oga");

        // Set a callback to run when loading is complete.
        me.loader.onload = this.loaded.bind(this);
     
        // Load the resources.
        me.loader.preload(game.resources);

        // Initialize melonJS and display a loading screen.
        me.state.set(me.state.LOADING, new myLoadingScreen());
        me.state.change(me.state.LOADING);

        
    },



    // Run on game resources loaded.
    "loaded" : function () {
        //me.state.set(me.state.MENU, new game.TitleScreen());
        me.state.set(me.state.PLAY, new game.PlayScreen());

        me.state.transition("fade", "#000000", 250);

        // add our player entity in the entity pool
        //me.entityPool.add("tilly", game.PlayerEntity);

        //// Collectibles
        //me.entityPool.add("SweetEntity", game.SweetEntity);
        //me.entityPool.add("JackEntity", game.JackEntity);
        //me.entityPool.add("BlockEntity", game.BlockEntity);
        //me.entityPool.add("ProjectileEntity", game.ProjectileEntity);


        //// Enemies
        //me.entityPool.add("WalkingEnemy", game.WalkingEnemy);
        //me.entityPool.add("FlyingEnemy", game.FlyingEnemy);


        // enable the keyboard
        //me.input.bindKey(me.input.KEY.LEFT, "left");
        //me.input.bindKey(me.input.KEY.RIGHT, "right");
        //me.input.bindKey(me.input.KEY.UP, "jump", true);
        //me.input.bindKey(me.input.KEY.X, "attack", true);
        //me.input.bindKey(me.input.KEY.Z, "throw", true);

        //me.input.bindKey(me.input.KEY.L, "levelskip", true); //function () { me.levelDirector.loadLevel("map2"); }.bind(this), true);

        //me.debug.renderHitBox = true;

        // Start the game.
        me.state.change(me.state.MENU);
    }

   
};


me.LevelEntity = me.ObjectEntity.extend(
    /** @scope me.LevelEntity.prototype */
    {
        /** @ignore */
        init: function (x, y, settings) {
            this.parent(x, y, settings);

            this.nextlevel = settings.to;

            this.fade = settings.fade;
            this.duration = settings.duration;
            this.fading = false;

            // a temp variable
            this.gotolevel = settings.to;
        },

        /**
         * @ignore
         */
        onFadeComplete: function () {
            me.levelDirector.loadLevel(this.gotolevel);
            me.game.viewport.fadeOut(this.fade, this.duration);
        },

        /**
         * go to the specified level
         * @name goTo
         * @memberOf me.LevelEntity
         * @function
         * @param {String} [level=this.nextlevel] name of the level to load
         * @protected
         */
        goTo: function (level) {
            this.gotolevel = level || this.nextlevel;
            // load a level
            //console.log("going to : ", to);
            if (this.fade && this.duration) {
                if (!this.fading) {
                    this.fading = true;
                    me.game.viewport.fadeIn(this.fade, this.duration,
                            this.onFadeComplete.bind(this));
                }
            } else {
                me.levelDirector.loadLevel(this.gotolevel);
            }
        },

        /** @ignore */
        onCollision: function (res,obj) {
            if (obj instanceof game.PlayerEntity) {
                this.goTo();
            }
        }
    });
