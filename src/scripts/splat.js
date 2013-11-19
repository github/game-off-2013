define('splat', function() {


    return {
        generate: function(centerLat, centerLng, radius, points) {

            var coordinates = [];

            for (var x = 180; x > -180; x -= (360 / points)) {
                coordinates.push([x, 66]);
            }

            coordinates.push(coordinates[0]);

            return {
                type: "Polygon",
                coordinates: [coordinates]
            }
        }
    };

});