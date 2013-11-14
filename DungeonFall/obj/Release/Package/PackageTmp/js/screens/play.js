game.PlayScreen = me.ScreenObject.extend({

    init: function() {
        this.alwaysUpdate = true;
    },

	/**	
	 *  action to perform on state change
	 */
	onResetEvent: function() {	
	    me.levelDirector.loadLevel("basedungeon");

	    // add our HUD to the game world        
	    me.game.world.addChild(new game.HUD.Container());
	    me.game.world.addChild(new game.Dungeon());
	    me.game.world.addChild(new game.FallingPiece());
	},
	
	
	/**	
	 *  action to perform when leaving this screen (state change)
	 */
	onDestroyEvent: function() {
	    // remove the HUD from the game world
	    me.game.world.removeChild(me.game.world.getEntityByProp("name", "HUD")[0]);
	},

	update: function() {
	    return true;
	},

    draw: function(context) {
        me.video.clearSurface(context, "#000000");
        //this.parent(context);
    }
});
