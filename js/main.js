// Robert- Here be dragons

var DEBUG = 1;

function GameState(){
	var that = this;

	this.pubsub = {};
	BindPubSub( this.pubsub );
	this.currentTime = Date.now(); //new Date().getTime();
    this.startTime =  Date.now();//new Date().getTime()
	this.oldTime =  Date.now();// new Date().getTime();
    this.oldDialogueTime =  Date.now();//new Date().getTime();

    this.gameStarted = false;
	this.name = "";
	this.gender = "Male";
    this.pronoun = "he";
	this.wallet = 45.00;
	this.hard = false;
	this.boughtOvenLight = false;
	this.turkeyWeight = 8;
    this.peekRecords = [];
    this.turkeyCooking = false;
    this.turkeyType = "";
    this.alarmTimer = 0;
    this.alarmBought = false;
    this.alarmActivated = false;
    this.turkeyCookCounter = 0;

    // stats
    this.storeVisits = 0;
    this.dialogueHeard = 0;
    this.ovenOpened = 0;

    // modifiers
    this.turkeyTypeModifier = 1;
    this.stuffingTypeModifier = 1;
    this.frillsModifier = 1;
    this.hardcoreModifier = 1;

    // Game State flags
    this.turkeyBought = false;
    var randomWeight = [ (UtilityFunctions.randRange(10,22)+"."+UtilityFunctions.randRange(10,99)),
                         (UtilityFunctions.randRange(10,22)+"."+UtilityFunctions.randRange(10,99)),
                         (UtilityFunctions.randRange(10,22)+"."+UtilityFunctions.randRange(10,99)),
                         (UtilityFunctions.randRange(10,22)+"."+UtilityFunctions.randRange(10,99)),
                         (UtilityFunctions.randRange(10,22)+"."+UtilityFunctions.randRange(10,99))
                        ];

    // Load all our resources:
    var queue = new createjs.LoadQueue(true);
    queue.addEventListener("progress", function(event){
    	that.pubsub.publish("Load", (event.progress*100/25));
    });

    that.mainUI = new GameUI( "demoCanvas", that );
    createjs.Ticker.addEventListener( "tick", gameLoop );
    queue.addEventListener("complete", function(event){
    	// Finished loading
    });
    queue.installPlugin(createjs.Sound);

    //
    queue.loadFile( {id: "DialogueBoxFile", src:"res/screens/GUI/DialogueBox.png"} );
    /*queue.loadFile( {id:"res/screens/LoadingScreen/Turkey0.png", src: "res/screens/LoadingScreen/Turkey0.png"} );
    queue.loadFile( {id:"res/screens/LoadingScreen/Turkey25.png", src: "res/screens/LoadingScreen/Turkey25.png"} );
    queue.loadFile( {id:"res/screens/LoadingScreen/Turkey50.png", src: "res/screens/LoadingScreen/Turkey50.png"} );
    queue.loadFile( {id:"res/screens/LoadingScreen/Turkey75.png", src: "res/screens/LoadingScreen/Turkey75.png"} );
    queue.loadFile( {id:"res/screens/LoadingScreen/TurkeyDone.png", src: "res/screens/LoadingScreen/TurkeyDone.png"} );*/

    // Screens
    queue.loadFile( {id: "res/screens/DifficultyScreen/Difficulty-Selection.png", src:"res/screens/DifficultyScreen/Difficulty-Selection.png"} );
    queue.loadFile( {id: "res/screens/DifficultyScreen/ButtonsandText.png", src:"res/screens/DifficultyScreen/ButtonsandText.png"} );
	queue.loadFile( {id: "res/screens/DifficultyScreen/ButtonMale.png", src:"res/screens/DifficultyScreen/ButtonMale.png"} );
    queue.loadFile( {id: "res/screens/DifficultyScreen/ButtonFemale.png", src:"res/screens/DifficultyScreen/ButtonFemale.png"} );

    // Load image assets
    queue.loadFile( {id: "TurkeySpriteFile", src:"res/screens/MainScreen/TurkeySprite.png"} );
    queue.loadFile( {id: "MainBackgroundFile", src:"res/screens/MainScreen/Main-Screen.png"} );
    queue.loadFile( {id: "OverlayGrassFile", src:"res/screens/MainScreen/Grass.png"} );
    queue.loadFile( {id: "StartButtonFile", src:"res/screens/MainScreen/ButtonStart.png"} );
    queue.loadFile( {id: "HelpButtonFile", src:"res/screens/MainScreen/ButtonHelp.png"} );
    queue.loadFile( {id: "CreditsButtonFile", src:"res/screens/MainScreen/ButtonCredits.png"} );

    queue.loadFile( {id: "CreditsScreenFile", src:"res/screens/HelpCreditsScreen/Credits.png" } );
    queue.loadFile( {id: "HelpP1P2", src:"res/screens/HelpCreditsScreen/HelpP1P2.png" } );
    queue.loadFile( {id: "HelpP3P4", src:"res/screens/HelpCreditsScreen/HelpP3P4.png" } );
    queue.loadFile( {id: "HelpP5P6", src:"res/screens/HelpCreditsScreen/HelpP5P6.png" } );
    queue.loadFile( {id: "HelpP7P8", src:"res/screens/HelpCreditsScreen/HelpP7P8.png" } );
    queue.loadFile( {id: "HelpP9P10", src:"res/screens/HelpCreditsScreen/HelpP9P10.png" } );


    queue.loadFile( {id: "ScoreScreenFile", src:"res/screens/ScoreScreen/Score-Evaluation-1.png" } );
    queue.loadFile( {id: "ScoreScreenFile", src:"res/screens/ScoreScreen/Score-Evaluation-2.png" } );

    queue.loadFile( {id: "MarketScreenfile", src:"res/screens/MarketScreen/MarketScreen.png"} );

    // Load sound assets
    queue.loadFile( {id: "TitleMusicFile", src:"res/sound/turkey_in_the_straw.mp3"} );
	queue.loadFile( {id: "MarketSoundFile", src:"res/sound/Store/Waterford.mp3"} );

	// UI sounds
    queue.loadFile( {id: "UIClickFile", src:"res/sound/GUI/click.mp3"} );
    queue.loadFile( {id: "UIBuzzFile", src:"res/sound/GUI/buzz.mp3"} );
    queue.loadFile( {id: "UIDingFile", src:"res/sound/GUI/ding.mp3"} );

    // Kitchen Items
    queue.loadFile( {id: "res/screens/KitchenScreen/KitchenScreen.png", src:"res/screens/KitchenScreen/KitchenScreen.png"});
    queue.loadFile( {id: "res/screens/KitchenScreen/FinalConfirmation.png", src:"res/screens/KitchenScreen/FinalConfirmation.png"});
	queue.loadFile( {id: "res/screens/KitchenScreen/TurkeyState1Small.png", src:"res/screens/KitchenScreen/TurkeyState1Small.png"});
	queue.loadFile( {id: "res/screens/KitchenScreen/TurkeyState2Small.png", src:"res/screens/KitchenScreen/TurkeyState2Small.png"});
	queue.loadFile( {id: "res/screens/KitchenScreen/TurkeyState3Small.png", src:"res/screens/KitchenScreen/TurkeyState3Small.png"});
	queue.loadFile( {id: "res/screens/KitchenScreen/TurkeyState4Small.png", src:"res/screens/KitchenScreen/TurkeyState4Small.png"});
	queue.loadFile( {id: "res/screens/KitchenScreen/TurkeyState5Small.png", src:"res/screens/KitchenScreen/TurkeyState5Small.png"});

	queue.loadFile( {id: "res/screens/KitchenScreen/CookbookKitchenGlow.png", src:"res/screens/KitchenScreen/CookbookKitchenGlow.png"});
	queue.loadFile( {id: "res/screens/KitchenScreen/CookbookKitchen.png", src:"res/screens/KitchenScreen/CookbookKitchen.png"});

	queue.loadFile( {id: "res/screens/KitchenScreen/AlarmKitchenGlow.png", src:"res/screens/KitchenScreen/AlarmKitchenGlow.png"});
	queue.loadFile( {id: "res/screens/KitchenScreen/AlarmKitchen.png", src:"res/screens/KitchenScreen/AlarmKitchen.png"});

	queue.loadFile( {id: "res/screens/KitchenScreen/TempProbeKitchenGlow.png", src:"res/screens/KitchenScreen/TempProbeKitchenGlow.png"});
	queue.loadFile( {id: "res/screens/KitchenScreen/TempProbeKitchen.png", src:"res/screens/KitchenScreen/TempProbeKitchen.png"});

	queue.loadFile( {id: "res/screens/KitchenScreen/StuffingSpecialKitchenGlow.png", src:"res/screens/KitchenScreen/StuffingSpecialKitchenGlow.png"});
	queue.loadFile( {id: "res/screens/KitchenScreen/StuffingSpecialKitchen.png", src:"res/screens/KitchenScreen/StuffingSpecialKitchen.png"});

	queue.loadFile( {id: "res/screens/KitchenScreen/StuffingRepurposedKitchenGlow.png", src:"res/screens/KitchenScreen/StuffingRepurposedKitchenGlow.png"});
	queue.loadFile( {id: "res/screens/KitchenScreen/StuffingRepurposedKitchen.png", src:"res/screens/KitchenScreen/StuffingRepurposedKitchen.png"});

	queue.loadFile( {id: "res/screens/KitchenScreen/StuffingExquisiteKitchenGlow.png", src:"res/screens/KitchenScreen/StuffingExquisiteKitchenGlow.png"});
	queue.loadFile( {id: "res/screens/KitchenScreen/StuffingExquisiteKitchen.png", src:"res/screens/KitchenScreen/StuffingExquisiteKitchen.png"});

	queue.loadFile( {id: "res/screens/KitchenScreen/StoreBrochureGlow.png", src:"res/screens/KitchenScreen/StoreBrochureGlow.png"});
	queue.loadFile( {id: "res/screens/KitchenScreen/StoreBrochure.png", src:"res/screens/KitchenScreen/StoreBrochure.png"});

	queue.loadFile( {id: "res/screens/KitchenScreen/FrillsBoxKitchenGlow.png", src:"res/screens/KitchenScreen/FrillsBoxKitchenGlow.png"});
	queue.loadFile( {id: "res/screens/KitchenScreen/FrillsBoxKitchen.png", src:"res/screens/KitchenScreen/FrillsBoxKitchen.png"});

	queue.loadFile( {id: "res/screens/KitchenScreen/DoorPeekLightOn.png", src:"res/screens/KitchenScreen/DoorPeekLightOn.png"});
	queue.loadFile( {id: "res/screens/KitchenScreen/DoorPeekLightOff.png", src:"res/screens/KitchenScreen/DoorPeekLightOff.png"});

	queue.loadFile( {id: "res/screens/KitchenScreen/DoorOpen.png", src:"res/screens/KitchenScreen/DoorOpen.png"});
	queue.loadFile( {id: "res/screens/KitchenScreen/DoorClosedLightOn.png", src:"res/screens/KitchenScreen/DoorClosedLightOn.png"});
	queue.loadFile( {id: "res/screens/KitchenScreen/DoorClosedLightOff.png", src:"res/screens/KitchenScreen/DoorClosedLightOff.png"});

	queue.loadFile( {id: "res/screens/KitchenScreen/PanFront.png", src:"res/screens/KitchenScreen/PanFront.png"});
	queue.loadFile( {id: "res/screens/KitchenScreen/OvenTurnRedState.png", src:"res/screens/KitchenScreen/OvenTurnRedState.png"});
	queue.loadFile( {id: "res/screens/KitchenScreen/LightButtonDepressed.png", src:"res/screens/KitchenScreen/LightButtonDepressed.png"});
    queue.loadFile( {id: "res/screens/KitchenScreen/Cookbook-Open.png", src:"res/screens/KitchenScreen/Cookbook-Open.png"});
    queue.loadFile( {id: "res/screens/KitchenScreen/Explosion_AnimationLowRes.png", src:"res/screens/KitchenScreen/Explosion_AnimationLowRes.png"});


	// Kitchen Sounds
	queue.loadFile( {id: "res/sound/Kitchen/Oven_Door_Full_Open.mp3", src:"res/sound/Kitchen/Oven_Door_Full_Open.mp3"});
	queue.loadFile( {id: "res/sound/Kitchen/Oven_Door_Full_Close.mp3", src:"res/sound/Kitchen/Oven_Door_Full_Close.mp3"});
	queue.loadFile( {id: "res/sound/Kitchen/Oven_Door_Peek_Close.mp3", src:"res/sound/Kitchen/Oven_Door_Peek_Close.mp3"});
	queue.loadFile( {id: "res/sound/Kitchen/Oven_Door_Peek_Open.mp3", src:"res/sound/Kitchen/Oven_Door_Peek_Open.mp3"});
    queue.loadFile( {id: "res/sound/Kitchen/Close_Cookbook.mp3", src:"res/sound/Kitchen/Close_Cookbook.mp3"});
    queue.loadFile( {id: "res/sound/Kitchen/Open_Cookbook.mp3", src:"res/sound/Kitchen/Open_Cookbook.mp3"});
    queue.loadFile( {id: "res/sound/Kitchen/Explosion_Sound.mp3", src:"res/sound/Kitchen/Explosion_Sound.mp3"} );
    queue.loadFile( {id: "res/sound/Kitchen/Double_Beep.mp3", src:"res/sound/Kitchen/Double_Beep.mp3"} );

    // Market Items
    queue.loadFile( {id: "res/screens/MarketScreen/MarketTopShelf.png", src:"res/screens/MarketScreen/MarketTopShelf.png"});

	queue.loadFile( {id: "res/items/Clipboard.png", src:"res/items/Clipboard.png"});
    queue.loadFile( {id: "res/items/Wallet.png", src:"res/items/Wallet.png"});

    queue.loadFile( {id: "res/items/FrillsBox.png", src:"res/items/ExitSign.png"});
    queue.loadFile( {id: "res/items/FrillsBoxGlow.png", src:"res/items/ExitGlow.png"});

    queue.loadFile( {id: "res/items/FrillsBox.png", src:"res/items/FrillsBox.png"});
    queue.loadFile( {id: "res/items/FrillsBoxGlow.png", src:"res/items/FrillsBoxGlow.png"});

    queue.loadFile( {id: "res/items/TempProbe.png", src:"res/items/TempProbe.png"});
    queue.loadFile( {id: "res/items/TempProbeGlow.png", src:"res/items/TempProbeGlow.png"});

	queue.loadFile( {id: "res/items/OvenLightBox.png", src:"res/items/OvenLightBox.png"});
    queue.loadFile( {id: "res/items/OvenLightBoxGlow.png", src:"res/items/OvenLightBoxGlow.png"});

	queue.loadFile( {id: "res/items/Alarm.png", src:"res/items/Alarm.png"});
    queue.loadFile( {id: "res/items/AlarmGlow.png", src:"res/items/AlarmGlow.png"});

	queue.loadFile( {id: "res/items/Cookbook1.png", src:"res/items/Cookbook1.png"});
    queue.loadFile( {id: "res/items/Cookbook1Glow.png", src:"res/items/Cookbook1Glow.png"});

	queue.loadFile( {id: "res/items/StuffingRepurposed.png", src:"res/items/StuffingRepurposed.png"});
    queue.loadFile( {id: "res/items/StuffingRepurposedGlow.png", src:"res/items/StuffingRepurposedGlow.png"});

	queue.loadFile( {id: "res/items/StuffingExquisite.png", src:"res/items/StuffingExquisite.png"});
    queue.loadFile( {id: "res/items/StuffingExquisiteGlow.png", src:"res/items/StuffingExquisiteGlow.png"});

	queue.loadFile( {id: "res/items/StuffingSpecial.png", src:"res/items/StuffingSpecial.png"});
    queue.loadFile( {id: "res/items/StuffingSpecialGlow.png", src:"res/items/StuffingSpecialGlow.png"});

	queue.loadFile( {id: "res/items/Turkey5.png", src:"res/items/Turkey5.png"});
    queue.loadFile( {id: "res/items/Turkey5Glow.png", src:"res/items/Turkey5Glow.png"});

    queue.loadFile( {id: "res/items/Turkey4.png", src:"res/items/Turkey4.png"});
    queue.loadFile( {id: "res/items/Turkey4Glow.png", src:"res/items/Turkey4Glow.png"});

	queue.loadFile( {id: "res/items/Turkey3.png", src:"res/items/Turkey3.png"});
    queue.loadFile( {id: "res/items/Turkey3Glow.png", src:"res/items/Turkey3Glow.png"});

	queue.loadFile( {id: "res/items/Turkey2.png", src:"res/items/Turkey2.png"});
    queue.loadFile( {id: "res/items/Turkey2Glow.png", src:"res/items/Turkey2Glow.png"});

	queue.loadFile( {id: "res/items/Turkey1.png", src:"res/items/Turkey1.png"});
    queue.loadFile( {id: "res/items/Turkey1Glow.png", src:"res/items/Turkey1Glow.png"});

    // People photos
   	queue.loadFile( {id: "res/people/Boyfriend.png", src:"res/people/Boyfriend.png"});
   	queue.loadFile( {id: "res/people/Brother.png", src:"res/people/Brother.png"});
   	queue.loadFile( {id: "res/people/Cat.png", src:"res/people/Cat.png"});
   	queue.loadFile( {id: "res/people/Dad.png", src:"res/people/Dad.png"});
   	queue.loadFile( {id: "res/people/Girlfriend.png", src:"res/people/Girlfriend.png"});
   	queue.loadFile( {id: "res/people/Grandma.png", src:"res/people/Grandma.png"});
   	queue.loadFile( {id: "res/people/Grandpa.png", src:"res/people/Grandpa.png"});
   	queue.loadFile( {id: "res/people/Mom.png", src:"res/people/Mom.png"});
   	queue.loadFile( {id: "res/people/PlayerFemale.png", src:"res/people/PlayerFemale.png"});
   	queue.loadFile( {id: "res/people/PlayerMale.png", src:"res/people/PlayerMale.png"});
    queue.loadFile( {id: "res/people/TurkeyGhost.png", src:"res/people/TurkeyGhost.png"});

    // Load Window elements
    queue.loadFile( {id: "res/screens/Window/Door1.png", src:"res/screens/Window/Door1.png"});
    queue.loadFile( {id: "res/screens/Window/Door2.png", src:"res/screens/Window/Door2.png"});
    queue.loadFile( {id: "res/screens/Window/Ground.png", src:"res/screens/Window/Ground.png"});
    queue.loadFile( {id: "res/screens/Window/Housefar.png", src:"res/screens/Window/Housefar.png"});
    queue.loadFile( {id: "res/screens/Window/Small1.png", src:"res/screens/Window/Small1.png"});
    queue.loadFile( {id: "res/screens/Window/Small2.png", src:"res/screens/Window/Small2.png"});
    queue.loadFile( {id: "res/screens/Window/Small3.png", src:"res/screens/Window/Small3.png"});
    queue.loadFile( {id: "res/screens/Window/Small4.png", src:"res/screens/Window/Small4.png"});
    queue.loadFile( {id: "res/screens/Window/Small5.png", src:"res/screens/Window/Small5.png"});
    queue.loadFile( {id: "res/screens/Window/StreetlightGlow.png", src:"res/screens/Window/StreetlightGlow.png"});
    queue.loadFile( {id: "res/screens/Window/Win1.png", src:"res/screens/Window/Win1.png"});
    queue.loadFile( {id: "res/screens/Window/Win2.png", src:"res/screens/Window/Win2.png"});
    queue.loadFile( {id: "res/screens/Window/Win3.png", src:"res/screens/Window/Win3.png"});
    queue.loadFile( {id: "res/screens/Window/Win4.png", src:"res/screens/Window/Win4.png"});
    queue.loadFile( {id: "res/screens/Window/Win5.png", src:"res/screens/Window/Win5.png"});
    queue.loadFile( {id: "res/screens/Window/Win6.png", src:"res/screens/Window/Win6.png"});
    queue.loadFile( {id: "res/screens/Window/Win7.png", src:"res/screens/Window/Win7.png"});
    queue.loadFile( {id: "res/screens/Window/Win8.png", src:"res/screens/Window/Win8.png"});
    queue.loadFile( {id: "res/screens/Window/Win9.png", src:"res/screens/Window/Win9.png"});
    queue.loadFile( {id: "res/screens/Window/Win10.png", src:"res/screens/Window/Win10.png"});
    queue.loadFile( {id: "res/screens/Window/Win11.png", src:"res/screens/Window/Win11.png"});
    queue.loadFile( {id: "res/screens/Window/Tree_Animation.png", src:"res/screens/Window/Tree_Animation.png"});
    queue.loadFile( {id: "res/screens/Window/Test4TransparencyFull.png", src:"res/screens/Window/Test4TransparencyFull.png"});
    queue.loadFile( {id: "res/screens/Window/Stars.png", src:"res/screens/Window/Stars.png"});



    this.screenState = 0;
    this.newScreen = "";

	this.marketItems = {
		"Frills Box" : new MarketItem( this, "Frills Box", 133,92, 3.00, "res/items/FrillsBox.png", "res/items/FrillsBoxGlow.png", "res/screens/KitchenScreen/FrillsBoxKitchen.png", "res/screens/KitchenScreen/FrillsBoxKitchenGlow.png",
		 "Some people dress up their dogs. Others dress up their house. Why not dress up your turkey?" ),


	    "Temperature Probe" : new MarketItem( this, "Temperature Probe", 200, 57, 9.00, "res/items/TempProbe.png", "res/items/TempProbeGlow.png", "res/screens/KitchenScreen/TempProbeKitchen.png", "res/screens/KitchenScreen/TempProbeKitchenGlow.png", "Ensure your food is cooked with this handy thermometer. Now with easy to read LED display" ),
	    "Oven Light in a Box" : new MarketItem( this, "Oven Light in a Box", 131,222, 15.00, "res/items/OvenLightBox.png", "res/items/OvenLightBoxGlow.png", null,null, "This will allow checking on your turkey without letting the heat out." ),

	    "Alarm Clock" : new MarketItem( this, "Alarm Clock", 173,248, 6.00, "res/items/Alarm.png", "res/items/AlarmGlow.png", "res/screens/KitchenScreen/AlarmKitchen.png", "res/screens/KitchenScreen/AlarmKitchenGlow.png", "Have you ever wanted to control time? Now you can. Digital readout counts down until time of choice. Audible alarm" ),
		"Cookbook" : new MarketItem( this, "Cookbook", 283,203, 3.00, "res/items/Cookbook1.png", "res/items/Cookbook1Glow.png","res/screens/KitchenScreen/CookbookKitchen.png",  "res/screens/KitchenScreen/CookbookKitchenGlow.png", "How do I cook turkey? Handy note space included for writing down temperature measurements" ),
	    "Repurposed Stuffing" : new MarketItem( this, "Repurposed Stuffing",  510,197, 2.00, "res/items/StuffingRepurposed.png", "res/items/StuffingRepurposedGlow.png", "res/screens/KitchenScreen/StuffingRepurposedKitchen.png", "res/screens/KitchenScreen/StuffingRepurposedKitchenGlow.png","At least 80% original breadcrumb. Guaranteed to contain no avian products" ),
	    "Exquisite Stuffing" : new MarketItem( this, "Exquisite Stuffing", 458,210, 3.00, "res/items/StuffingExquisite.png", "res/items/StuffingExquisiteGlow.png", "res/screens/KitchenScreen/StuffingExquisiteKitchen.png","res/screens/KitchenScreen/StuffingExquisiteKitchenGlow.png", "Colonial merchants once traveled the four reaches of the Earth to bring back the ingredients contained in this very box" ),

	    "Special Stuffing" : new MarketItem( this, "Special Stuffing", 390,220, 6.00, "res/items/StuffingSpecial.png", "res/items/StuffingSpecialGlow.png",
	    	"res/screens/KitchenScreen/StuffingSpecialKitchen.png","res/screens/KitchenScreen/StuffingSpecialKitchenGlow.png",
	    	"Once rated as the most handsome man in the universe. Scott and his patented special stuffing will set you on the path to food heaven" ),

	    "Organic Turkey" : new MarketItem( this, "Organic Turkey", 180,360, randomWeight[0]*1.2, "res/items/Turkey5.png", "res/items/Turkey5Glow.png",null,null, "All natural. No hormones. No antibiotics. Free Range. Lead Free", parseFloat(randomWeight[0]) ),
	    "Free Range Turkey": new MarketItem( this, "Free Range Turkey", 540,320, randomWeight[1]*1.00, "res/items/Turkey4.png", "res/items/Turkey4Glow.png",null,null, "Our turkeys have wide open spaces to roam and are fed with only the highest quality feed.", parseFloat(randomWeight[1]) ),
	    "Sunny Farms Turkey" : new MarketItem( this, "Sunny Farms Turkey", 265,415, randomWeight[2]*0.60, "res/items/Turkey3.png", "res/items/Turkey3Glow.png",null,null, "100% Turkey product from Sunny Farms Heavy Industries, Ltd.", parseFloat(randomWeight[2]) ),
	    "Pastured Turkey": new MarketItem( this, "Pastured Turkey", 474,357, randomWeight[3]*1.4, "res/items/Turkey2.png", "res/items/Turkey2Glow.png",null,null, "Grassy fields and natural ingredients allow our turkeys to have a better life, and taste great.", parseFloat(randomWeight[3]) ),
		"General Turkey": new MarketItem( this, "General Turkey", 378,426, randomWeight[4]*0.80, "res/items/Turkey1.png", "res/items/Turkey1Glow.png",null,null, "100% General Satisfaction Guaranteed", parseFloat(randomWeight[4]) )
	};

        // Important Model, dummy placeholder
    this.ovenModel = { secondTick:function(){}, setRawTemp:function(){}, getRawTemp:function(){}, getCookTime:function(){return 1000;} };


    /* all turkeys */
    this.turkeyStates = [
        new createjs.Bitmap( "res/screens/KitchenScreen/TurkeyState1Small.png" ),
        new createjs.Bitmap( "res/screens/KitchenScreen/TurkeyState2Small.png" ),
        new createjs.Bitmap( "res/screens/KitchenScreen/TurkeyState3Small.png" ),
        new createjs.Bitmap( "res/screens/KitchenScreen/TurkeyState4Small.png" ),
        new createjs.Bitmap( "res/screens/KitchenScreen/TurkeyState5Small.png" )
    ];

	this.purchasedItems = [];

	// did we already show the player the kitchen intro?
	this.kitchenIntro = false;

    this.addRecord = function( record ){
        that.peekRecords.push( new Record( record.type, that.currentTime, record.text ) );
    }
    that.pubsub.subscribe( "AddRecord", this.addRecord );


    function addHighScore(name, turkeyPoundage, cookTime, score){
    	var scores = {};
    	var now = new Date();
    	if( !localStorage.getItem("highScores") ){
    		scores = JSON.parse( localStorage.getItem("highScores") );
    	}

    	scores[now.getYear()+"/"+now.getMonth()+"/"+now.getDay()] = {
    			"name" : name,
    			"weight" : turkeyPoundage,
    			"cookTime" : cookTime,
    			"score" : score
    	};

    	localStorage.setItem("highScores", JSON.stringfy(scores));
    }

	function gameLoop(){
		that.mainUI.draw();
	}

	return {
	//	"main": this
	}
}

