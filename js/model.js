function TurkeyLayer( name, layerNumber1, turkeyModel, ovenModel ){
	var that = this;

	this.name = name;
	this.layerNumber=layerNumber1;
	this.waterLost = 0;
	this.finalTemperature = 20;
	this.cookCondition = "Raw";
	this.highestTemp = 0;
    return {
    	updateTemperatureTick: function(){
    		that.finalTemperature = turkeyModel.globTemp[that.layerNumber]
			if(DEBUG) console.log(turkeyModel.globTemp);
			that.highestTemp = that.finalTemperature > that.highestTemp ? that.finalTemperature : that.highestTemp;
			that.waterLost = that.waterLost + UtilityFunctions.waterLoss( that.finalTemperature );
			that.cookCondition = UtilityFunctions.cookCondition(that.waterLost, that.name);
			if(DEBUG) console.log( that.name + ": "+ that.waterLost + " " + that.cookCondition + " " + that.finalTemperature + " C" );
    	},
		getCondition: function(){
			return that.cookCondition;
		},
		getTemperature: function(){
			return that.finalTemperature;
		},
		getHighestTemp: function(){
			return that.highestTemp;
		}

    }
}


function TurkeyModel( weight, ovenModel ){
	this.density = 700; 	 	 // kg/m3 Assuming Density of Water 1000 kg/m3
	this.cp = 2810;			 	 // 2810 J/kg K for Turkey. Extra is to semi-account for water evaporation energy
	this.heatConvection = 9; 	 // W/m2 K Some Reasonable estimate for natural Convection. Change as needed. 5-25
	this.thermalConduct = 0.412; // W/m K // Chicken
	this.skin = {};
	this.body = {};
	this.core = {};
	this.splitsNum = 20;
		console.log(UtilityFunctions.lbs2kgs(weight))
	this.totalRadius = UtilityFunctions.calculateRadius( UtilityFunctions.lbs2kgs(weight), this.density );

	
	this.totalLayers = [ new TurkeyLayer("Skin", this.splitsNum-1, this, ovenModel ),
						 new TurkeyLayer("Body", this.splitsNum-10, this, ovenModel ),
						 new TurkeyLayer("Core", 1, this, ovenModel ) ];

	// Whenever temperature is changed
	this.updateLayerTemps = function() {
		this.globTemp = UtilityFunctions.transientSphereSeries( this.density,
    															this.thermalConduct,
    															this.heatConvection,
    															this.cp,
    															this.totalRadius,
																ovenModel.tempInfini,
																this.splitsNum,
																this.deltar,
																this.globTemp,
																this.pointRadius
																);
			for (var i in this.totalLayers ){
		        this.totalLayers[i].updateTemperatureTick();
	    }
	};
	
	this.resetLayerTemps = function() {
		for (var i in this.totalLayers ) {
		    this.totalLayers[i].resetLayerTemps();
	    }
	};
	
	//Sheen Model Stuff
	this.globTemp=[];
	this.pointRadius = []
	this.splitsNum = 20;
	this.deltar = this.totalRadius/this.splitsNum; //20 Data Points
	
	this.initializePoints = function() {
		var step = ( this.totalRadius - this.deltar ) / ( this.splitsNum - 1 );
		for (var i = 0; i<this.splitsNum ; i++ ) {
			this.pointRadius.push(step*i+this.deltar);
			this.globTemp.push(20+step*i); //Starts at 20 C for initilizating
		}	
	};
	this.initializePoints()
	
	
}

