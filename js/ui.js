function ClockUI( stage, gameState ){
	var that = this;

	this.minuteRadius = 30;
	this.hourRadius = 0.7 * this.minuteRadius;
	this.clockX = 246;
	this.clockY = 146;

	this.getClockAngles = function( ){
		var currTime = new Date( gameState.currentTime );

		var hourAngle = 720 * ( currTime.getHours() / 24 ) - 90;
		var minuteAngle = 360 * ( currTime.getMinutes() / 60 ) - 90;
		return [ hourAngle, minuteAngle ];
	}

	var minuteLine = new createjs.Shape();
	minuteWidth = this.minuteRadius;
	minuteHeight = 1;
	minuteLine.graphics.beginFill('black').drawRect( 0, 0, minuteWidth, minuteHeight );
	minuteLine.regX = 0;
	minuteLine.regY = minuteHeight / 2;
	minuteLine.x = this.clockX;
	minuteLine.y = this.clockY;

	var hourLine = new createjs.Shape();
	hourWidth = this.hourRadius;
	hourHeight = 1;
	hourLine.graphics.beginFill('black').drawRect( 0, 0, hourWidth, hourHeight );
	hourLine.regX = 0;
	hourLine.regY = hourHeight / 2;
	hourLine.x = this.clockX;
	hourLine.y = this.clockY;

	stage.addChild( minuteLine );
	stage.addChild( hourLine );
	return {
		tick: function(){
			var angles = that.getClockAngles();
			hourLine.rotation = angles[0];
			minuteLine.rotation = angles[1];
		}
	}

}

function HelpUI( stage, gameState ){
	var that = this;
	this.showingHelp = false;
	var helpPages = [
		new createjs.Bitmap("res/screens/HelpCreditsScreen/HelpP1P2.png"),
		new createjs.Bitmap("res/screens/HelpCreditsScreen/HelpP3P4.png"),
		new createjs.Bitmap("res/screens/HelpCreditsScreen/HelpP5P6.png"),
		new createjs.Bitmap("res/screens/HelpCreditsScreen/HelpP7P8.png")
	];
	var position = 0;
	var helpImg = helpPages[0];
	var closeButton = new Button( stage, gameState, 708, 8, 80, 50,null, null, function(){ that.hideHelp(); } );
	var nextButton = new Button( stage, gameState, 645, 543, 80, 50, null,null, function(){ gameState.pubsub.publish("Play", "Open_Cookbook");
		if( helpImg ){
			position++;
			helpImg.visible = false;
			helpImg = helpPages[ position % 4 ];
			helpImg.visible = true;
		} else{
			that.hideHelp();
		}
	 });
	var prevButton = new Button( stage, gameState, 77, 546, 80, 50, null,null, function(){ gameState.pubsub.publish("Play", "Open_Cookbook");
		if( helpImg ){
			position--;
			helpImg.visible = false;
			helpImg = helpPages[ Math.abs(position) % 4 ];
			helpImg.visible = true;
		} else {
			that.hideHelp();
		}
	});

	this.hideHelp = function(){
		helpImg.visible=false;
		stage.removeChild( closeButton );
		stage.removeChild( nextButton );
		stage.removeChild( prevButton );
		for( var i in helpPages ){
			helpPages[i].visible = false;
			stage.removeChild( helpPages[i] );
		}
		that.showingHelp = false;
		gameState.pubsub.publish("Play", "Close_Cookbook");
	}

	// Show core temperature
	this.showHelp = function(){
		if( that.showingHelp ) return;
		gameState.pubsub.publish("Play", "Open_Cookbook");

		for( var i in helpPages ){
			helpPages[i].visible = false;
			stage.addChild( helpPages[i] );
		}

		helpPages[0].visible = true;
		stage.addChild( that.background );
		stage.addChild( closeButton );
		stage.addChild( nextButton );
		stage.addChild( prevButton );
		that.showingHelp = true;

	}

	// change temperature, this one's for the UI
    gameState.pubsub.subscribe( "ShowHelp", this.showHelp );
}


function FinalConfirmationUI(stage, gameState){
	var that = this;
	this.showingConfirm = false;

	var finalImg = new createjs.Bitmap("res/screens/KitchenScreen/FinalConfirmation.png");
	var yesButton = new Button( stage, gameState, 355, 338, 388, 50, null, null, function(){
		gameState.pubsub.publish( "Play", "Ding" );
		gameState.pubsub.publish( "SwitchScreen", "ScoreScreen" );
		that.hideFinalConfirm();
	} );
	var noButton = new Button( stage, gameState, 355, 395, 388, 50, null, null, function(){that.hideFinalConfirm();} );

	this.hideFinalConfirm = function(){
		stage.removeChild( finalImg );
		stage.removeChild( yesButton );
		stage.removeChild( noButton );
		that.showingConfirm = false;
	};

	// Show core temperature
	this.showFinalConfirm = function(){
		if(DEBUG) console.log("Showing final confirm");
		if( !that.showingConfirm ){
			that.showingConfirm = true;
			stage.addChild( finalImg );
			stage.addChild( noButton );
			stage.addChild( yesButton );
		}
	};

	// change temperature, this one's for the UI
    gameState.pubsub.subscribe( "ShowFinalConfirm", this.showFinalConfirm );
}

