define('plateCareeProjection', function() {
    'use strict';

    return {
        calculateFractionOfAreaInPixel: function(x, y, width, height) {
            // Compensate for area distortion of plate carrée projection
            // See http://en.wikipedia.org/wiki/Equirectangular_projection
            return Math.cos(Math.PI * ((y/height) - (1/2))) * (Math.PI / 2)  / (width * height);
        }
    };
});