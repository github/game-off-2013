define(function (require) {
    'use strict';

    var dual;

    beforeEach(function() {
        dual = require('dual');

    });

    describe('grouping faces by corresponding face in the dual', function() {
        it('should identify vertex nodes for dual faces contained within an icosahedron face', function() {
            var actual = dual.findFaceRelativeVertexNodes(5);

            var expected = [
                [1,3,2,8,5,7,1],
                [4,7,5,14,10,13,4],
                [5,8,6,15,11,14,5],
                [9,13,10,22,17,21,9],
                [10,14,11,23,18,22,10],
                [11,15,12,24,19,23,11]
            ];

            expect(actual).toEqual(expected);
        });

        it('should identify vertex nodes for dual faces on all icosohedron faces', function() {
            var actual = dual.findFaceVertexNodes(3);

            expect(actual.length).toEqual(20);
            expect(actual[0]).toEqual([1,3,2,8,5,7,1]);
            expect(actual[10]).toEqual([91,93,92,98,95,97,91]);
        });

        it('should identify vertex nodes for dual faces along icosahedron edges', function() {
            var edgeMap = [
                [[2,4,6],[6,8,10]],
                [[1,3,5],[5,7,9]]
            ];

            var edgeJoins = [[
                { face: 0, edge:0 },
                { face: 2, edge:1 }
            ]];

            var n = 3;

            var expected = [
                [2,4,6,23,21,19,2],
                [6,8,10,27,25,23,6]
            ];

            var actual = dual.findEdgeVertexNodes(n, edgeMap, edgeJoins);

            expect(actual).toEqual(expected);
        });

        it('should identify vertex nodes for dual faces along reversed icosahedron edges', function() {
            var edgeMap = [
                [[2,4,6],[6,8,10]],
                [[1,3,5],[5,7,9]]
            ];

            var edgeJoins = [[
                { face: 0, edge:0 },
                { face: 2, edge:1, reverse: true }
            ]];

            var n = 3;

            var expected = [
                [2,4,6,23,25,27,2],
                [6,8,10,19,21,23,6]
            ];

            var actual = dual.findEdgeVertexNodes(n, edgeMap, edgeJoins);

            expect(actual).toEqual(expected);
        });
    });

    describe('buildEdgeMap', function() {
        it('should return nodes in hexagons along edges', function() {
            var actual = dual.buildEdgeMap(4);

            var expected = [
                [[0,3,1], [1,7,4], [4,13,9]],
                [[9,13,10], [10,14,11], [11,15,12]],
                [[12,15,6], [6,8,2], [2,3,0]]
            ];

            expect(actual).toEqual(expected);
        });
    });

    describe('findEdgeJoins', function() {
        it('should find matching edges', function() {
            var faces = [
                [[0,0], [1,2], [2,0]],
                [[3,2],[2,0],[1,2]],
                [[2,0],[4,0],[3,2]]
            ];

            var expected = [[
                { face: 0, edge: 1 },
                { face: 1, edge: 1, reverse: true }
            ],[
                { face: 1, edge: 0 },
                { face: 2, edge: 2 }
            ]];

            var actual = dual.findEdgeJoins(faces);

            expect(actual).toEqual(expected);
        });
    });
});