function OvenModel( turkeyWeight, gameState ) {
	var that = this;
	this.tempInfini=20; //C
	this.setTemp = 20;
	this.globalTime = 0;
	
	var turkey = new TurkeyModel(turkeyWeight, this );
	var proportional = 0.004; // This value is arbitrary to how fast you want the temperatures to converge. (Or oscillate, which could be realistic as well)
	var errorTolerance = 10; //Stove is accurate to 1 degree Celcius Should hopefully oscillate below that value.
   	// Equalize temp will need to be sent each time iteration
   	this.equalizeTemp= function(){
            var error = Math.abs(this.setTemp-this.tempInfini);
            if( this.setTemp>this.tempInfini ){
                    this.tempInfini = this.tempInfini + error*proportional;
            }
            else if( this.setTemp<this.tempInfini ){
                    this.tempInfini = this.tempInfini - error*proportional;
            }

            if( error>errorTolerance ) {
				return(true);
            }
    	}
    return {
    	getTurkeyState: function(){
    		return {
    			"skin" : {
    				"temp": turkey.totalLayers[0].getTemperature(),
    				"cond": turkey.totalLayers[0].getCondition(),
    				"highest" : turkey.totalLayers[0].getHighestTemp()
    			},
    			"body" : {
    				"temp": turkey.totalLayers[1].getTemperature(),
    				"cond": turkey.totalLayers[1].getCondition(),
    				"highest" : turkey.totalLayers[1].getHighestTemp()
    			},
    			"core" : {
    				"temp": turkey.totalLayers[2].getTemperature(),
    				"cond": turkey.totalLayers[2].getCondition(),
    				"highest" : turkey.totalLayers[2].getHighestTemp()
    			}
    		};
    	},
    	changeTemp: function(setTemp){
    		if(DEBUG) console.log("temp changed to " + setTemp);
            that.setTemp = setTemp;
    	},
    	// set the tempInfini
    	setRawTemp: function(newTemp){
    		if(DEBUG) console.log("raw temp changed to" + that.tempInfini);
    		that.tempInfini = newTemp;
    	},
    	getRawTemp: function(){
    		return that.tempInfini;
    	},
    	getCookTime: function(){
    		return that.globalTime;
    	},
	    secondTick: function(){
			that.globalTime = that.globalTime + 1;
	    	if ( that.equalizeTemp() ) {

	    		// Turn on oven light
				gameState.pubsub.publish( "OvenLight", "On" );
			}
			else {
				// Turn off oven light
				gameState.pubsub.publish( "OvenLight", "Off" );
			}
				if(DEBUG) console.log("Oven Temp " + that.tempInfini )
				turkey.updateLayerTemps();
	    }
	}
}


