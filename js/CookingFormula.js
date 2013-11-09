//Global Variables for Turkey
density = 996; // kg/m3 Assuming Density of Water 1000 kg/m3
cp = 2810 // J/kg K for Turkey

function celsiusToFarenheit(celsius) {
farenheit = (celsius*(9/5)) + 32;
return(farenheit)
}

function poundsToKilograms(pounds) {
kilograms = (pounds * 0.453592);
return(kilograms)
}

function findClosest(value,array) {
closestDiff = null;
closestPosition = null;
	for (var i=0;i<array.length;i++) {
		diff = Math.abs(value-array[i])
		if (diff<closestDiff || closestDiff == null) {
			closestPosition=i;
			closestDiff = diff;
		} 
	}
	return ([closestPosition,array[closestPosition]])
}

function biotSphereCoefficients (Biot) {
Bi = [0.01, 0.02, 0.04, 0.06, 0.08, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1.0, 2, 3, 4, 5, 6, 7, 8, 9, 10, 20, 30, 40, 50, 100, 10000]
lambdaOne = [0.1730, 0.2445, 0.3450, 0.4217, 0.4860, 0.5423, 0.7593, 0.9208, 1.0528, 1.1656, 1.2644, 1.3525, 1.4320, 1.5044, 1.5708, 2.0288, 2.2889, 2.4556, 2.5704, 2.6537, 2.7165, 2.7654, 2.8044, 2.8363, 2.9857, 3.3072, 3.0632, 3.0788, 3.1102, 3.1416]
alphaOne = [ 1.0030, 1.0060, 1.0120, 1.0179, 1.0239, 1.0298, 1.0592, 1.0880, 1.1164, 1.1441, 1.1713, 1.1978, 1.2236, 1.2488, 1.2732, 1.4793, 1.6227, 1.7202, 1.7870, 1.8338, 1.8673, 1.8920, 1.9106, 1.9249, 1.9781, 1.9898, 1.9942, 1.9962, 1.9990, 2 ]
position = findClosest(Biot,Bi)[0]
return([lambdaOne[position], alphaOne[position]])
}


function calculateRadius(weight) {
//Using Ratios for a rectangular Box Turkey
ratioLvG=1.4; //1.4, Turkey length vs shoulder girth
ratioLvH=2; //2, Turkey length vs height from resting position

length = Math.pow(weight/((1/ratioLvG)*(1/ratioLvH)*density),(1/3))
depth = 1/(ratioLvG /length);
height = 1/(ratioLvH /length);
simpleRadius = length/2; //Doesn't take into account equal Volume

rectangleVolume = depth*height*length*(1/3); //m^3  Multiple by 1/3 to account for triangular shape and empty Space
complexRadius = Math.pow(rectangleVolume/((4/3)*Math.PI), 1/3); //Volume of 3D Box = 3D Sphere

console.log("Simple Radius  " + simpleRadius + " Meters")
console.log("Complex Radius  " + complexRadius + " Meters")
}

function LumpedCapacitanceMethod (radius, tempInitial,tempInfini, t) {
name : "Skin"
volume = (4/3)*Math.PI*Math.pow(radius,3); //3D Sphere
surfaceArea = 4*Math.PI*Math.pow(radius,2); //3D Sphere
heatConvection = 12; // W/m2 K Some Reasonable estimate for natural Convection. Change as needed. 5-25
thermalConduct = 0.412 // W/m K
cp = 2810 // J/kg K

mass = density * volume;

charLength = volume/surfaceArea ;

biotNum = heatConvection * charLength/thermalConduct

console.log("The Biot Value is " + biotNum)

b=(heatConvection)/(density*charLength*cp)
console.log("The time constant b is "+ b)

tempAtTime = Math.exp(-b*t)*(tempInitial-tempInfini)+tempInfini;
console.log("The Temperature at time " + t +" seconds is " + tempAtTime)

Qdot = -1*heatConvection*surfaceArea*(tempAtTime-tempInfini) //Heat Transfer Rate Useful for water Loss
console.log("The Heat Flux is " + Qdot )
}

function transientSphere (rPosition,rTotal,tempInitial,tempInfini,t) {
heatConvection = 12; // W/m2 K Some Reasonable estimate for natural Convection. Change as needed. 5-25
thermalConduct = 0.412 // W/m K
alpha = thermalConduct/(density*cp)
console.log("Alpha is " + alpha)

Fourier = (alpha*t)/Math.pow(rTotal,2)
console.log("Fourier is " +  Fourier)
biotNum = heatConvection * rTotal/thermalConduct
console.log("The Biot Value is " + biotNum)
temp=biotSphereCoefficients(biotNum)
lambdaOne=temp[0];
alphaOne=temp[1];
console.log("lambda1 is " + lambdaOne)
console.log("A1 is " + alphaOne)

//This is only valid for Fourier greater than 0.2
sinPortion= Math.sin(lambdaOne*rPosition/rTotal)/(lambdaOne*rPosition/rTotal);
expotentialPortion = alphaOne*(1/Math.exp(Math.pow(lambdaOne,2)*Fourier))
tempAtTimeAndRadius=(sinPortion*expotentialPortion*(tempInitial-tempInfini))+tempInfini
console.log("The Temperature at radius " + rPosition + " m and time " + t + "  seconds is " + tempAtTimeAndRadius + " C or " + celsiusToFarenheit(tempAtTimeAndRadius) + " F");
}


function transientSphereEgg (rPosition,tempInitial,tempInfini,t) {
rTotal = 0.025
heatConvection = 1200; // W/m2 K Some Reasonable estimate for natural Convection. Change as needed. 5-25
thermalConduct = 0.627 // W/m K
alpha = 0.000000151
console.log("Alpha is " + alpha)

Fourier = alpha * t/Math.pow(rTotal,2)
console.log("Fourier is " +  Fourier)
biotNum = heatConvection * rTotal/thermalConduct

console.log("The Biot Value is " + biotNum)
temp=biotSphereCoefficients(biotNum)
lambdaOne=temp[0];
alphaOne=temp[1];
console.log("lambda1 is " + lambdaOne)
console.log("A1 is " + alphaOne)

//This is only valid for Fourier greater than 0.2
sinPortion= Math.sin(lambdaOne*rPosition/rTotal)/(lambdaOne*rPosition/rTotal);
expotentialPortion = alphaOne*(1/Math.exp(Math.pow(lambdaOne,2)*Fourier))
tempAtTimeAndRadius=(sinPortion*expotentialPortion*(tempInitial-tempInfini))+tempInfini
console.log("The Temperature At radius " + rPosition +" m and time " + t + "  seconds is " + tempAtTimeAndRadius + " C or " + celsiusToFarenheit(tempAtTimeAndRadius) + " F" );
}