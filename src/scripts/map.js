define('map', function() {
    'use strict';

    var map = {};


    return function(imgUrl, area, projection, domElement, renderCallback) {
        var map = this;

        var context;

        render();

        function render() {
            var rendered = false;

            map.image = new Image();
            map.image.addEventListener('load', function() {
                if (!rendered) {
                    var canvasElement = document.createElement('canvas');
                    canvasElement.width = map.image.width;
                    canvasElement.height = map.image.height;

                    map.context = canvasElement.getContext('2d');
                    map.context.drawImage(map.image, 0, 0, map.image.width, map.image.height);
                    map.rawImageData = map.context.getImageData(0, 0, map.image.width, map.image.height);

                    domElement.appendChild(canvasElement);

                    map.updateSeaLevel(0);

                    rendered = true;
                    if (typeof(renderCallback) === "function") {
                        renderCallback();
                    }
                }

            }, false);

            map.image.src = imgUrl;
        }

        this.updateSeaLevel = function(seaLevel) {
            this.seaLevel = seaLevel;

            var imageData = map.context.getImageData(0, 0, map.image.width, map.image.height);

            for (var x = 0; x < map.image.width; ++x) {
                for (var y = 0; y < map.image.height; ++y) {
                    if (shouldPixelBeLand(x, y)) {
                        setPixelLand(imageData, x, y);
                    } else {
                        setPixelWater(imageData, x, y);
                    }
                }
            }

            map.context.putImageData(imageData, 0, 0);
        };

        this.calculateRemainingLandArea = function() {
            var remainingLandProportion = 0;

            for (var x = 0; x < map.image.width; ++x) {
                for (var y = 0; y < map.image.height; ++y) {
                    if (shouldPixelBeLand(x, y)) {
                        remainingLandProportion += projection.calculateFractionOfAreaInPixel(x,
                                                                                             y,
                                                                                             map.image.width,
                                                                                             map.image.height);
                    }
                }
            }

            return Math.round(remainingLandProportion * area);
        }

        function shouldPixelBeLand(x, y) {
            // Convert sea level in meters to greyscale values in source from
            // http://en.m.wikipedia.org/wiki/File:Srtm_ramp2.world.21600x10800.jpg
            var threshold = (map.seaLevel / 50) + 13;
            var pixelColour = getPixelColour(map.rawImageData, x, y);
            return !(pixelColour.r === pixelColour.g
                     && pixelColour.r === pixelColour.b
                     && pixelColour.r < threshold);
        }

        function setPixelLand(imageData, x, y) {
            var rawPixelColour = getPixelColour(map.rawImageData, x, y);
            setPixelColour(imageData, x, y, rawPixelColour.r * 2, rawPixelColour.g * 3 + 30, rawPixelColour.b * 1.5);
        }

        function setPixelWater(imageData, x, y) {
            setPixelColour(imageData, x, y, 0, 105, 148);
        }

        function setPixelColour(imageData, x, y, r, g, b) {
            var index = (x + y * imageData.width) * 4;
            imageData.data[index+0] = r;
            imageData.data[index+1] = g;
            imageData.data[index+2] = b;
        }

        function getPixelColour(imageData, x, y) {
            var index = (x + y * imageData.width) * 4;
            return {
                r: imageData.data[index+0],
                g: imageData.data[index+1],
                b: imageData.data[index+2]
            };
        }

    }
});