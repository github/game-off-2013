define('map', function() {
    'use strict';

    var map = {};

    var image = new Image();
    var context;

    map.render = function(imgUrl, callback) {
        var rendered = false;

        image.addEventListener('load', function() {
            if (!rendered) {
                var canvasElement = document.createElement('canvas');
                canvasElement.width = image.width;
                canvasElement.height = image.height;
                document.getElementById('game').appendChild(canvasElement);
                context = canvasElement.getContext('2d');
                context.drawImage(image, 0, 0, image.width, image.height);
                rendered = true;
                callback();
            }

        }, false);

        image.src = imgUrl;
    };

    map.updateSeaLevel = function(seaLevel, areaPerPixel) {
        // Convert sea level in meters to greyscale values in source from
        // http://en.m.wikipedia.org/wiki/File:Srtm_ramp2.world.21600x10800.jpg
        var threshold = (seaLevel / 50) + 13;

        var canvasElement = document.createElement('canvas');
        canvasElement.width = image.width;
        canvasElement.height = image.height;
        var rawContext = canvasElement.getContext('2d');
        rawContext.drawImage(image, 0, 0, image.width, image.height);
        var imageData = rawContext.getImageData(0, 0, image.width, image.height);

        var remainingLand = 0;
        for (var x = 0; x < imageData.width; ++x) {
            for (var y = 0; y < imageData.height; ++y) {
                var colour = getPixel(imageData, x, y);
                if ((colour.r === colour.g && colour.r === colour.b) && colour.r < threshold) {
                    setPixel(imageData, x, y, 0, 105, 148);
                } else {
                    setPixel(imageData, x, y, colour.r * 2, colour.g * 3 + 30, colour.b * 1.5);
                    remainingLand += areaPerPixel(y);
                }
            }
        }

        context.putImageData(imageData, 0, 0);
        return remainingLand;
    };

    function setPixel(imageData, x, y, r, g, b) {
        var index = (x + y * imageData.width) * 4;
        imageData.data[index+0] = r;
        imageData.data[index+1] = g;
        imageData.data[index+2] = b;
    }

    function getPixel(imageData, x, y) {
        var index = (x + y * imageData.width) * 4;
        return {
            r: imageData.data[index+0],
            g: imageData.data[index+1],
            b: imageData.data[index+2]
        };
    }

    return map;
});