function DeathUI(stage, gameState){
	var that = this;
	this.showingConfirm = false;

	var finalImg = new createjs.Bitmap("res/screens/KitchenScreen/HouseFireRetry.png");
	var deathCount = new createjs.Text( UtilityFunctions.randRange(1,6), "24px Arial", "black" );
	deathCount.x = 695;
	deathCount.y = 260;

	var retryButton = new Button( stage, gameState, 578, 520, 200, 50, null, null, function(){
		document.location.reload();
	} );

	var explosion = { boom:{ frames:[35,34,33,32,31,30,29,28,27,26,25,24,23,22,21,20,19,18,17,16,15,14,13,12,11,10,9,8,7,6,5,4,3,2,1,0], next:false } };
	var data = {
    	images: ["res/screens/KitchenScreen/Explosion_AnimationLowRes.png"],
     	frames: { width:400, height:300 },
     	animations: explosion
 	};

 	var spriteSheet = new createjs.SpriteSheet(data);
 	var animation = new createjs.Sprite(spriteSheet, "treeAnimations");
 	animation.x = 0;
 	animation.y = 0;
 	animation.scaleX = animation.scaleY = 2;

	// Show core temperature
	this.showDeath = function(){
		gameState.pubsub.publish( "Play", {name:"Explosion", volume:1} );

		stage.addChild( finalImg );
	 	animation.gotoAndPlay("boom");
	 	stage.addChild( deathCount );
		stage.addChild( animation );
		stage.addChild( retryButton );
	};

	// change temperature, this one's for the UI
    gameState.pubsub.subscribe( "Death", this.showDeath );
}

function AlarmUI(stage, gameState){
	var that = this;
	this.showingConfirm = false;

	var oldTime = Date.now();
	var showColon = true;
	var timerText = new createjs.Text("00:00", "24px Arial", "black" );
	timerText.x = 372;
	timerText.y = 290;

	var clearButton = new Button( stage, gameState, 364, 327, 17, 13, null, null, function(){
		gameState.alarmTimer = 0;
		gameState.alarmActivated = false;
		that.updateTimer();
	} );

	var hourButton = new Button( stage, gameState, 386, 327, 24, 13, null, null, function(){
		gameState.alarmTimer +=3600;
		gameState.alarmActivated = true;
		that.updateTimer();
	} );
	var minuteButton = new Button( stage, gameState, 414, 327, 24, 13, null, null, function(){
		gameState.alarmTimer +=300;
		gameState.alarmActivated = true;
		that.updateTimer();
	} );

	this.updateTimer = function(){
		var colon = showColon ? ":" : " ";
		var totalSec = gameState.alarmTimer;
		var hours = parseInt( totalSec / 3600 ) % 24
		var minutes = parseInt( totalSec / 60 ) % 60;
		var timeText = ("00"+hours).slice(-2) + colon + ("00"+minutes).slice(-2);
		timerText.text = timeText;
	}

	this.activateTimer = function(){
		gameState.alarmActivated = true;
	}

	// Show core temperature

	stage.addChild( timerText );
	stage.addChild( clearButton );
	stage.addChild( hourButton );
	stage.addChild( minuteButton );


	this.updateTimer();

	return{
		tick: function(){
			// IMPORTANT: SECOND TIMER
    		var diff = Date.now() - oldTime;
			if( diff > 1000 ){
				if( gameState.alarmActivated && gameState.alarmTimer <=0 ){
					gameState.alarmTimer = 0;
					gameState.pubsub.publish("Play", "BeepBeep")
				}

    			that.updateTimer();
    			showColon = !showColon;

    			if( gameState.alarmActivated )
    				gameState.alarmTimer --;

    			oldTime = Date.now();
    		}
		}
	}


}

function CookbookUI( stage, gameState ){
	var that = this;
	this.showingCookbook = false;

	var cookbookImg = new createjs.Bitmap("res/screens/KitchenScreen/Cookbook-Open.png");
	var closeButton = new Button( stage, gameState, 710, 10, 100, 50, null, null, function(){that.hideCookbook();} );
	var turkeyTypeText = new createjs.Text("", "18px Arial", "black" );
	turkeyTypeText.x = 535;
	turkeyTypeText.y = 56;

	var turkeyWeightText = new createjs.Text("", "18px Arial", "black" );
	turkeyWeightText.x = 553;
	turkeyWeightText.y = 85;

	var logEntries = [];
	this.hideCookbook = function(){

		stage.removeChild( closeButton );
		stage.removeChild( cookbookImg );
		stage.removeChild( turkeyTypeText );
		stage.removeChild(turkeyWeightText);
		for( i in logEntries ){
			stage.removeChild(logEntries[i]);
		}
		that.showingCookbook = false;
		gameState.pubsub.publish("Play", "Close_Cookbook");
	}

	// Show core temperature
	this.showCookbook = function(){
		if( !that.showingCookbook ){
			stage.addChild( cookbookImg );
			stage.addChild( closeButton );

			turkeyTypeText.text = gameState.turkeyType;
			turkeyWeightText.text = gameState.turkeyWeight + " lbs";
			for( i in gameState.peekRecords ){
				var record = gameState.peekRecords[i];
				var time = new Date( gameState.peekRecords[i].getTime() );

				var logLine = new createjs.Text( "OFF", "12px Arial", "black" );

				logLine.x = 423;
				logLine.y = 50 * i+ 165;
				logLine.textBaseline = "alphabetic";
				logLine.text = record.getType() + "   " + ("00"+time.getHours()).slice(-2) + ":" + ("00"+time.getMinutes()).slice(-2) + "        " + record.getContent();

				logEntries.push(logLine);
				stage.addChild(logLine);
			}
			stage.addChild(turkeyTypeText);
			stage.addChild(turkeyWeightText);

			that.showingCookbook = true;
		}
	}

	// change temperature, this one's for the UI
    gameState.pubsub.subscribe( "ShowCookbook", this.showCookbook );

}

