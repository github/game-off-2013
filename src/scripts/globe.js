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
            var duals = [];

            var incomplete = faces.concat();

            var areEqual = function(arr1, arr2) {
                if (arr1.length !== arr2.length) {
                    return false;
                }

                for (var i = 0; i < arr1.length; ++i) {
                    if (arr1[i] !== arr2[i]) {
                        return false;
                    }
                }

                return true;
            };

            var fillInside = function(polygon) {
                if (d3.geo.area(polygon) > Math.PI) {
                    polygon.coordinates.forEach(function(coords) { coords.reverse() } );
                }
                return polygon;
            };

            while (incomplete.length >= 1) {
                var current = incomplete[0];
                var points = current.coordinates[0];

                for (var i = points.length - 2; i >= 0; --i) {
                    var dual = {
                        type: "Polygon",
                        coordinates: [[d3.geo.centroid(current)]]
                    };

                    var nextEdge = [points[i + 1], points[i]];
                    var nextFace = current;
                    var vertex = nextEdge[0];

                    do {
                        var possibleNextEdge = null;
                        var found = false;

                        for (var k = 0; k < incomplete.length && !found; ++k) {
                            if (incomplete[k] === nextFace) {
                                continue;
                            }
                            //console.log(k);

                            var other = incomplete[k].coordinates[0];

                            for (var j = 0; j < other.length - 1; ++j) {
                                var otherEdge = [other[j], other[j + 1]];

                                if ((areEqual(otherEdge[0], nextEdge[0]) && areEqual(otherEdge[1], nextEdge[1])) ||
                                    (areEqual(otherEdge[0], nextEdge[1]) && areEqual(otherEdge[1], nextEdge[0]))) {
                                    nextFace = incomplete[k];
                                    found = true;
                                } else if (areEqual(otherEdge[0], vertex) || areEqual(otherEdge[1], vertex)) {
                                    possibleNextEdge = otherEdge;
                                }
                            }
                        }

                        dual.coordinates[0].push(d3.geo.centroid(nextFace));
                        nextEdge = possibleNextEdge;
                    } while (found && (nextFace !== current));

                    if (found) {
                        // May be unnecessary?
                        duals.push(fillInside(dual));
                    }


                }

                incomplete.shift();
            }

            var polygon = svg.selectAll("path")
                .data(duals)
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