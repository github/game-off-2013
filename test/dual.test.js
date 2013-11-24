define(function (require) {
    'use strict';

    var dual;

    beforeEach(function() {
        dual = require('dual');
    });


    describe('generateDual', function() {
        it('should round-trip a simple tetrahedron', function() {
            var vertices = [[0,90], [-60,-30], [60,-30], [180,-30]];
            var faces = [[0,2,1],[0,3,2],[0,1,3],[1,2,3]].map(function(triple) {
                return triple.map(function(i) {
                    return vertices[i];
                })
            });

            var polygons = faces.map(function(face) {
                return {
                    type: "Polygon",
                    coordinates: [face.concat([face[0]])]
                }
            });

            var result = dual.generateDual(polygons);

            expect(result).toEqual(polygons);
        });
    });
});