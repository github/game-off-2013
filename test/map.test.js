define(function (require) {
    'use strict';

    describe('map', function() {
        var map;

        beforeEach(function() {
            map = require('map');
        });

        it('should render the map to a canvas', function() {
            var renderComplete = false;

            runs(function() {
                map.render('base/test/map.test.png', 2, 2, function() {
                    renderComplete = true;
                });
            });

            waitsFor(function() {
                return renderComplete;
            }, "The map should be rendered", 5000);

            runs(function() {
                expect(document.getElementsByTagName('canvas').length).toBe(1);

                var canvas = document.getElementsByTagName('canvas')[0];
                expect(canvas.width).toBe(2);
                expect(canvas.height).toBe(2);

                var context = canvas.getContext('2d');

                var pixel = context.getImageData(1, 1, 1, 1);
                expect(pixel.data[0]).toBe(128);
                expect(pixel.data[1]).toBe(128);
                expect(pixel.data[2]).toBe(128);
            });
        });

        it ('should colour pixels based on sea level', function() {

        });
    });
});