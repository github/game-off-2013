define(['require', 'Squire'], function (require, Squire) {
    'use strict';

    var createCell = function() {
        return {
            neighbours: []
        };
    };

    var createGrid = function() {
        var cells = [];
        for (var i = 0; i < 10; ++i) {
            for (var j = 0; j < 10; ++j) {
                var cell = createCell();

                if (i > 0) {
                    var upNeighbour = cells[cells.length - 10];
                    upNeighbour.neighbours.push(cell);
                    cell.neighbours.push(upNeighbour);
                }

                if (j > 0) {
                    var leftNeighbour = cells[10 * i + j - 1];
                    leftNeighbour.neighbours.push(cell);
                    cell.neighbours.push(leftNeighbour);
                }

                cells.push(cell);
            }
        }

        return cells;
    };

    function isLand(cell) {
        return cell.attributes.indexOf('land') > -1;
    }

    describe('terrain', function() {
        var cells;
        var terrain;
        var d3;

        beforeEach(function() {
            cells = [];

            d3 = {
                geo: jasmine.createSpyObj('geo', ['centroid'])
            };

            d3.geo.centroid.andReturn([0,0]);

            runs(function() {
                new Squire()
                    .mock('d3', d3)
                    .require(['terrain'], function (terrainWithMockDeps) {
                        terrain = terrainWithMockDeps;
                    })
            });

            waitsFor(function() {
                return terrain;
            }, "", 500)
        });

        describe('generate', function() {
            it('should mark the poles as polar', function() {
                cells.push(createCell());
                cells.push(createCell());
                cells.push(createCell());

                d3.geo.centroid.andCallFake(function(cell) {
                    return [[0, 80], [0, 0], [0, -80]][cells.indexOf(cell)];
                });

                terrain.generate(cells);

                expect(cells[0].attributes).toContain('polar');
                expect(cells[1].attributes).not.toContain('polar');
                expect(cells[2].attributes).toContain('polar');
            });

            it('should mark the correct proportion of cells as land', function() {
                cells = createGrid();

                terrain.generate(cells, 0.65);

                var landArea = 0;
                cells.forEach(function(cell) {
                    if (isLand(cell)) {
                        ++landArea;
                    }
                });

                expect(landArea).toBe(65);
            });

            it('should mark the remaining cells as sea', function() {
                cells = createGrid();

                terrain.generate(cells, 0.65);

                var seaArea = 0;
                cells.forEach(function(cell) {
                    if (cell.attributes.indexOf('sea') > -1) {
                        ++seaArea;
                    }
                });

                expect(seaArea).toBe(35);
            });

            it('should clump land together locally', function() {
                cells = createGrid();

                terrain.generate(cells, 0.65);

                var landLandNeighbours = 0;
                var landSeaNeighbours = 0;
                cells.forEach(function(cell) {
                    cell.neighbours.forEach(function(neighbour) {
                        if (isLand(neighbour) && isLand(cell)) {
                            ++landLandNeighbours;
                        } else if (isLand(neighbour) || isLand(cell)) {
                            ++landSeaNeighbours;
                        }
                    });
                });

                expect(landLandNeighbours).toBeGreaterThan(landSeaNeighbours * 1.5);
            });

            it('should distribute land globally', function() {
                cells = createGrid();

                terrain.generate(cells, 0.65);

                var averageIndex = 0;
                cells.forEach(function(cell, index) {
                    if (isLand(cell)) {
                        averageIndex += index / cells.length;
                    }
                });

                expect(averageIndex).toBeGreaterThan(0.25 * cells.length);
                expect(averageIndex).toBeLessThan(0.75 * cells.length);
            });
        });

        afterEach(function() {
            terrain = null;
        });
    });
});