define('map', function() {
    'use strict';

    var map = {};

    var image = new Image();
    var context;
    var width;
    var height;

    map.render = function(imgUrl, canvasWidth, canvasHeight, callback) {
        var canvasElement = document.createElement('canvas');
        canvasElement.width = canvasWidth;
        canvasElement.height = canvasHeight;
        document.getElementById('game').appendChild(canvasElement);

        context = canvasElement.getContext('2d');
        width = canvasWidth;
        height = canvasHeight;

        image.addEventListener('load', function() {
            context.drawImage(image, 0, 0, canvasWidth, canvasHeight);
            callback();
        }, false);

        image.src = imgUrl;
    };

    map.updateSeaLevel = function(seaLevel, areaPerPixel) {
        // Convert sea level in meters to greyscale values in source from
        // http://en.m.wikipedia.org/wiki/File:Srtm_ramp2.world.21600x10800.jpg
        var threshold = (seaLevel / 50) + 13;

        var canvasElement = document.createElement('canvas');
        canvasElement.width = width;
        canvasElement.height = height;
        var rawContext = canvasElement.getContext('2d');
        rawContext.drawImage(image, 0, 0, width, height);
        var imageData = rawContext.getImageData(0, 0, width, height);

        var remainingLand = 0;
        for (var x = 0; x < imageData.width; ++x) {
            for (var y = 0; y < imageData.height; ++y) {
                var colour = getPixel(imageData, x, y);
                if ((colour.r === colour.g && colour.r === colour.b) && colour.r < threshold) {
                    setPixel(imageData, x, y, 0, 0, 255);
                } else {
                    setPixel(imageData, x, y, 0, 255, 0);
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