UtilityFunctions = {

	// Cache the lambda if the Biot number does not change, to avoid expensive root-finding operations
	cachedBiot: null,
	cachedLambda: null,

	// Using Ratios for a rectangular Box Turkey
	calculateRadius: function(weight, density) {

		var ratioLvG=1.4; //1.4, Turkey length vs shoulder girth
		var ratioLvH=2; //2, Turkey length vs height from resting position

		var length = Math.pow(weight/((1/ratioLvG)*(1/ratioLvH)*density),(1/3))
		var depth = 1/(ratioLvG /length);
		var height = 1/(ratioLvH /length);
		var simpleRadius = length/2; //Doesn't take into account equal Volume

		var rectangleVolume = depth*height*length; //m^3  Multiple by 1/4 to account for triangular shape and empty Space
		var complexRadius = Math.pow(rectangleVolume/((4/3)*Math.PI), 1/3); //Volume of 3D Box = 3D Sphere

		//if(DEBUG) console.log("Simple Radius  " + simpleRadius + " Meters")
		//if(DEBUG) console.log("Complex Radius  " + complexRadius + " Meters")
		return complexRadius;
	},

	sphereVolume: function(radius) {
		return((4/3)*Math.PI*Math.pow(radius,3))
	},

	waterLoss: function(temperature) {
		return (Math.pow(10,(temperature-20)/80)-1)
	},

	transientSphereSeries: function( density, thermalConduct, heatConvection, cp, rTotal, tempInfinity, splitsNum, deltar, globTemp,pointRadius) {

//Not Global Stuff
		var r0 = rTotal;
		var deltat = 0.1
			
		var alpha = thermalConduct/(density*cp)
		var h=heatConvection; 

		for (var j=0; j<(1/deltat); j++ ) {
			var dTdr=[]
		//	globTemp[splitsNum-1] should be last entry in globtemp
				for (var k=0; k<splitsNum; k++){
				if (k==0) {
					dTdr.push((globTemp[1] - globTemp[0])/deltar) }
				else if (k==splitsNum-1) {
					dTdr.push((globTemp[splitsNum-1] - globTemp[splitsNum-2])/deltar)}
				else {
					dTdr.push((globTemp[k+1] - globTemp[k-1])/(2*deltar))}
				}
				dTdr[splitsNum-1] = heatConvection*(tempInfinity-globTemp[splitsNum-1])/thermalConduct
				
				var parenthesis = []
				for (var k=0; k<splitsNum; k++){
					parenthesis.push(dTdr[k]*Math.pow(pointRadius[k],2))
				}
				
				dPdr = []
				for (var k=0; k<splitsNum; k++){
				if (k==0) {
					dPdr.push((parenthesis[1] - parenthesis[0])/deltar) }
				else if (k==splitsNum-1) {
					dPdr.push((parenthesis[splitsNum-1] - parenthesis[splitsNum-2])/deltar)}
				else {
					dPdr.push((parenthesis[k+1] - parenthesis[k-1])/(2*deltar))}
				}
				
				for (var k=0; k<splitsNum; k++){
					globTemp[k]=alpha*dPdr[k]/Math.pow(pointRadius[k],2)*deltat + globTemp[k] //dTdr * deltaT in one loop
				}
			//dTdt(1)=dTdt(1)/2; 
				}

		return(globTemp)
	},

	/* Utility Functions */
	C2F: function( celsius ){
		return ( (celsius*(9/5)) + 32 );
	},
	F2C: function( farenheit ) {
		return ( (farenheit-32) *(5/9) );
	},
	lbs2kgs: function(pounds){
		return pounds * 0.453592
	},
	randRange: function(min, max){
		return Math.floor(Math.random()*(max-min+1))+min;
	},
	cookCondition: function(cookValue, layerName){

		if( layerName == "Skin" || layerName == "Body" ){
			var multiplier = 1;
			if (cookValue>=multiplier*600000) {
				return ["Fire", (cookValue)/(multiplier*700000),"fire"];
			}
			else if(cookValue>=multiplier*400000) {
				return ["Burnt", (cookValue)/(multiplier*600000), "burnt"];
			}
			else if (cookValue>=multiplier*300000) {
				return ["Dry", (cookValue)/(multiplier*350000), "dry"];
			}
			else if (cookValue>=multiplier*250000){ // >250000
				return ["Cooked", (cookValue)/(multiplier*300000), "overcooked"];
			}
			else if (cookValue>=multiplier*80000) { // >50000
				return ["Cooked", (cookValue)/(multiplier*250000), "cooked"];
			}
			else if (cookValue>=multiplier*50000){
				return ["Undercooked", (cookValue)/(multiplier*80000), "slightly cooked"];
			}
			else if (cookValue>=multiplier*25000) { //
				return ["Undercooked", (cookValue)/(multiplier*50000), "undercooked"];
			}
			else {
				return ["Raw", 1, "raw"];
			}
		}
		else{
			var multiplier = 1;
			if (cookValue>=multiplier*45000) { //
				return ["Fire", (cookValue)/(multiplier*600000),"fire"];
			}
			else if(cookValue>=multiplier*35000){//
				return ["Burnt", (cookValue)/(multiplier*45000), "burnt"];
			}
			else if (cookValue>=multiplier*25000){ // 
				return ["Dry", (cookValue)/(multiplier*35000), "dry"];
			}
			else if (cookValue>=multiplier*22000){ //
				return ["Cooked", (cookValue)/(multiplier*25000), "overcooked"];
			}
			else if (cookValue>=multiplier*12000){ //
				return ["Cooked", (cookValue)/(multiplier*22000), "cooked"];
			}
			else if (cookValue>=multiplier*7000){ //
				return ["Undercooked", (cookValue)/(multiplier*12000), "slightly cooked"];
			}
			else if (cookValue>=multiplier*3000) {
				return ["Undercooked", (cookValue)/(multiplier*7000), "undercooked"];
			}
			else {
				return ["Raw", 1, "raw"];
			}

		}
	}
}

//Running the Program Stuff
/*
var ovenObject = new OvenModel();
var turkey = new TurkeyModel(9, ovenObject);

globalTime=0;
setInterval(function(){ovenObject.secondTick();},1000);
*/