function GameUI( canvasElem, gameState ){
	var that = this;

	var SCREEN_OUT = 1;
	var SCREEN_IN  = 2;
	var SCREEN_STABLE = 0;

	this.stage = new createjs.Stage( canvasElem );
	this.stage.enableMouseOver(25);

	this.activeScreenName = "EndingScreen";
	this.activeScreenObj = {};

	/* Initialize All Screens */
	this.screens = {
		"LoadingScreen" 	 : LoadingScreen,
		"MainScreen" 	 	 : MainScreen,
		"DifficultyScreen" 	 : DifficultyScreen,
		"KitchenScreen"		 : KitchenScreen,
		"MarketScreen"		 : MarketScreen,
		"ScoreScreen"		 : ScoreScreen,
		"CreditsScreen"		 : CreditsScreen
	}

	var soundManager = new SoundManager( gameState );

	this.activeScreenObj = new LoadingScreen( this.stage, gameState );
	var textContent = new createjs.Text( "", "20px Arial", "white" );
	textContent.x = 750;
	textContent.y = 30;
	//this.stage.addChild( textContent);
	var overlay = new createjs.Shape();
 	overlay.graphics.beginFill("#fffffff").drawRect(0, 0, 800, 600 );
 	overlay.alpha = 0;
	this.stage.addChild(overlay);

	var dialogManager = new DialogUI( this.stage, gameState );

	// delay for fade in and fade-out
	this.switchScreen = function( screenName ){
		gameState.screenState = SCREEN_OUT;
		dialogManager.minDialog();
		if(DEBUG) console.log("Switch screen called with" + screenName);
		gameState.newScreen = screenName;
	};
	this.actuallySwitchScreen = function( screenName ){
		that.stage.removeAllChildren();
		that.activeScreenObj = new that.screens[ screenName ]( that.stage, gameState );
		//that.stage.addChild( textContent );
		that.stage.addChild( overlay );
		dialogManager.render();
	};
    new HelpUI(this.stage, gameState);

	gameState.pubsub.subscribe( "SwitchScreen", this.switchScreen );
	gameState.pubsub.subscribe( "ActuallySwitchScreen", this.actuallySwitchScreen );

	// Allow items to be removed if they don't have access to stage
	gameState.pubsub.subscribe( "RemoveItems", function(items){
		for (var index in items ){
			that.stage.removeChild(items[index]);
		}
	});

	return {
		draw : function(){
			if( gameState.screenState == SCREEN_OUT ){
				overlay.alpha +=0.3;
			}
			if( gameState.screenState == SCREEN_IN ){
				overlay.alpha -=0.3;
			}
			if( overlay.alpha > 1.0 ){
				gameState.screenState = SCREEN_IN;
				overlay.alpha = 1;
				gameState.pubsub.publish( "ActuallySwitchScreen", gameState.newScreen );
			}
			if( overlay.alpha  < 0.0 ){
				gameState.screenState = SCREEN_STABLE;
				overlay.alpha = 0;
			}
			soundManager.tick();
			that.activeScreenObj.blit();
			dialogManager.tick();
			textContent.text = createjs.Ticker.getMeasuredFPS().toFixed(1);
			that.stage.update();
		}
	}
}

