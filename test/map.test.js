define(function (require) {
    'use strict';

    describe('map', function() {
        var map;
        var container;

        beforeEach(function() {
            this.addMatchers({
                toBeLand: function() {
                    // More green than blue
                    return this.actual.data[1] > this.actual.data[2];
                },
                toBeSea: function() {
                    // More blue than green
                    return this.actual.data[2] > this.actual.data[1];
                }
            });

            container = document.createElement('div');
            container.id = 'game';
            document.body.appendChild(container);
            map = require('map');
            var mapRendered = false;
            map.render('base/test/map.test.png', function() {
                mapRendered = true;
            });
            waitsFor(function() {
                return mapRendered;
            }, "The map should be rendered", 500);
        });

        afterEach(function() {
            document.body.removeChild(container);
        });

        it('should render the map to a canvas', function() {
            var canvas = getCanvas();
            expect(canvas.width).toBe(2);
            expect(canvas.height).toBe(2);

            var pixel = getPixel(canvas, 0, 1);
            expect(pixel.data[0]).toBe(128);
            expect(pixel.data[1]).toBe(128);
            expect(pixel.data[2]).toBe(128);
        });

        it ('should colour pixels based on sea level', function() {
            map.updateSeaLevel(2850, areaPerPixel);

            expect(getPixel(getCanvas(), 1, 1)).toBeSea();
            expect(getPixel(getCanvas(), 0, 1)).toBeLand();
        });

        it ('should update when sea level is changed', function() {
            map.updateSeaLevel(2850, areaPerPixel);
            map.updateSeaLevel(5850, areaPerPixel);

            expect(getPixel(getCanvas(), 1, 1)).toBeSea();
            expect(getPixel(getCanvas(), 0, 1)).toBeSea();
        });

        it ('should return remaining land', function() {
            var result = map.updateSeaLevel(2850, areaPerPixel);
            expect(result).toBe(3);
        });

        function getCanvas() {
            expect(container.getElementsByTagName('canvas').length).toBe(1);
            var canvas = container.getElementsByTagName('canvas')[0];
            return canvas;
        }

        function getPixel(canvas, x, y) {
            var context = canvas.getContext('2d');
            return context.getImageData(x, y, 1, 1);
        }

        function areaPerPixel(y) {
            return y + 1;
        }
    });
});