/// <reference path="../lib/melonJS-0.9.8.js" />

/* Game namespace */
var game = {

    // Run on page load.
    "onload" : function () {
        // Initialize the video.
        if (!me.video.init("screen", 1120, 608, true)) {
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
        me.entityPool.add("hero", game.Hero);
        me.entityPool.add("mob", game.Mob);

        //// Collectibles
        //me.entityPool.add("SweetEntity", game.SweetEntity);
        //me.entityPool.add("JackEntity", game.JackEntity);
        //me.entityPool.add("BlockEntity", game.BlockEntity);
        //me.entityPool.add("ProjectileEntity", game.ProjectileEntity);


        //// Enemies
        //me.entityPool.add("WalkingEnemy", game.WalkingEnemy);
        //me.entityPool.add("FlyingEnemy", game.FlyingEnemy);


        // enable the keyboard
        me.input.bindKey(me.input.KEY.LEFT, "rotleft", false);
        me.input.bindKey(me.input.KEY.RIGHT, "rotright", false);
        me.input.bindKey(me.input.KEY.UP, "moveup", false);
        me.input.bindKey(me.input.KEY.DOWN, "movedown", false);
        //me.input.bindKey(me.input.KEY.X, "attack", true);
        me.input.bindKey(me.input.KEY.Z, "push");

        //me.input.bindKey(me.input.KEY.L, "levelskip", true); //function () { me.levelDirector.loadLevel("map2"); }.bind(this), true);

        //me.debug.renderHitBox = true;

        // Start the game.
        me.state.change(me.state.PLAY);
    },

    

   
};


