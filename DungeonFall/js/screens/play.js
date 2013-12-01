/// <reference path="../../lib/melonJS-0.9.10.js" />
game.PlayScreen = me.ScreenObject.extend({

    init: function() {
        this.alwaysUpdate = true;

        me.game.viewport = new me.Viewport(0, 0, 1120, 608);
    },

	/**	
	 *  action to perform on state change
	 */
    onResetEvent: function () {
        // add our HUD to the game world        
        me.game.world.addChild(new game.HUD.Container());
        me.game.world.addChild(new game.HUD.FloatyTextContainer());

	    me.levelDirector.loadLevel("basedungeon");

	    var hero = new game.Hero(-32, 9 * 32, { name:"hero", image: "hero" });
	    //hero.init(-32, 9 * 32, { image: "hero" });
	    me.game.world.addChild(hero);
	    me.game.world.addChild(new game.Dungeon());
	    me.game.world.addChild(new game.FallingPiece());
	},
	
	
	/**	
	 *  action to perform when leaving this screen (state change)
	 */
	onDestroyEvent: function() {
	    // remove the HUD from the game world
	    me.game.world.removeChild(me.game.world.getEntityByProp("name", "HUD")[0]);
	    var ftc = me.game.world.getEntityByProp("name", "FloatyTextContainer")[0];
	    ftc.clear();
	    me.game.world.removeChild(ftc);
	},

	update: function() {
	    return true;
	},

    draw: function(context) {
        me.video.clearSurface(context, "#000000");
        //this.parent(context);
    }
});
