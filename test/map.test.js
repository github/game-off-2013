define(function (require) {
    'use strict';

    var AREA = 1000;

    describe('map', function() {
        var Map = require('map');
        var map;
        var container;

        var mockProjection = {
            calculateFractionOfAreaInPixel: function (x, y) {
                // Pixels take up areas 0.1, 0.2, 0.3 and 0.4
                return ((y * 2) + x + 1) * 0.1;
            }
        };

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
            document.body.appendChild(container);

            var mapRendered = false;
            var onRender = function() {
                mapRendered = true;
            };

            map = new Map('base/test/map.test.png', AREA, mockProjection, container, onRender);
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
        });

        it('should initialize sea level to zero', function() {
            expect(getPixel(getCanvas(), 1, 1)).toBeLand();
            expect(getPixel(getCanvas(), 0, 1)).toBeLand();
        });

        it ('should colour pixels based on sea level', function() {
            map.updateSeaLevel(2850);

            expect(getPixel(getCanvas(), 1, 1)).toBeSea();
            expect(getPixel(getCanvas(), 0, 1)).toBeLand();
        });

        it ('should update when sea level is changed', function() {
            map.updateSeaLevel(2850);
            map.updateSeaLevel(5850);

            expect(getPixel(getCanvas(), 1, 1)).toBeSea();
            expect(getPixel(getCanvas(), 0, 1)).toBeSea();
        });

        it ('should correctly calculate remaining land', function() {
            map.updateSeaLevel(2850);
            expect(map.calculateRemainingLandArea()).toBe(0.4 * AREA);
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
    });
});