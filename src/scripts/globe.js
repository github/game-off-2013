define('globe', ['splat'], function(splat) {
    'use strict';

    var width = 950;
    var height = 500;

    return {
        render: function() {
            var width = 960,
                height = 500;

            var projection = d3.geo.orthographic()
                .scale(250)
                .translate([width / 2, height / 2])
                .clipAngle(90);

            var path = d3.geo.path()
                .projection(projection);

            var λ = d3.scale.linear()
                .domain([0, width])
                .range([-180, 180]);

            var φ = d3.scale.linear()
                .domain([0, height])
                .range([90, -90]);

            var svg = d3.select("body").append("svg")
                .attr("width", width)
                .attr("height", height);

            svg.on("mousemove", function() {
                var p = d3.mouse(this);
                projection.rotate([λ(p[0]), φ(p[1])]);
                svg.selectAll("path").attr("d", path);
            });

            svg.append("defs").append("path")
                .datum({type: "Sphere"})
                .attr("id", "sphere")
                .attr("d", path);

            svg.append("use")
                .attr("class", "stroke")
                .attr("xlink:href", "#sphere");

            svg.append("use")
                .attr("class", "fill")
                .attr("xlink:href", "#sphere");

            svg.append("path")
                .datum(d3.geo.graticule())
                .attr("class", "graticule")
                .attr("d", path);

            svg.append("path")
                .datum({type: 'FeatureCollection', features: [
                    {
                        geometry: splat.generate(0, 0, 24, 10),
                        properties: {}
                    }
                ]})
                .attr("class", "land")
                .attr("d", path);
        }
    };
});