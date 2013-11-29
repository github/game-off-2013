define('terrain', ['d3', 'arrayUtils'], function(d3, arrayUtils) {
    'use strict';

    var generate = function(cells, proportionLand) {
        cells.forEach(function(cell) {
            cell.attributes = [];
            if (Math.abs(d3.geo.centroid(cell)[1]) > 66.5) {
                cell.attributes.push('polar');
            }
        });

        var landCells = [];

        while (landCells.length < (proportionLand * cells.length / 10)) {
            arrayUtils.addIfNotPresent(landCells, arrayUtils.getRandomElement(cells));
        }

        while (landCells.length < (proportionLand * cells.length)) {
            arrayUtils.addIfNotPresent(landCells,
                arrayUtils.getRandomElement(
                    arrayUtils.getRandomElement(landCells).neighbours));
        }

        cells.forEach(function(cell) {
            cell.attributes.unshift('sea');
        });
        landCells.forEach(function(cell) {
            cell.attributes[0] = 'land';
        });
    };

    return {
        generate: generate
    };
});