function Record( type, dateTime, record ){
    return {
        getTime: function(){
            return dateTime;
        },
        getContent: function(){
            return record;
        },
        getType: function(){
            return type;
        }
    }
}

    //"Turkey weight, "
    //"Opened oven for X seconds"
    //"Core temperature measured at "


function BindPubSub( obj ){
	(function(q) {
	    var topics = {}, subUid = -1;
	    q.subscribe = function(topic, func) {
	        if (!topics[topic]) {
	            topics[topic] = [];
	        }
	        var token = (++subUid).toString();
	        topics[topic].push({
	            token: token,
	            func: func
	        });
	        return token;
	    };

	    q.publish = function(topic, args) {
	        if (!topics[topic]) {
	            return false;
	        }
	        setTimeout(function() {
	            var subscribers = topics[topic],
	                len = subscribers ? subscribers.length : 0;

	            while (len--) {
	                subscribers[len].func(args);
	            }
	        }, 0);
	        return true;

	    };

	    q.unsubscribe = function(token) {
	        for (var m in topics) {
	            if (topics[m]) {
	                for (var i = 0, j = topics[m].length; i < j; i++) {
	                    if (topics[m][i].token === token) {
	                        topics[m].splice(i, 1);
	                        return token;
	                    }
	                }
	            }
	        }
	        return false;
	    };
	}(obj));
}
