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
                    if (cell.attributes.indexOf('land') > -1) {
                        ++landArea;
                    }
                });

                expect(landArea).toBe(65);
            });

            it('should clump land cells together', function() {
                // TODO:HGC
            });
        });

        afterEach(function() {
            terrain = null;
        });
    });
});