define('globe', ['dual'], function(dual) {
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

            var n = 2;

            var faces = d3.geodesic.polygons(n);

            var polygon = svg.selectAll("path")
                .data(dual.generateDual(faces))
                .enter().append("path");

            $('path').each(function(i, elem) {
                $(elem).click(function() {
                    console.log(i % (n*n));
                    $(elem).css('fill', 'black');
                })
            });

            polygon.attr("d", path);

            $(document).keydown(function(e) {
                switch (e.keyCode) {
                    case 37:
                        origin[0] -=5;
                        projection.rotate(origin);
                        polygon.attr("d", path);
                        break;
                    case 38:
                        origin[1] +=5;
                        projection.rotate(origin);
                        polygon.attr("d", path);
                        break;
                    case 39:
                        origin[0] +=5;
                        projection.rotate(origin);
                        polygon.attr("d", path);
                        break;
                    case 40:
                        origin[1] -=5;
                        projection.rotate(origin);
                        polygon.attr("d", path);
                        break;
                    default:
                        break;
                }
            });
        }
    };
});