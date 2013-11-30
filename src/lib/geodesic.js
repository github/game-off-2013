/*
 Copyright (c) 2013, Michael Bostock
 All rights reserved.

 Redistribution and use in source and binary forms, with or without
 modification, are permitted provided that the following conditions are met:

 * Redistributions of source code must retain the above copyright notice, this
 list of conditions and the following disclaimer.

 * Redistributions in binary form must reproduce the above copyright notice,
 this list of conditions and the following disclaimer in the documentation
 and/or other materials provided with the distribution.

 * The name Michael Bostock may not be used to endorse or promote products
 derived from this software without specific prior written permission.

 THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 DISCLAIMED. IN NO EVENT SHALL MICHAEL BOSTOCK BE LIABLE FOR ANY DIRECT,
 INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY
 OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

(function() {
  var phi = 1.618033988749895,
      rho = 180 / Math.PI;

  // Co-ordinates of an icosahedron
  var vertices = [
    [1,phi,0], [-1,phi,0], [1,-phi,0], [-1,-phi,0],
    [0,1,phi], [0,-1,phi], [0,1,-phi], [0,-1,-phi],
    [phi,0,1], [-phi,0,1], [phi,0,-1], [-phi,0,-1]
  ];

  var faces = [
    [0,1,4],  [1,9,4],  [4,9,5],  [5,9,3],  [2,3,7],
    [3,2,5],  [7,10,2], [0,8,10], [0,4,8],  [8,2,10],
    [8,4,5],  [8,5,2],  [1,0,6],  [11,1,6], [3,9,11],
    [6,10,7], [3,11,7], [11,6,7], [6,0,10], [9,1,11]
  ].map(function(face) {
    return face.map(function(i) {
      return vertices[i];
    });
  });

  d3.geodesic = {
    multipolygon: function(n) {
      return {
        type: "MultiPolygon",
        coordinates: subdivideFaces(~~n).map(function(face) {
          face = face.map(project);
          face.push(face[0]);
          face = [face];
          if (d3.geo.area({type: "Polygon", coordinates: face}) > Math.PI) face[0].reverse();
          return face;
        })
      };
    },
    polygons: function(n) {
      return d3.geodesic.multipolygon(~~n).coordinates.map(function(face) {
        return {type: "Polygon", coordinates: face};
      });
    },
    multilinestring: function(n) {
      return {
        type: "MultiLineString",
        coordinates: subdivideEdges(~~n).map(function(edge) {
          return edge.map(project);
        })
      };
    },
    faces: faces
  };

  function subdivideFaces(n) {
    return d3.merge(faces.map(function(face) {
      var i01 = interpolate(face[0], face[1]),
          i02 = interpolate(face[0], face[2]),
          faces = [];

      faces.push([
        face[0],
        i01(1 / n),
        i02(1 / n)
      ]);

      for (var i = 1; i < n; ++i) {
        var i1 = interpolate(i01(i / n), i02(i / n)),
            i2 = interpolate(i01((i + 1) / n), i02((i + 1) / n));
        for (var j = 0; j <= i; ++j) {
          faces.push([
            i1(j / i),
            i2(j / (i + 1)),
            i2((j + 1) / (i + 1))
          ]);
        }
        for (var j = 0; j < i; ++j) {
          faces.push([
            i1(j / i),
            i1((j + 1) / i),
            i2((j + 1) / (i + 1))
          ]);
        }
      }

      return faces;
    }));
  }

  function subdivideEdges(n) {
    var edges = {};

    subdivideFaces(n).forEach(function(face) {
      add(face[0], face[1]);
      add(face[1], face[2]);
      add(face[2], face[0]);
    });

    function add(p0, p1) {
      var t;
      if (p0[0] < p1[0] || (p0[0] == p1[0] && (p0[1] < p1[1] || (p0[1] == p1[1] && p0[2] < p1[2])))) t = p0, p0 = p1, p1 = t;
      edges[p0.map(round) + " " + p1.map(round)] = [p0, p1];
    }

    function round(d) {
      return d3.round(d, 4);
    }

    return d3.values(edges);
  }

  function interpolate(p0, p1) {
    var x0 = p0[0],
        y0 = p0[1],
        z0 = p0[2],
        x1 = p1[0] - x0,
        y1 = p1[1] - y0,
        z1 = p1[2] - z0;
    return function(t) {
      return [
        x0 + t * x1,
        y0 + t * y1,
        z0 + t * z1
      ];
    };
  }

  // Project our x, y, z co-ordinates int latitude and longitude
  function project(p) {
    var x = p[0],
        y = p[1],
        z = p[2];
    return [
      Math.atan2(y, x) * rho,
      Math.acos(z / Math.sqrt(x * x + y * y + z * z)) * rho - 90
    ];
  }
})();
