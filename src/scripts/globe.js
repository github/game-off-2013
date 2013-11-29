define('globe', ['jquery', 'd3', 'grid', 'terrain'], function ($, d3, grid, terrain) {
    'use strict';

    return {
        render: function () {
            var width = 960,
                height = 500;

            var origin = [0, -5];

            var projection = d3.geo.orthographic()
                .rotate(origin)
                .scale(240)
                .clipAngle(90);

            var path = d3.geo.path()
                .projection(projection);

            var svg = d3.select('body').append('svg')
                .attr('width', width)
                .attr('height', height);

            var n = 13;

            var cells = grid.generate(n);
            terrain.generate(cells, 0.5);

            var polygons = svg.selectAll('path')
                .data(cells)
                .enter().append('path')
                .attr('class', function (cell) {
                    return cell.attributes.join(' ');
                });

            cells.forEach(function (cell, index) {
                cell.polygon = polygons[0][index];
            });

            $('path').each(function (i, elem) {
                $(elem).click(function () {
                    if ($(elem).attr('class').substr(0,4) === 'land') {
                        $(elem).attr('class', 'developed land');
                    }
                    d3.select(elem).datum().neighbours.forEach(function (neighbour) {
                        if ($(neighbour.polygon).attr('class').substr(0,4) === 'land') {
                            $(neighbour.polygon).attr('class', 'developed land');
                        }
                    });
                    polygons.attr('d', path);
                });
            });

            polygons.attr('d', path);

            $(document).keydown(function (e) {
                switch (e.keyCode) {
                case 37:
                    origin[0] -= 5;
                    projection.rotate(origin);
                    polygons.attr('d', path);
                    break;
                case 38:
                    origin[1] += 5;
                    projection.rotate(origin);
                    polygons.attr('d', path);
                    break;
                case 39:
                    origin[0] += 5;
                    projection.rotate(origin);
                    polygons.attr('d', path);
                    break;
                case 40:
                    origin[1] -= 5;
                    projection.rotate(origin);
                    polygons.attr('d', path);
                    break;
                default:
                    break;
                }
            });
        }
    };
});