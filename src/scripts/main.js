require(['map'], function(map) {
    'use strict';

    var EARTH_SURFACE_AREA = 510.1;

    var w = 960;
    var h = 480;

    var seaLevel = 0;

    map.render('map.png', w, h, update);

    window.setTimeout(function() {
        window.setInterval(function() {
            ++seaLevel;
            update();
        }, 20);
    }, 1000);

    function update() {
        var remainingLand = map.updateSeaLevel(seaLevel, areaPerPixel);
        document.getElementById('seaLevel').value = seaLevel;
        document.getElementById('remainingLand').value = Math.round(remainingLand);
    }

    function areaPerPixel(y) {
        // Compensate for area distortion of plate carrée projection
        // See http://en.wikipedia.org/wiki/Equirectangular_projection
        return Math.cos(Math.PI * ((y/h) - (1/2))) * (Math.PI / 2) * (EARTH_SURFACE_AREA / (w * h));
    }
});