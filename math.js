function doPolygonsIntersect(a, b) {
	var polygons = [a, b];
	var minA, maxA, projected, i, i1, j, minB, maxB;
	var totalMin;
	for (i = 0; i < polygons.length; i++) {
		// for each polygon, look at each edge of the polygon, and determine if it separates
		// the two shapes
		var polygon = polygons[i];
		for (i1 = 0; i1 < polygon.length; i1++) {
			// grab 2 vertices to create an edge
			var i2 = (i1 + 1) % polygon.length;
			var p1 = polygon[i1];
			var p2 = polygon[i2];

			// find the line perpendicular to this edge
			var normal = {
				x: p2.y - p1.y,
				y: p1.x - p2.x
			};

			minA = maxA = undefined;
			// for each vertex in the first shape, project it onto the line perpendicular to the edge
			// and keep track of the min and max of these values
			for (j = 0; j < a.length; j++) {
				projected = normal.x * a[j].x + normal.y * a[j].y;
				if (isUndefined(minA) || projected < minA) {
					minA = projected;
				}
				if (isUndefined(maxA) || projected > maxA) {
					maxA = projected;
				}
			}

			// for each vertex in the second shape, project it onto the line perpendicular to the edge
			// and keep track of the min and max of these values
			minB = maxB = undefined;
			for (j = 0; j < b.length; j++) {
					projected = normal.x * b[j].x + normal.y * b[j].y;
				if (isUndefined(minB) || projected < minB) {
					minB = projected;
				}
				if (isUndefined(maxB) || projected > maxB) {
					maxB = projected;
				}
			}

			// if there is no overlap between the projects, the edge we are looking at separates the two
			// polygons, and we know there is no overlap
			if (maxA < minB || maxB < minA) {
				return false;
			}
		}
	}
	return true;
}

function sqr(x) {
	return x * x;
}

function dist2(v, w) {
	return sqr(v.x - w.x) + sqr(v.y - w.y);
}

function distToSegmentSquared(p, v, w) {
	var l2 = dist2(v, w);
	if (l2 === 0) return dist2(p, v);
	var t = ((p.x - v.x) * (w.x - v.x) + (p.y - v.y) * (w.y - v.y)) / l2;
	if (t < 0) return dist2(p, v);
	if (t > 1) return dist2(p, w);
	return dist2(p, { x: v.x + t * (w.x - v.x),
	y: v.y + t * (w.y - v.y) });
}

function distToSegment(p, v, w) {
	return Math.sqrt(distToSegmentSquared(p, v, w));
}

function isUndefined(zz) {
	if (zz === undefined) {
		return true;
	}
}

function rotatePoint(pX, pY, oX, oY, rAngle) {
	return {
		x: Math.cos(rAngle) * (pX - oX) - Math.sin(rAngle) * (pY - oY) + oX,
		y: Math.sin(rAngle) * (pX - oX) + Math.cos(rAngle) * (pY - oY) + oY
	};
}

function reverseInsertionSort(items) {
	var len = items.length,
		value,
		i,
		j;
	
	for (i=0; i < len; i++) {
		value = items[i];
		for (j=i-1; j > -1 && items[j] < value; j--) {
			items[j+1] = items[j];
		}
		items[j+1] = value;
	}
	return items;
}

function drawBullets() {
	var h = 6; //actually is width
	var w = 10; //actually is height
	var bulletsToDestroy = [];
	for (var i = 0; i < bullets.length; i++) {
		if (bullets[i]["x"] > 1024 || bullets[i]["x"] < 0 || bullets[i]["y"] > 640 || bullets[i]["y"] < 0) {
			bulletsToDestroy.push(i);
		}
		else {
			var intersection = false;
			var ang = Math.atan(bullets[i]["vy"]/bullets[i]["vx"]);
			var bulletVertices = [
				rotatePoint(bullets[i]["x"], bullets[i]["y"], bullets[i]["x"] + w/2, bullets[i]["y"] + h/2, ang),
				rotatePoint(bullets[i]["x"] + w, bullets[i]["y"], bullets[i]["x"] + w/2, bullets[i]["y"] + h/2, ang),
				rotatePoint(bullets[i]["x"] + w, bullets[i]["y"] + h, bullets[i]["x"] + w/2, bullets[i]["y"] + h/2, ang),
				rotatePoint(bullets[i]["x"], bullets[i]["y"] + h, bullets[i]["x"] + w/2, bullets[i]["y"] + h/2, ang)
			];

			for (var avert in mapData['vertices']) {
				if (doPolygonsIntersect(mapData['vertices'][avert],bulletVertices)) {
					bulletsToDestroy.push(i);
					intersection = true;
					break;
				}
			}
			if (!intersection) {
				this.ctx.save();
				this.ctx.translate(bullets[i]["x"] + w/2,bullets[i]["y"] + h/2);
				this.ctx.rotate(ang);
				ctx.fillStyle = "rgb(230,230,230)";
				this.ctx.fillRect(-w/2,-h/2,w,h);
				this.ctx.restore();
				bullets[i]["x"] += bullets[i]["vx"] * tDelta;
				bullets[i]["y"] += bullets[i]["vy"] * tDelta;
			}
		}
	}

	bulletsToDestroy = reverseInsertionSort(bulletsToDestroy);

	for (var j = 0; j < bulletsToDestroy.length; j++) {
		bullets.splice(bulletsToDestroy[j], 1);
	}
}

