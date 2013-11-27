define(function (require) {
    'use strict';

    var dual;

    beforeEach(function() {
        require('geodesic');
        dual = require('dual');
    });


    describe('generateDual', function() {
        it('should find the dual for a simple tetrahedron', function() {
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

            polygons.forEach(function(polygon) {
                //console.log(polygon.coordinates[0].length)
            });

            var result = dual.generateDual(polygons);

            expect(result.length).toBe(polygons.length);
            result.forEach(function(polygon) {
                expect(polygon.coordinates[0].length).toBe(4);
            });
        });

        it ('should find the dual for a minimal icosahedron', function() {
            var polygons = d3.geodesic.polygons(1);

            var result = dual.generateDual(polygons);

            expect(result.length).toBe(12);

            result.forEach(function(polygon) {
                expect(polygon.coordinates[0].length).toBe(6);
            });
        });

        it ('should find the dual for a subdivided icosahedron', function() {
            var polygons = d3.geodesic.polygons(3);

            var result = dual.generateDual(polygons);

            // Each hexagon 'uses' 6/3s of a triangle, each pentagon (of which there are twelve) uses 5/3s of a triangle
            // So the 12 pentagons use 20 triangles and the rest of the triangles are used by hexagons at a ratio of 2:1
            expect(result.length).toBe(12 + ((polygons.length - 20) / 2));

            var counts = [];
            result.forEach(function(polygon) {
                var vertices = polygon.coordinates[0].length - 1;

                counts[vertices] = (counts[vertices] || 0) + 1;
            });

            expect(counts[5]).toBe(12);
            expect(counts[6]).toBe(result.length - 12);
        });

        it('should store references to duals against original polygons', function() {
            var polygons = d3.geodesic.polygons(3);

            var result = dual.generateDual(polygons);

            polygons.forEach(function(polygon) {
                expect(polygon.duals.length).toBe(3);
            });
        });
    });
});