function OvenUI( stage, gameState ){
	var that = this;
	var OVEN_CLOSED = 0;
	var OVEN_PEEK = 1;
	var OVEN_OPEN = 2;

	this.ovenDoor = OVEN_CLOSED;
	var ovenLight = new createjs.Shape();
	ovenLight.graphics.beginFill( "black" ).drawCircle( 181, 126, 2 );

	var confirmation = new FinalConfirmationUI(stage, gameState );
	var death = new DeathUI(stage,gameState);

	// Oven light control
	this.changeOvenLight = function( state ){
		if( state == "On" ){
			ovenLight.visible = false;
		} else {
			ovenLight.visible = true;
		}
	}
	this.doneSkipTime = true;

	// place turkeys in oven
	for (i in gameState.turkeyStates){
		gameState.turkeyStates[i].alpha = 0;
		gameState.turkeyStates[i].scaleX = gameState.turkeyStates[i].scaleY =1;
		gameState.turkeyStates[i].x = 75;
		gameState.turkeyStates[i].y = 258;
	}

	var temperatureText = new createjs.Text( "OFF", "40px Arial", "#ff7700" );
	temperatureText.x = 50;
	temperatureText.y = 147;
	temperatureText.textBaseline = "alphabetic";

	var lightPressedImg = new createjs.Bitmap( "res/screens/KitchenScreen/LightButtonDepressed.png" );
	lightPressedImg.alpha = 0;

	var doorClosedLightOff = new createjs.Bitmap( "res/screens/KitchenScreen/DoorClosedLightOff.png" );
	doorClosedLightOff.alpha = 1;

	var doorClosedLightOn = new createjs.Bitmap( "res/screens/KitchenScreen/DoorClosedLightOn.png" );
	doorClosedLightOn.alpha = 0;

	var doorPeekLightOff = new createjs.Bitmap( "res/screens/KitchenScreen/DoorPeekLightOff.png" );
	doorPeekLightOff.alpha = 0;

	var doorPeekLightOn = new createjs.Bitmap( "res/screens/KitchenScreen/DoorPeekLightOn.png" );
	doorPeekLightOn.alpha = 0;

	var doorOpen = new createjs.Bitmap( "res/screens/KitchenScreen/DoorOpen.png" );
	doorOpen.alpha = 0;

	var redState = new createjs.Bitmap( "res/screens/KitchenScreen/OvenTurnRedState.png" );
	redState.alpha = 0;

	var panFront = new createjs.Bitmap( "res/screens/KitchenScreen/PanFront.png" );
	panFront.alpha = 0;

	this.changeTemperature = function( direction ){

		if( gameState.turkeyBought ){
			if( temperatureText.text == "OFF" && direction == "Up" ) temperatureText.text = "125";
			if( !( temperatureText.text == "OFF" && direction == "Down" ) ){

				var temp = ( direction == "Up" ? parseInt(temperatureText.text)+25 : parseInt(temperatureText.text)-25);

				 // Check lower bound for OFF
				 temp = temp < 150 ? temp = "OFF" : temp;

				 // Check upper bound
				 if( temp > 500 ){
				 	redState.alpha = ( temp - 500 )/( 1100 - 500 );
				 }

				 // if over 1100 F, burn house down
				 if( temp > 1100 ){
				 	gameState.pubsub.publish("Death","");
				 	return;
				 }

				 temperatureText.text = temp;
			}

			 // Tell our model to set the actual temperature
			 gameState.ovenModel.changeTemp( UtilityFunctions.F2C( temperatureText.text == "OFF" ? 125 : parseInt( temperatureText.text ) ) );
		}
		else{
			gameState.pubsub.publish("ShowDialog",{seq:"EmptyOven", autoAdvance: true});
		}
	}

	this.ovenLightToggle = function(){

		// Only work if the user bought an oven light
		if( gameState.boughtOvenLight ){
			lightPressedImg.alpha = lightPressedImg.alpha == 0 ? 1 : 0;
			if( that.ovenDoor == OVEN_CLOSED){
				doorClosedLightOn.alpha = lightPressedImg.alpha == 0 ? 0 : 1;
				doorClosedLightOff.alpha = lightPressedImg.alpha == 0 ? 1 : 0;
				doorOpen.alpha = 0;
			}
			else if( that.ovenDoor == OVEN_PEEK ){
				doorPeekLightOn.alpha = lightPressedImg.alpha == 0 ? 0 : 1;
				doorPeekLightOff.alpha = lightPressedImg.alpha == 0 ? 1 : 0;
				doorOpen.alpha = 0;
			}
		}
	}

	this.startTurkeyModel = function(){
		if(DEBUG) console.log("weight is" + gameState.turkeyWeight);
		gameState.ovenModel = new OvenModel( gameState.turkeyWeight, gameState );
	}

	var handleBar = new createjs.Shape();
 	handleBar.graphics.beginFill("#ffffff").drawRect(20, 190, 300, 20);
 	handleBar.alpha = 0.01;
 	handleBar.addEventListener( "mouseover", function(){ document.body.style.cursor='pointer'; } );
 	handleBar.addEventListener( "mouseout", function(){ document.body.style.cursor='default'; } );
 	handleBar.addEventListener( "pressup", handlePress );

    var evalSkin  = {
    	"raw": "The turkey looks no different from when I put it in",
    	"undercooked": "The skin looks pink",
    	"slightly cooked": "The turkey could use a couple more minutes",
    	"cooked": "The turkey looks good enough to eat",
    	"overcooked": "The turkey looks a bit shriveled",
    	"dry": "The turkey looks like cardboard",
    	"burnt": "The turkey looks burnt"
    };


	// Look for a drag event
	function handlePress(event) {
		if( event.stageY > 300 && that.ovenDoor != OVEN_OPEN ){
			that.ovenDoor = OVEN_OPEN;
			doorPeekLightOn.alpha = doorClosedLightOn.alpha = 0;
			doorPeekLightOff.alpha = doorClosedLightOff.alpha = 0;
			doorOpen.alpha = 1;
			handleBar.graphics.clear();
			handleBar.graphics.beginFill("#ffffff").drawRect(5, 450, 400, 60);
			handleBar.alpha = 0.01;

			if( gameState.turkeyBought ){
				var state = gameState.ovenModel.getTurkeyState();
				if(!evalSkin[turkeyState["skin"]["cond"][2]])
					gameState.pubsub.publish("Death","");
				gameState.pubsub.publish( "ShowDialog", {seq:"custom", autoAdvance:true, customText:evalSkin[turkeyState["skin"]["cond"][2]] + "." } );
				gameState.pubsub.publish( "AddRecord", {type:"Open ", text:"The turkey looked " + turkeyState["skin"]["cond"][2]} );
				//gameState.ovenModel.setRawTemp( (gameState.ovenModel.getRawTemp() - 25) < 150 ? 150 : gameState.ovenModel.getRawTemp() - 25 );
				gameState.ovenOpened++;
			}

			gameState.pubsub.publish( "Play", "Oven_Door_Full_Open" );
		}else if (that.ovenDoor == OVEN_OPEN ){
			that.ovenDoor = OVEN_PEEK;
			gameState.pubsub.publish( "Play", "Oven_Door_Full_Close" );
			handleBar.graphics.clear();
		 	handleBar.graphics.beginFill("#ffffff").drawRect(20, 190, 300, 20);
 			handleBar.alpha = 0.01;
			ovenPeek();
		}
	}

	handleBar.addEventListener( "click", ovenPeek );

	function ovenPeek(){
		if( that.ovenDoor == OVEN_CLOSED && that.ovenDoor != OVEN_OPEN ){
			gameState.pubsub.publish( "Play", "Oven_Door_Peek_Open" );
			doorPeekLightOn.alpha = lightPressedImg.alpha;
			doorPeekLightOff.alpha = !lightPressedImg.alpha;
			doorClosedLightOn.alpha = 0;
			doorClosedLightOff.alpha = 0;
			doorOpen.alpha = 0;
			that.ovenDoor = OVEN_PEEK;

			handleBar.y = 48;
			if( gameState.turkeyBought ){
				var state = gameState.ovenModel.getTurkeyState();
				if(!evalSkin[turkeyState["skin"]["cond"][2]])
					gameState.pubsub.publish("Death","");
				gameState.pubsub.publish( "ShowDialog", {seq:"custom", autoAdvance:true, customText:evalSkin[turkeyState["skin"]["cond"][2]] } );
				gameState.pubsub.publish( "AddRecord", {type:"Peek ", text:"The turkey looked " +turkeyState["skin"]["cond"][2]} );
			}
		}
		else if (that.ovenDoor == OVEN_PEEK){
			doorClosedLightOn.alpha = lightPressedImg.alpha;
			doorClosedLightOff.alpha = !lightPressedImg.alpha;
			doorPeekLightOn.alpha = 0;
			doorPeekLightOff.alpha = 0;
			that.ovenDoor = OVEN_CLOSED;
			gameState.pubsub.publish( "Play", "Oven_Door_Peek_Close" );
			doorOpen.alpha = 0;
			handleBar.y = 0;
		}
	}

	// Show core temperature
	this.showTempDialog = function(){
		if( that.ovenDoor != OVEN_OPEN ){
			gameState.pubsub.publish("ShowDialog", {seq:"OpenDoor", autoAdvance:true});
		}
		else{
			state = gameState.ovenModel.getTurkeyState();
			gameState.pubsub.publish( "ShowDialog", {seq:"custom", autoAdvance:true, customText:"The core temperature of the turkey reads " + UtilityFunctions.C2F(state.core.temp).toFixed(2) + " F" } );
			gameState.pubsub.publish( "AddRecord", {type:"Probe", text:"Core temperature measured: " + UtilityFunctions.C2F(state.core.temp).toFixed(2) + " F"} );
		}
	}

	new CookbookUI( stage, gameState );

	// change temperature, this one's for the UI
    gameState.pubsub.subscribe( "ChangeTemperature", this.changeTemperature );
    gameState.pubsub.subscribe( "ShowTempDialog", this.showTempDialog );
    gameState.pubsub.subscribe( "OvenLightToggle", this.ovenLightToggle );
	gameState.pubsub.subscribe( "OvenLight", this.changeOvenLight );
	gameState.pubsub.subscribe( "StartTurkeyModel", this.startTurkeyModel );
	gameState.pubsub.subscribe("DoneSkipTime", function(){ that.doneSkipTime = true; });

    this.secondTick = function(diff){
    		// check if oven door is open
    		gameState.ovenModel.secondTick();
    		gameState.currentTime += diff;
	}

	gameState.pubsub.subscribe( "SkipTime", function(){
		if(DEBUG) console.log("Skipping time");
		for(var i = 0; i < 1200; i++){
			that.secondTick( 1000 );
		}
		if( gameState.alarmActivated )
			gameState.alarmTimer -= 1200;
		gameState.pubsub.publish("DoneSkipTime","");
	});

    return {
    	tick: function(){
    		// IMPORTANT: SECOND TIMER
    		var diff = Date.now() - gameState.oldTime;
    		var dialoguediff = Date.now() - gameState.oldDialogueTime;
			if( diff > 1000 ){
    			that.secondTick( diff );

	    		if( gameState.turkeyBought ){
					gameState.turkeyCookCounter++;
					// what's the state of the turkey
					turkeyState = gameState.ovenModel.getTurkeyState();
					gameState.turkeyStates[0].alpha = 1;
					if( turkeyState["skin"]["cond"][0] == "Undercooked" )
						gameState.turkeyStates[1].alpha = turkeyState["skin"]["cond"][1];
					if( turkeyState["skin"]["cond"][0] == "Cooked" )
						gameState.turkeyStates[2].alpha = turkeyState["skin"]["cond"][1];
					if( turkeyState["skin"]["cond"][0] == "Dry" )
						gameState.turkeyStates[3].alpha = turkeyState["skin"]["cond"][1];
					if( turkeyState["skin"]["cond"][0] == "Burnt" )
						gameState.turkeyStates[4].alpha = turkeyState["skin"]["cond"][1];
					if( turkeyState["skin"]["cond"][0] == "Fire" )
						gameState.turkeyStates[4].alpha = 1;
				}
				gameState.oldTime = Date.now();
			}
			if( gameState.turkeyBought && dialoguediff > 2*60*1000 ){
					gameState.pubsub.publish( "ShowDialog", {seq:"Spouse gets surprise movie tickets", autoAdvance:true, random:true} );
					gameState.oldDialogueTime = Date.now();
			}
    	},
    	render: function(){
    		stage.addChild( redState );
		    stage.addChild( ovenLight );
		    stage.addChild( temperatureText );

		    stage.addChild( lightPressedImg);
			// Turkey goes here
				// did the player actually buy a turkey? if so, determine its cooked state
				if( gameState.turkeyBought ){

					// what's the state of the turkey
					turkeyState = gameState.ovenModel.getTurkeyState();
					gameState.turkeyStates[0].alpha = 1;
					if( turkeyState["skin"]["cond"] == "Undercooked" )
						gameState.turkeyStates[1].alpha = turkeyState["skin"]["cond"][1];
					if( turkeyState["skin"]["cond"] == "Cooked" )
						gameState.turkeyStates[2].alpha = turkeyState["skin"]["cond"][1];
					if( turkeyState["skin"]["cond"] == "Dry" )
						gameState.turkeyStates[3].alpha = turkeyState["skin"]["cond"][1];
					if( turkeyState["skin"]["cond"] == "Burnt" )
						gameState.turkeyStates[4].alpha = turkeyState["skin"]["cond"][1];
					if( turkeyState["skin"]["cond"] == "Fire" )
						gameState.turkeyStates[4].alpha = 1;

					panFront.alpha = 1;
					stage.addChild(gameState.turkeyStates[0]);
					for(i in gameState.turkeyStates){
						stage.addChild(gameState.turkeyStates[i]);
					}
					stage.addChild(panFront);
				}

			// Pan front goes here
			stage.addChild( panFront );

			//finalize button
			if( gameState.turkeyBought ){
				stage.addChild( new Button( stage, gameState, 45, 250, 250, 150, null, null, function(){
					gameState.pubsub.publish("Play", "Error");
					gameState.pubsub.publish("ShowFinalConfirm","");
				} ) );
			}

			stage.addChild( doorPeekLightOn);
		    stage.addChild( doorPeekLightOff);

		    stage.addChild( doorClosedLightOn);
		    stage.addChild( doorClosedLightOff);

		    stage.addChild( doorOpen);
		    stage.addChild( new Button( stage, gameState, 45, 163, 41, 17, "ChangeTemperature", "Up" ) );
		    stage.addChild( new Button( stage, gameState, 95, 163, 41, 17, "ChangeTemperature", "Down" ) );
		    stage.addChild( new Button( stage, gameState, 145, 163, 41, 17, "OvenLightToggle", "" ) );
		    if( gameState.hard == false )
		    	stage.addChild( new Button( stage, gameState, 205, 105, 80, 80, null, null, function(){
		    		if( that.doneSkipTime ){
		    			gameState.pubsub.publish("SkipTime","");
		    			that.doneSkipTime = false;
		    		}
		    	}) );
			stage.addChild( handleBar);

    		return this;
    	}
	}
}

