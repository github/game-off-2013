define('facilityList', ['underscore'], function(_) {
    'use strict';

    return function(availableFacilities) {
        this.facilities = [];

        this.addFacility = function(facilityName) {
            this.facilities.push(availableFacilities[facilityName]);
        };

        this.removeFacility = function(facility) {
            var facilityIndex = this.facilities.indexOf(facility);
            this.facilities.splice(facilityIndex, 1);
        };

        this.annualFoodDifference = function() {
            return _.reduce(this.facilities, function(sum, next) { return sum + next.normalDelta.food; }, 0);
        };

        this.annualEnergyDifference = function () {
            return _.reduce(this.facilities, function(sum, next) { return sum + next.normalDelta.energy; }, 0);
        };

        this.annualPollutionDifference = function() {
            return _.reduce(this.facilities, function(sum, next) { return sum + next.normalDelta.pollution; }, 0);
        };
    };
});
