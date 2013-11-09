define(function (require) {
    'use strict';

    describe('map', function() {
        var map;
        var container;

        beforeEach(function() {
            container = document.createElement('div');
            container.id = 'game';
            document.body.appendChild(container);
            map = require('map');
            var mapRendered = false;
            map.render('base/test/map.test.png', 2, 2, function() {
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

            var pixel = getPixel(getCanvas(), 1, 1);
            expect(pixel.data[0]).toBe(0);
            expect(pixel.data[1]).toBe(0);
            expect(pixel.data[2]).toBe(255);

            var pixel = getPixel(getCanvas(), 0, 1);
            expect(pixel.data[0]).toBe(0);
            expect(pixel.data[1]).toBe(255);
            expect(pixel.data[2]).toBe(0);
        });

        it ('should update when sea level is changed', function() {
            map.updateSeaLevel(2850, areaPerPixel);
            map.updateSeaLevel(5850, areaPerPixel);

            var pixel = getPixel(getCanvas(), 1, 1);
            expect(pixel.data[0]).toBe(0);
            expect(pixel.data[1]).toBe(0);
            expect(pixel.data[2]).toBe(255);

            var pixel = getPixel(getCanvas(), 0, 1);
            expect(pixel.data[0]).toBe(0);
            expect(pixel.data[1]).toBe(0);
            expect(pixel.data[2]).toBe(255);
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