function WindowUI( stage, gameState ){

	var dayNight = new createjs.Bitmap("res/screens/Window/Test4-217.png");
	var mood = new createjs.Bitmap("res/screens/Window/Test4TransparencyFull.png");

 	var smallWindows= [ new createjs.Bitmap("res/screens/Window/Small1.png"),
 			  	 	   	new createjs.Bitmap("res/screens/Window/Small2.png"),
 				 	  	new createjs.Bitmap("res/screens/Window/Small3.png"),
 				   		new createjs.Bitmap("res/screens/Window/Small4.png"),
 				   		new createjs.Bitmap("res/screens/Window/Small5.png")
 				 ];

 	var windows= [ new createjs.Bitmap("res/screens/Window/Win1.png"),
 				   new createjs.Bitmap("res/screens/Window/Win2.png"),
 				   new createjs.Bitmap("res/screens/Window/Win3.png"),
 				   new createjs.Bitmap("res/screens/Window/Win4.png"),
 				   new createjs.Bitmap("res/screens/Window/Win5.png"),
 				   new createjs.Bitmap("res/screens/Window/Win6.png"),
 				   new createjs.Bitmap("res/screens/Window/Win7.png"),
 				   new createjs.Bitmap("res/screens/Window/Win8.png"),
 				   new createjs.Bitmap("res/screens/Window/Win9.png"),
 				   new createjs.Bitmap("res/screens/Window/Win10.png"),
 				   new createjs.Bitmap("res/screens/Window/Win11.png")
 				 ];

	mood.y=30;
	dayNight.y=30;

	var secondCounter = 0;
	mood.x = dayNight.x = -(new Date( gameState.currentTime ).getHours()*682.625);

	var ground = new createjs.Bitmap( "res/screens/Window/Ground.png" );
	var houses = new createjs.Bitmap( "res/screens/Window/Housefar.png" );
	var streetLight = new createjs.Bitmap( "res/screens/Window/StreetlightGlow.png" );
	streetLight.alpha = 0;

	var treeAnimations = { rustle:{ frames:[0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17], next:false } };
	var data = {
    	images: ["res/screens/Window/Tree_Animation.png"],
     	frames: { width:386, height:287 },
     	animations: treeAnimations
 	};
	var spriteSheet = new createjs.SpriteSheet(data);
 	var animation = new createjs.Sprite(spriteSheet);
 	animation.x = 415;
 	animation.y = 30;

	// Fast forward, move sky
	gameState.pubsub.subscribe( "SkipTime", function(){
		var newpos =  -(new Date( gameState.currentTime ).getHours()*682.625);
		 dayNight.x = mood.x =newpos < -15583 ? 0 : newpos;
	});

    stage.addChild( dayNight );
    stage.addChild( ground );
    stage.addChild( houses );
    stage.addChild( animation );
    stage.addChild( mood );

    for( var i in smallWindows ){
    	smallWindows[i].visible = UtilityFunctions.randRange(0,1);
    	streetLight.alpha= 1;
    	stage.addChild( smallWindows[i] );
    }

    for( var i in windows ){
    	windows[i].visible = UtilityFunctions.randRange(0,1);
    	streetLight.alpha= 1;
    	stage.addChild( windows[i] );
    }
    stage.addChild( streetLight );
return {

	tick: function(){

		// move the sky
		secondCounter++;
		if( secondCounter > 600 ){
			dayNight.x-=11.38;
			mood.x -= 11.38;
			secondCounter = 0;

			// move tree sometimes
			if( UtilityFunctions.randRange(0,1) )
				animation.gotoAndPlay("rustle");

			// turn on lights
			if( dayNight.x < 0 && dayNight.x > -4545 ){
				for( var i in smallWindows ){
					smallWindows[i].visible = UtilityFunctions.randRange(0,1);
				}
				for( var i in windows ){
					windows[i].visible = UtilityFunctions.randRange(0,1);
				}

				// turn on random window lights
				streetLight.alpha= 1;
			}
			else if( dayNight.x < -11687 ){
				for( var i in smallWindows ){
					smallWindows[i].visible = UtilityFunctions.randRange(0,1);
				}
				for( var i in windows ){
					windows[i].visible = UtilityFunctions.randRange(0,1);
				}
				streetLight.alpha= 1;
			}
			// daytime, turn off all lights
			else{
				for( var i in smallWindows ){
					smallWindows[i].visible = 0;
				}
				for( var i in windows ){
					windows[i].visible = 0;
				}

				streetLight.alpha= 0;
			}
		}

		// if too many lights are causing an issue on your browser, turn them off
		if( createjs.Ticker.getMeasuredFPS().toFixed(1) < 13 ){
			for( var i in windows ){
				windows[i].visible = 0;
			}
		}

		if( dayNight.x < -15583 )
			dayNight.x = 0;

	}
}
}

