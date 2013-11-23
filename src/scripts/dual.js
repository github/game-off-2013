define('dual', function() {
    'use strict';

    var findFaceRelativeVertexNodes = function(n) {
        var duals = [];
        for (var i = 1; i < n-1; ++i) {
            for (var j = 0; j < i; ++j) {
                var a = i*i+j;
                duals.push([
                    a,              // Starting node
                    a + i + 1,      // Neighbouring node in intersecting row
                    a + 1,          // Next node in own row
                    a + 3 * i + 4,  // SCIENCE!
                    a + 2 * i + 2,  // SCIENCE!
                    a + 3 * i + 3,  // SCIENCE!
                    a]
                );
            }
        }
        return duals;
    };

    var findFaceVertexNodes = function(n) {
        var duals = [];
        var faceRelativeDuals = findFaceRelativeVertexNodes(n);
        for (var i = 0; i < 20; ++i) {
            var start = i * n * n;
            duals = duals.concat(faceRelativeDuals.map(function(nodes) {
                return nodes.map(function(node) {
                    return node + start;
                })
            }));
        }
        return duals;
    };

    var findEdgeVertexNodes = function(n, edgeMap, edgeJoins) {
        var nn = n * n;
        var duals = [];
        edgeJoins.forEach(function(edgeJoin) {
            for (var i = 0; i < n - 1; ++i) {
                var nodes = [];
                var j;
                for (j = 0; j < 3; ++j) {
                    nodes.push(edgeJoin[0].face * nn + edgeMap[edgeJoin[0].edge][i][j]);
                }
                for (j = 0; j < 3; ++j) {
                    var dualIndex;
                    var nodeIndex;

                    if (edgeJoin[1].reverse) {
                        dualIndex = n-2-i;
                        nodeIndex = j;
                    } else {
                        dualIndex = i;
                        nodeIndex = 2 - j;
                    }

                    nodes.push(edgeJoin[1].face * nn + edgeMap[edgeJoin[1].edge][dualIndex][nodeIndex]);
                }
                nodes.push(nodes[0]);
                duals.push(nodes);
            }
        });
        return duals;
    };

    var buildEdgeMap = function(n) {
        var map = [[], [], []];

        for (var i = 0; i < n - 1; ++i) {
            map[0].push([i*i, (i + 2) * (i + 2) - (i + 1), (i + 1) * (i + 1)]);
            map[2].push([(i + 1) * (i + 2),(i + 2) * (i + 2) - 1, i * (i + 1)]);
            map[1].push([(n - 2) * n + i + 1, (n - 1) * n + 1 + i, (n - 2) * n + i + 2]);
        }

        map[2].reverse();

        return map;
    };

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

    var findEdgeJoins = function(faces) {
        var incomplete = faces.concat(); // Take a shallow copy

        var edgeJoins = [];
        var faceIndex = 0;

        while (incomplete.length > 1) {
            var current = incomplete.shift();

            for (var i = 0; i < 3; ++i) {
                var edge = [current[i], current[(i + 1) % 3]];

                incomplete.forEach(function(other, relativeIndex) {
                    for (var j = 0; j < 3; ++j) {
                        var otherEdge = [other[j], other[(j + 1) % 3]];

                        var edgeJoin = [
                            { face: faceIndex, edge: i },
                            { face: faceIndex + relativeIndex + 1, edge: j }
                        ];

                        if (areEqual(otherEdge[0], edge[0]) && areEqual(otherEdge[1], edge[1])) {
                            edgeJoins.push(edgeJoin);
                            break;
                        } else if (areEqual(otherEdge[0], edge[1]) && areEqual(otherEdge[1], edge[0])) {
                            edgeJoin[1].reverse = true;
                            edgeJoins.push(edgeJoin);
                            break;
                        }
                    }
                });
            }

            ++faceIndex;
        }

        return edgeJoins;
    };

    return {
        findFaceRelativeVertexNodes: findFaceRelativeVertexNodes,
        buildEdgeMap: buildEdgeMap,
        findEdgeJoins: findEdgeJoins,
        findEdgeVertexNodes: findEdgeVertexNodes,
        findFaceVertexNodes: findFaceVertexNodes
    };
});