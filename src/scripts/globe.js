define('globe', ['dual'], function(dual) {
    'use strict';

    var width = 950;
    var height = 500;

    return {
        render: function() {
            var width = 960,
                height = 500;

            var origin = [-80, 20],
                velocity = [0.029, 0.01],
                t0 = Date.now();

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

            var centroids = d3.geodesic.polygons(n).map(function(d) {
                return d3.geo.centroid(d);
            });

            var dualMap = dual.findFaceVertexNodes(n).concat(
                dual.findEdgeVertexNodes(n, dual.buildEdgeMap(n), dual.findEdgeJoins(d3.geodesic.faces)));

            var fillInside = function(polygon) {
                if (d3.geo.area(polygon) > Math.PI) {
                    polygon.coordinates.forEach(function(coords) { coords.reverse() } );
                }
                return polygon;
            };

            var hexes = dualMap.map(function(duals) {
                return fillInside({
                    type: "Polygon",
                    coordinates: [duals.map(function(centroidIndex) {
                        return centroids[centroidIndex];
                    })]
                });
            });

            var polygon = svg.selectAll("path")
                .data(hexes)
                .enter().append("path");
                //.style("fill", function(d, i) { return d3.hsl(i * 10 / n, .7, .5); });

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