function MarketItem( gameState, name, x, y, cost, mouseOutImg, mouseOverImg, mouseOutKitchenImg, mouseOverKitchenImg, funnyDescription, weight ){
	var that = this;
		this.name = name;
		this.bought = false;

		var mouseOverKitchen = new createjs.Bitmap( mouseOverKitchenImg );
		var mouseOutKitchen = new createjs.Bitmap( mouseOutKitchenImg );

		var mouseOver = new createjs.Bitmap( mouseOverImg );
		var mouseOut = new createjs.Bitmap( mouseOutImg );

		mouseOver.x = mouseOut.x = x;
		mouseOver.y = mouseOut.y = y;
	 	mouseOut.addEventListener( "mouseover", function(){
	 		document.body.style.cursor='pointer';
	 		mouseOver.visible = true;
	 		mouseOut.visible = false;
	 		gameState.pubsub.publish("ShowPrice", cost );
	 		gameState.pubsub.publish("ShowDesc", {title:that.name, desc:funnyDescription, weight:weight} );
	 	});
 		mouseOut.addEventListener( "mouseout", function(){
 			document.body.style.cursor='default';
 			mouseOver.visible = false;
 			mouseOut.visible = true;
 			gameState.pubsub.publish("ClearClipboard", {});
 		} );
 		mouseOver.addEventListener( "mouseover", function(){
 			document.body.style.cursor='pointer';
 			mouseOver.visible = true;
 			mouseOut.visible = false;
 			gameState.pubsub.publish("ShowPrice", cost );
 			gameState.pubsub.publish("ShowDesc", {title:that.name, desc:funnyDescription, weight:weight} );
 		});
 		mouseOver.addEventListener( "mouseout", function(){
 			document.body.style.cursor='default';
 			mouseOver.visible = false;
 			mouseOut.visible = true;
 			gameState.pubsub.publish("ClearClipboard", {});
 		} );


	 	mouseOutKitchen.addEventListener( "mouseover", function(){
	 		document.body.style.cursor='pointer';
	 		mouseOverKitchen.visible = true;
	 		mouseOutKitchen.visible = false;
	 	});
 		mouseOutKitchen.addEventListener( "mouseout", function(){
 			document.body.style.cursor='default';
 			mouseOverKitchen.visible = false;
 			mouseOverKitchen.visible = true;
 		} );
 		mouseOverKitchen.addEventListener( "mouseover", function(){
 			document.body.style.cursor='pointer';
 			mouseOverKitchen.visible = true;
 			mouseOutKitchen.visible = false;
 		});
 		mouseOverKitchen.addEventListener( "mouseout", function(){
 			document.body.style.cursor='default';
 			mouseOverKitchen.visible = false;
 			mouseOutKitchen.visible = true;
 		} );

 		// We've bought the item, now we click it in the Kitchen
 		mouseOverKitchen.addEventListener("click",function(){
 			if ( that.name.indexOf("Temperature") != -1 ){
 				gameState.pubsub.publish( "ShowTempDialog", "" );
 			}

 			if ( that.name.indexOf("Cookbook") != -1 ){
 				if(DEBUG) console.log("click, show cookbook");
 				gameState.pubsub.publish("ShowCookbook","");
 				gameState.pubsub.publish("Play", "Open_Cookbook");
 			}
 		});

 		mouseOver.addEventListener( "click", function(){
 			if(!that.bought && cost <= gameState.wallet ){

	 			if( that.name.indexOf("Turkey") != -1 && gameState.turkeyBought != true ){
	 				gameState.turkeyBought = true;
	 				gameState.turkeyWeight = weight;
	 				gameState.turkeyType = that.name;
				    gameState.marketItems[ that.name ].delete();
				    that.bought = true;

				    // record time started
    				gameState.pubsub.publish( "AddRecord", {type:"Note ", text:"Turkey bought and placed in oven"} );


				    gameState.pubsub.publish("Play", {name:"Buy", volume:0.7} );
				    gameState.pubsub.publish("WalletAmount", gameState.wallet - Math.abs(cost))
				    gameState.pubsub.publish("StartTurkeyModel","");
	 			}
	 			// can we buy this? Only possible if you already bought a turkey
	 			else if( that.name.indexOf("Turkey") == -1 && gameState.turkeyBought == true ){

	 				// if we bought an oven light, enable it!
	 				if( that.name.indexOf("Light") != -1 ) gameState.boughtOvenLight = true;

	 				// if we bought a clock, enable it!
	 				if( that.name.indexOf("Alarm") != -1 ) gameState.alarmBought = true;
	 				if( that.name.indexOf("Frills") != -1 ) gameState.frillsModifier = 5;

	 				if( that.name.indexOf("Exquisite") != -1 ){ gameState.stuffingTypeModifier = gameState.stuffingTypeModifier > 1.08 ? gameState.stuffingTypeModifier : 1.08; }
	 				if( that.name.indexOf("Special") != -1 ){ gameState.stuffingTypeModifier = gameState.stuffingTypeModifier > 1.17 ? gameState.stuffingTypeModifier : 1.17; }
	 				if( that.name.indexOf("Repurposed") != -1 ){ gameState.stuffingTypeModifier = gameState.stuffingTypeModifier > 1.05 ? gameState.stuffingTypeModifier : 1.05; }

		 			gameState.purchasedItems.push( objReturn );
		 			gameState.marketItems[ that.name ].delete();
		 			that.bought = true;
		 			gameState.pubsub.publish("Play", {name:"Buy", volume:0.7});
		 			gameState.pubsub.publish("WalletAmount", gameState.wallet - Math.abs(cost));
		 		}
		 		// One turkey only
		 		else if( that.name.indexOf("Turkey") != -1 && gameState.turkeyBought == true ){
		 			gameState.pubsub.publish( "ShowDialog", {seq:"CannotBuyTurkey", autoAdvance:true} );
		 			gameState.pubsub.publish( "Play", "Error" );
		 		}
		 		// Buy turkey first
		 		else{
		 			gameState.pubsub.publish( "ShowDialog", {seq:"BuyTurkeyFirst", autoAdvance:false} );
		 			gameState.pubsub.publish( "Play", "Error" );
		 		}
 			}
 			else{
 				gameState.pubsub.publish( "ShowDialog", {seq:"NoMoney", autoAdvance:true} );
	 			gameState.pubsub.publish( "Play", "Error" );
	 		}
 		});
 		mouseOver.visible = false;

 	var objReturn = {
		tick: function(){},
		getName: function(){return that.name;},
		delete: function( stage ){
			that.visible = false;
			gameState.pubsub.publish("RemoveItems", [mouseOut, mouseOver]);
		},
		draw: function( stage, newx, newy ){
			if( newx && newy ){
				mouseOut.x = mouseOver.x = newx;
				mouseOut.y = mouseOver.y = newy;
			}
			if(DEBUG) console.log("NewScreen for item "+that.name +" is " +gameState.newScreen );
			if( gameState.newScreen == "KitchenScreen" ){
				mouseOutKitchen.visible = true;
				stage.addChild( mouseOutKitchen );
				mouseOverKitchen.visible = false;
	    		stage.addChild( mouseOverKitchen );
	    		return;
			}

			if( !that.bought ){
				stage.addChild( mouseOut );
	    		stage.addChild( mouseOver );
	    	}
		}
	}
	return objReturn;
}



