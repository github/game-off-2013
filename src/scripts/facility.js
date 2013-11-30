define('facility', function() {
    'use strict';

    return function(name, energyCost, landCost, annualFoodDifference, annualEnergyDifference, annualPollutionDifference) {
        this.name = name;
        this.energyCost = energyCost;
        this.landCost = landCost;
        this.annualFoodDifference = annualFoodDifference;
        this.annualEnergyDifference = annualEnergyDifference;
        this.annualPollutionDifference = annualPollutionDifference;
    };
});
