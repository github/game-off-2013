define('terrain', ['d3'], function(d3) {

    var generate = function(cells, proportionLand) {
        cells.forEach(function(cell, index) {
            cell.attributes = [];
            if (Math.abs(d3.geo.centroid(cell)[1]) > 66.5) {
                cell.attributes.push('polar');
            }
            if (index < cells.length * proportionLand) {
                cell.attributes.push('land');
            }
        });
    };

    return {
        generate: generate
    };
});
