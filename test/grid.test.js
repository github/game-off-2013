define(function (require) {
    'use strict';

    var grid;

    describe('grid', function() {
        beforeEach(function() {
            grid = require('grid');
        });

        describe('generate', function() {
            it ('should generate a globe with the correct number of cells', function() {
                var cells = grid.generate(3);
                expect(cells.length).toBe(92);
            });

            it ('should associate cells with their neighbours', function() {
                var cells = grid.generate(4);

                cells.forEach(function(cell) {
                    expect(cell.neighbours.length).toBe(cell.coordinates[0].length - 1);
                    cell.neighbours.forEach(function(neighbour) {
                        expect(neighbour.neighbours.indexOf(cell)).toBeGreaterThan(-1);
                    });
                });
            });
        });
    });
});