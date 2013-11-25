define('dual', ['d3'], function(d3) {
    'use strict';

    var areEqual = function(arr1, arr2) {
        if (arr1.length !== arr2.length) {
            return false;
        }

        for (var i = 0; i < arr1.length; ++i) {
            if (Math.abs(arr1[i] - arr2[i]) > 0.1) {
                return false;
            }
        }

        return true;
    };

    var generateDual = function(faces) {
        var duals = [];
        var incomplete = faces.concat(); // Take a copy of the source data

        while (incomplete.length >= 1) {
            var current = incomplete[0];
            var points = current.coordinates[0];

            for (var i = points.length - 1; i > 0; --i) {
                var dual = {
                    type: 'Polygon',
                    coordinates: [[d3.geo.centroid(current)]]
                };

                var nextEdge = [points[i], points[i - 1]];
                var nextFace = current;
                var vertex = nextEdge[0];
                var found = false;

                do {
                    var possibleNextEdge = null;
                    found = false;

                    for (var k = 0; k < incomplete.length && !found; ++k) {
                        if (incomplete[k] === nextFace) {
                            continue;
                        }

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
                    duals.push(dual);
                }
            }

            incomplete.shift();
        }

        return duals;
    };

    return {
        generateDual: generateDual
    };
});