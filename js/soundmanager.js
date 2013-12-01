function SoundInstance( soundObj, loop ){
	this.soundObj = soundObj;
}
function SoundManager( gameState ){
	var that = this;
	var soundCache = {};

	var AUDIO_OUT = 1;
	var AUDIO_IN  = 2;
	var AUDIO_STABLE = 0;

	this.audioState = AUDIO_STABLE;

	// Register all sounds loaded in gameState
	createjs.Sound.registerSound("res/sound/turkey_in_the_straw.mp3", "TitleMusic");
	createjs.Sound.registerSound("res/sound/Store/Waterford.mp3", "MarketMusic");
	createjs.Sound.registerSound("res/sound/GUI/click.mp3", "Click");
	createjs.Sound.registerSound("res/sound/GUI/buzz.mp3", "Error");
	createjs.Sound.registerSound("res/sound/Store/buy.mp3", "Buy");
	createjs.Sound.registerSound("res/sound/Store/entrance.mp3", "Entrance");
	createjs.Sound.registerSound("res/sound/GUI/ding.mp3", "Ding");
	createjs.Sound.registerSound("res/sound/Kitchen/Double_Beep.mp3", "BeepBeep");

	// Kitchen sound
	createjs.Sound.registerSound("res/sound/Kitchen/Oven_Door_Full_Open.mp3", "Oven_Door_Full_Open");
	createjs.Sound.registerSound("res/sound/Kitchen/Oven_Door_Full_Close.mp3", "Oven_Door_Full_Close");
	createjs.Sound.registerSound("res/sound/Kitchen/Oven_Door_Peek_Close.mp3", "Oven_Door_Peek_Close");
	createjs.Sound.registerSound("res/sound/Kitchen/Oven_Door_Peek_Open.mp3", "Oven_Door_Peek_Open");
	createjs.Sound.registerSound("res/sound/Kitchen/Explosion_Sound.mp3", "Explosion");

	createjs.Sound.registerSound("res/sound/Kitchen/Open_Cookbook.mp3", "Open_Cookbook");
	createjs.Sound.registerSound("res/sound/Kitchen/Close_Cookbook.mp3", "Close_Cookbook");


	this.backgroundSounds = [];
	this.backgroundSoundsQueue = [];
	this.fadeOut = function(){
		for ( var i in that.backgroundSounds ){
			that.backgroundSounds[i].audioState = AUDIO_OUT;
		}
	};
	this.play = function( soundName ){
		var channel = createjs.Sound.createInstance("Click");
		if( typeof soundName != "object" ){

			channel = ( soundCache[soundName] ? soundCache[soundName] : soundCache[soundName] = createjs.Sound.createInstance(soundName) );
			channel.volume = 1;
		}
		else{
			channel = ( soundCache[soundName.name] ? soundCache[soundName.name] : soundCache[soundName.name] = createjs.Sound.createInstance(soundName.name) );
			channel.volume = soundName.volume;
		}
		channel.play();
	};

	this.backgroundLoop = function( soundName ){
		var newBackgroundSound;
		if( typeof soundName != "object" ){
			newBackgroundSound = soundCache[soundName] ? soundCache[soundName] : soundCache[soundName] = createjs.Sound.createInstance(soundName);
		}
		else{
			newBackgroundSound = soundCache[soundName.name] ? soundCache[soundName.name] : soundCache[soundName.name] = createjs.Sound.createInstance( soundName.name );
			newBackgroundSound.setPosition(soundName.pos || 0);
			newBackgroundSound.volume = newBackgroundSound.desiredVolume = soundName.volume || 1;
			newBackgroundSound.play();

			// loop-de-loop
		 	newBackgroundSound.addEventListener("complete", function(){
		 		if( newBackgroundSound.volume == 0 ){ newBackgroundSound.stop(); return; }
		 		newBackgroundSound.setPosition(soundName.pos || 0);
		 		newBackgroundSound.volume = newBackgroundSound.desiredVolume = soundName.volume || 1;
     			newBackgroundSound.play();
     		});
		}
		that.backgroundSoundsQueue.push(newBackgroundSound);
	};

	gameState.pubsub.subscribe( "Play", this.play );
	gameState.pubsub.subscribe( "BackgroundLoop", this.backgroundLoop );
	gameState.pubsub.subscribe( "FadeOut", this.fadeOut );

	return {
		tick: function(){
			for ( var i in that.backgroundSounds ){
				if( that.backgroundSounds[i].audioState == AUDIO_OUT ){
					that.backgroundSounds[i].volume -=0.03;
				}
				if( that.backgroundSounds[i].audioState == AUDIO_IN ){
					that.backgroundSounds[i].volume +=0.03;
				}
				if( that.backgroundSounds[i].volume >= that.backgroundSounds[i].desiredVolume ){
					that.backgroundSounds[i].volume = that.backgroundSounds[i].desiredVolume;
				}
				if( that.backgroundSounds[i].volume <= 0.0 ){
					that.backgroundSounds[i].volume = 0;
					that.backgroundSounds[i].stop();
					that.backgroundSounds.splice( i, 1 );
				}
			}
			if( that.backgroundSounds.length == 0 ){
				for ( var i in that.backgroundSoundsQueue ){
					var newSound = that.backgroundSoundsQueue[i];
					newSound.audioState = AUDIO_IN;
					that.backgroundSounds.push( newSound );
				}
				that.backgroundSoundsQueue = [];
			}
		}
	}

}