function intersect(x1, y1, x2, y2, x3, y3, x4, y4){
	var a1, a2, b1, b2, c1, c2;
	var r1, r2 , r3, r4;
	var denom, offset, num;

	// Compute a1, b1, c1, where line joining points 1 and 2
	// is "a1 x + b1 y + c1 = 0".
	a1 = y2 - y1;
	b1 = x1 - x2;
	c1 = (x2 * y1) - (x1 * y2);

	// Compute r3 and r4.
	r3 = ((a1 * x3) + (b1 * y3) + c1);
	r4 = ((a1 * x4) + (b1 * y4) + c1);

	// Check signs of r3 and r4. If both point 3 and point 4 lie on
	// same side of line 1, the line segments do not intersect.
	if ((r3 !== 0) && (r4 !== 0) && sameSign(r3, r4)){
		return 0; //return that they do not intersect
	}

	// Compute a2, b2, c2
	a2 = y4 - y3;
	b2 = x3 - x4;
	c2 = (x4 * y3) - (x3 * y4);

	// Compute r1 and r2
	r1 = (a2 * x1) + (b2 * y1) + c2;
	r2 = (a2 * x2) + (b2 * y2) + c2;

	// Check signs of r1 and r2. If both point 1 and point 2 lie
	// on same side of second line segment, the line segments do
	// not intersect.
	if ((r1 !== 0) && (r2 !== 0) && (sameSign(r1, r2))){
		return 0; //return that they do not intersect
	}

	//Line segments intersect: compute intersection point.
	denom = (a1 * b2) - (a2 * b1);

	if (denom === 0) {
		return 1; //collinear
	}

	if (denom < 0){
		offset = -denom / 2;
	}
	else {
		offset = denom / 2 ;
	}

	// The denom/2 is to get rounding instead of truncating. It
	// is added or subtracted to the numerator, depending upon the
	// sign of the numerator.
	// num = (b1 * c2) - (b2 * c1);
	// if (num < 0){
	//	x = (num - offset) / denom;
	// } 
	// else {
	//	x = (num + offset) / denom;
	// }

	// num = (a2 * c1) - (a1 * c2);
	// if (num < 0){
	//	y = ( num - offset) / denom;
	// } 
	// else {
	//	y = (num + offset) / denom;
	// }

	// lines_intersect
	return 1; //lines intersect, return true
}

function sameSign(a, b){
	return (( a * b) >= 0);
}

function calcDecay() {
	if (activePlayer == player1) {
		if (player1.decaying['x']) {
			if (player1.ax > 0) {
				if (player1.ax > 1.34) {
					player1.ax -= 1.34;
				}
				else {
					player1.ax = 0;
				}
			}
			else {
				if (player1.ax < 0) {
					if (player1.ax < -1.34) {
						player1.ax += 1.34;
					}
					else {
						player1.ax = 0;
					}	
				}
				else {
					player1.decaying['x'] = false;
				}
			}
		}

		if (player1.decaying['y']) {
			if (player1.ay > 0) {
				if (player1.ay > 1.34) {
					player1.ay -= 1.34;
				}
				else {
					player1.ay = 0;
				}
			}
			else {
				if (player1.ay < 0) {
					if (player1.ay < -1.34) {
						player1.ay += 1.34;
					}
					else {
						player1.ay = 0;
					}	
				}
				else {
					player1.decaying['y'] = false;
				}
			}
		}
	}

	//====
	if (activePlayer == player2) {
		if (player2.decaying['x']) {
			if (player2.ax > 0) {
				if (player2.ax > 1.34) {
					player2.ax -= 1.34;
				}
				else {
					player2.ax = 0;
				}
			}
			else {
				if (player2.ax < 0) {
					if (player2.ax < -1.34) {
						player2.ax += 1.34;
					}
					else {
						player2.ax = 0;
					}	
				}
				else {
					player2.decaying['x'] = false;
				}
			}
		}

		if (player2.decaying['y']) {
			if (player2.ay > 0) {
				if (player2.ay > 1.34) {
					player2.ay -= 1.34;
				}
				else {
					player2.ay = 0;
				}
			}
			else {
				if (player2.ay < 0) {
					if (player2.ay < -1.34) {
						player2.ay += 1.34;
					}
					else {
						player2.ay = 0;
					}	
				}
				else {
					player2.decaying['y'] = false;
				}
			}
		}
	}
}

function checkBulletsPlayers() {
	for (var blt in bullets) {
		var ang = Math.atan(bullets[blt]["vy"]/bullets[blt]["vx"]);
		var bulletVertices = [
			rotatePoint(bullets[blt]["x"], bullets[blt]["y"], bullets[blt]["x"] + 5, bullets[blt]["y"] + 3, ang),
			rotatePoint(bullets[blt]["x"] + 10, bullets[blt]["y"], bullets[blt]["x"] + 5, bullets[blt]["y"] + 3, ang),
			rotatePoint(bullets[blt]["x"] + 10, bullets[blt]["y"] + 6, bullets[blt]["x"] + 5, bullets[blt]["y"] + 3, ang),
			rotatePoint(bullets[blt]["x"], bullets[blt]["y"] + 6, bullets[blt]["x"] + 5, bullets[blt]["y"] + 3, ang)
		];
		
		if (bullets[blt].p == 2) {
			if (doPolygonsIntersect(bulletVertices,[{x:player1.x,y:player1.y},{x:player1.x + player1.getWidth(),y:player1.y},{x:player1.x + player1.getWidth(),y:player1.y + player1.getHeight()},{x:player1.x,y:player1.y + player1.getHeight()}])) {
				win("Player 2");
				return true;
			}
		}

		if (bullets[blt].p == 1) {
			if (doPolygonsIntersect(bulletVertices,[{x:player2.x,y:player2.y},{x:player2.x + player2.getWidth(),y:player2.y},{x:player2.x + player2.getWidth(),y:player2.y + player2.getHeight()},{x:player2.x,y:player2.y + player2.getHeight()}])) {
				win("Player 1");
				return true;
			}
		}
	}
}