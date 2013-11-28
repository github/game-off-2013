define('facilityList', ['underscore'], function(_) {
    'use strict';

    return function(availableFacilities) {
        this.facilities = [];
        var currentEnergy = 0;

        this.addFacility = function(facilityName, currentTime) {
            this.facilities.push(availableFacilities[facilityName]);
        };

        this.removeFacility = function(facility) {
            var facilityIndex = this.facilities.indexOf(facility);
            this.facilities.splice(facilityIndex, 1);
        };

        this.update = function(currentTime, unfloodedLandArea) {
            var foodDelta =  _.reduce(this.facilities, function(sum, next) { return sum + next.normalDelta.food; }, 0);
            var pollutionDelta = _.reduce(this.facilities, function(sum, next) { return sum + next.normalDelta.pollution; }, 0);
            var energyDelta = _.reduce(this.facilities, function(sum, next) { return sum + next.normalDelta.energy; }, 0);
            currentEnergy += energyDelta;

            var consumedLandArea = _.reduce(this.facilities, function(sum, next) { return sum + next.landCost; }, 0)
            return {
                buildableLandArea: unfloodedLandArea - consumedLandArea,
                pollutionDelta: pollutionDelta,
                foodDelta: foodDelta
            };
        };
    };
});
