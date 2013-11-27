define('globe', ['jquery', 'dual','d3','geodesic',], function($, dual, d3) {
    'use strict';

    var width = 950;
    var height = 500;

    return {
        render: function() {
            var width = 960,
                height = 500;

            var origin = [-80, 20];

            var projection = d3.geo.orthographic()
                .rotate(origin)
                .scale(240)
                .clipAngle(90);

            var path = d3.geo.path()
                .projection(projection);

            var svg = d3.select("body").append("svg")
                .attr("width", width)
                .attr("height", height);

            var n = 13;

            var faces = d3.geodesic.polygons(n);
            var duals = dual.generateDual(faces);

            var previous = false;
            var polygons = svg.selectAll("path")
                .data(duals)
                .enter().append("path")
                .attr('class', function() {
                    if (previous) {
                        return (Math.random() > 0.25) ? 'land' : 'sea'
                    } else {
                        return (Math.random() > 0.5) ? 'land' : 'sea'
                    }
                });

            faces.forEach(function(face) {
                face.duals.forEach(function(first) {
                    var firstPolygon = $(polygons[0][duals.indexOf(first)]);
                    face.duals.forEach(function(second) {
                        if (first !== second) {
                            var secondPolygon = polygons[0][duals.indexOf(second)];
                            if (!firstPolygon.data('neighbours')) {
                                firstPolygon.data('neighbours', []);
                            }
                            if (firstPolygon.data('neighbours').indexOf(secondPolygon) === -1) {
                                firstPolygon.data('neighbours').push(secondPolygon);
                            }
                        }
                    })
                })
            });

            $('path').each(function(i, elem) {
                $(elem).click(function() {
                    $(elem).attr('class', 'developed land');
                    $(elem).data('neighbours').forEach(function(neighbour) {
                        $(neighbour).attr('class', 'developed land');
                    });
                });
            });

            polygons.attr("d", path);

            $(document).keydown(function(e) {
                switch (e.keyCode) {
                    case 37:
                        origin[0] -=5;
                        projection.rotate(origin);
                        polygons.attr("d", path);
                        break;
                    case 38:
                        origin[1] +=5;
                        projection.rotate(origin);
                        polygons.attr("d", path);
                        break;
                    case 39:
                        origin[0] +=5;
                        projection.rotate(origin);
                        polygons.attr("d", path);
                        break;
                    case 40:
                        origin[1] -=5;
                        projection.rotate(origin);
                        polygons.attr("d", path);
                        break;
                    default:
                        break;
                }
            });
        }
    };
});