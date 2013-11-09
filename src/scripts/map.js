define('map', function() {
    'use strict';

    return {
        render: function(imgUrl, canvasWidth, canvasHeight, callback) {
            var canvasElement = document.createElement('canvas');
            canvasElement.width = canvasWidth;
            canvasElement.height = canvasHeight;
            document.body.appendChild(canvasElement);

            var context = canvasElement.getContext('2d');

            var image = new Image();

            image.addEventListener("load", function() {
                context.drawImage(image, 0, 0, canvasWidth, canvasHeight);
                callback();
            }, false);

            image.src = imgUrl;
        }
    };
});