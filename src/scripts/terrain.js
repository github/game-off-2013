define('terrain', ['d3', 'arrayUtils'], function(d3, arrayUtils) {
    'use strict';

    var MAX_LAND_ALTITUDE = 1000;

    var isLand = function isLand(cell) {
        return cell.attributes.indexOf('land') > -1;
    };

    var generate = function generateTerrain(cells, proportionLand) {
        cells.forEach(function(cell) {
            cell.attributes = [];
            if (Math.abs(d3.geo.centroid(cell)[1]) > 66.5) {
                cell.attributes.push('polar');
            }
        });

        var landCells = [];
        var totalLand = proportionLand * cells.length;
        var currentAltitude = MAX_LAND_ALTITUDE;
        var step = MAX_LAND_ALTITUDE / totalLand;

        var tryAddLand = function(candidate) {
            if (arrayUtils.addIfNotPresent(landCells, candidate)) {
                candidate.altitude = Math.ceil(currentAltitude);
                currentAltitude -= step;
            }
        };

        while (landCells.length < (totalLand / 10)) {
            tryAddLand(arrayUtils.getRandomElement(cells));
        }

        while (landCells.length < (totalLand)) {
            tryAddLand(
                arrayUtils.getRandomElement(
                    arrayUtils.getRandomElement(landCells).neighbours));
        }

        cells.forEach(function(cell) {
            cell.attributes.unshift('sea');
        });
        landCells.forEach(function(cell) {
            cell.attributes[0] = 'land';
        });

        var calculateRemainingLandArea = function() {
            var landArea = 0;
            cells.forEach(function(cell) {
                if (isLand(cell)) {
                    ++landArea;
                }
            });
            return landArea;
        };

        var updateSeaLevel = function(seaLevel) {
            cells.forEach(function(cell) {
                cell.attributes[0] = cell.altitude > seaLevel ? 'land' : 'sea';
            });
        };

        return {
            calculateRemainingLandArea: calculateRemainingLandArea,
            updateSeaLevel: updateSeaLevel
        };
    };

    return {
        generate: generate,
        isLand: isLand
    };
});