function ImgButton( stage, gameState, x, y, mouseOutImg, mouseOverImg, eventCmd, arg, sound, altfunc ){
		var mouseOver = new createjs.Bitmap( mouseOverImg );
		var mouseOut = new createjs.Bitmap( mouseOutImg );
		mouseOver.x = mouseOut.x = x;
		mouseOver.y = mouseOut.y = y;
	 	mouseOut.addEventListener( "mouseover", function(){ document.body.style.cursor='pointer'; mouseOver.visible = true; mouseOut.visible = false;  } );
 		mouseOut.addEventListener( "mouseout", function(){ document.body.style.cursor='default'; mouseOver.visible = false; mouseOut.visible = true; } );
 		mouseOver.addEventListener( "mouseover", function(){ document.body.style.cursor='pointer'; mouseOver.visible = true; mouseOut.visible = false;  } );
 		mouseOver.addEventListener( "mouseout", function(){ document.body.style.cursor='default'; mouseOver.visible = false; mouseOut.visible = true; } );
 		mouseOver.addEventListener( "click", function(){
 			if( sound ){
 				gameState.pubsub.publish("Play", sound );
 			}
 			if( !altfunc){
 				gameState.pubsub.publish( eventCmd, arg );
 				return;
 			}
 			altfunc();
 		} );
 		mouseOver.visible = false;
    	stage.addChild( mouseOut );
    	stage.addChild( mouseOver );

	return {
		tick: function(){}
	}
}

function Button( stage, gameState, x_orig, y_orig, x_dest, y_dest, eventCmd, arg, altfunc ){
	var that = this;
	if(DEBUG) console.log("button clicked with "+ arg);

	var button = new createjs.Shape();
 	button.graphics.beginFill("#ffffff").drawRect(x_orig, y_orig, x_dest, y_dest);
 	button.alpha = 0.01;
 	button.addEventListener( "click", function(){
 		gameState.pubsub.publish( "Play", "Click" );
		if( !altfunc ){
			gameState.pubsub.publish( eventCmd, arg );
			return;
		}
		altfunc();
 		gameState.pubsub.publish( eventCmd, arg );
	 } );
 	button.addEventListener( "mouseover", function(){ document.body.style.cursor='pointer'; } );
 	button.addEventListener( "mouseout", function(){ document.body.style.cursor='default'; } );
	return button;
}
