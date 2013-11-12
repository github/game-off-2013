myLoadingScreen = me.ScreenObject.extend({
    /*---
    
        constructor
        
        ---*/
    init: function () {
        this.parent(true);

        // flag to know if we need to refresh the display
        this.invalidate = false;

        // handle for the susbcribe function
        this.handle = null;

    },

    // call when the loader is resetted
    onResetEvent: function () {
        // melonJS logo
        this.logo1 = new me.Font('impact', 32, 'white', 'middle');
        this.logo1.textBaseline = "alphabetic";

        // setup a callback
        this.handle = me.event.subscribe(me.event.LOADER_PROGRESS, this.onProgressUpdate.bind(this));

        // load progress in percent
        this.loadPercent = 0;
    },

    // destroy object at end of loading
    onDestroyEvent: function () {
        // "nullify" all fonts
        this.logo1 = null;
        // cancel the callback
        if (this.handle) {
            me.event.unsubscribe(this.handle);
            this.handle = null;
        }
    },

    // make sure the screen is refreshed every frame 
    onProgressUpdate: function (progress) {
        this.loadPercent = progress;
        this.invalidate = true;
    },

    // make sure the screen is refreshed every frame 
    update: function () {
        if (this.invalidate === true) {
            // clear the flag
            this.invalidate = false;
            // and return true
            return true;
        }
        // else return false
        return false;
    },

    /*---
    
        draw function
      ---*/

    draw: function (context) {

        // measure the logo size
        var logo1_width = this.logo1.measureText(context, "loading...").width;
        var xpos = (me.video.getWidth() - logo1_width)/2;
        var ypos = me.video.getHeight() / 2;

        // clear surface
        me.video.clearSurface(context, "black");

        // draw the melonJS logo
        this.logo1.draw(context, 'loading...', xpos, ypos);
        xpos += logo1_width;

        ypos += this.logo1.measureText(context, "loading...").height / 2;

        // display a progressive loading bar
        var progress = Math.floor(this.loadPercent * 300);

        // draw the progress bar
        context.strokeStyle = "silver";
        context.strokeRect((me.video.getWidth()/2) - 150, ypos, 300, 6);
        context.fillStyle = "#89b002";
        context.fillRect((me.video.getWidth() / 2) - 148, ypos + 2, progress - 4, 2);
    }

});