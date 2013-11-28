define('globe', ['jquery', 'd3','grid'], function($, d3, grid) {
    'use strict';

    var width = 950;
    var height = 500;

    return {
        render: function() {
            var width = 960,
                height = 500;

            var origin = [-80, 20];

            var projection = d3.geo.orthographic()
                .rotate(origin)
                .scale(240)
                .clipAngle(90);

            var path = d3.geo.path()
                .projection(projection);

            var svg = d3.select("body").append("svg")
                .attr("width", width)
                .attr("height", height);

            var n = 13;

            var cells = grid.generate(n);

            var previous = false;
            var polygons = svg.selectAll("path")
                .data(cells)
                .enter().append("path")
                .attr('class', function() {
                    if (previous) {
                        return (Math.random() > 0.25) ? 'land' : 'sea'
                    } else {
                        return (Math.random() > 0.5) ? 'land' : 'sea'
                    }
                });

            cells.forEach(function(cell, index) {
                cell.polygon = polygons[0][index];
            });

            $('path').each(function(i, elem) {
                $(elem).click(function() {
                    $(elem).attr('class', 'developed land');
                    d3.select(elem).datum().neighbours.forEach(function(neighbour) {
                        $(neighbour.polygon).attr('class', 'developed land');
                    });
                });
            });

            polygons.attr("d", path);

            $(document).keydown(function(e) {
                switch (e.keyCode) {
                    case 37:
                        origin[0] -=5;
                        projection.rotate(origin);
                        polygons.attr("d", path);
                        break;
                    case 38:
                        origin[1] +=5;
                        projection.rotate(origin);
                        polygons.attr("d", path);
                        break;
                    case 39:
                        origin[0] +=5;
                        projection.rotate(origin);
                        polygons.attr("d", path);
                        break;
                    case 40:
                        origin[1] -=5;
                        projection.rotate(origin);
                        polygons.attr("d", path);
                        break;
                    default:
                        break;
                }
            });
        }
    };
});