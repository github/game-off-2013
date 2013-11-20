define('facilityList', ['underscore'], function(_) {
    'use strict';

    return function() {
        this.facilities = [];

        this.addFacility = function(facility) {
            this.facilities.push(facility);
        };

        this.removeFacility = function(facility) {
            var facilityIndex = this.facilities.indexOf(facility);
            this.facilities.splice(facilityIndex, 1);
        }

        this.annualFoodDifference = function() {
            return _.reduce(this.facilities, function(sum, next) { return sum + next.annualFoodDifference; }, 0);
        };

        this.annualEnergyDifference = function () {
            return _.reduce(this.facilities, function(sum, next) { return sum + next.annualEnergyDifference; }, 0);
        };

        this.annualPollutionDifference = function() {
            return _.reduce(this.facilities, function(sum, next) { return sum + next.annualPollutionDifference; }, 0);
        };
    };
});
