define('grid', ['dual','d3','geodesic',], function(dual) {
    'use strict';

    var generate = function(n) {
        var faces = d3.geodesic.polygons(n);
        var cells = dual.generateDual(faces);

        faces.forEach(function(face) {
            face.duals.forEach(function(first) {
                face.duals.forEach(function(second) {
                    if (first !== second) {
                        if (!first.neighbours) {
                            first.neighbours = [];
                        }
                        if (first.neighbours.indexOf(second) === -1) {
                            first.neighbours.push(second);
                        }
                    }
                });
            });
        });

        return cells;
    };

    return {
        generate: generate
    };
});
