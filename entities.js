function mainCharacter(x,y,context) {
	this.x = x;
	this.y = y;
	var maxInc = 35;
	this.angle = 0;
	var w = 30;
	var h = 30;
	this.ax = 0;
	this.ay = 0;
	this.decaying = {
		x: false,
		y: false
	}

	this.getWidth = function() {
		return w;
	};

	this.getHeight = function() {
		return h;
	};

	this.ctx = context;
	this.handlers = [];

	this.draw = function () {
		this.ctx.fillStyle = 'rgb(0,0,200)';
		if (activePlayer == this && playingBack == 0 && recording == 1) {
			var nextVertices = [{x:this.x + this.ax,y:this.y + this.ay},{x:this.x + this.ax + w,y:this.y + this.ay},{x:this.x + this.ax + w,y:this.y + this.ay + h},{x:this.x + this.ax,y:this.y + this.ay + h}];
			for (var wall in mapData['vertices']) {
				if (doPolygonsIntersect(mapData['vertices'][wall],nextVertices)) {
					var next = 0;
					var next2 = 0;
					var intersected = [[false,false,false,false],[false,false,false,false],[false,false,false,false],[false,false,false,false]];
					var intersected2 = [[false,false,false,false],[false,false,false,false],[false,false,false,false],[false,false,false,false]];
					for (var seg = 0; seg < mapData['vertices'][wall].length; seg++) {
						if (seg == 3) {
							next = 0;
						}
						else {
							next = seg + 1;
						}
						for (var seg2 = 0; seg2 < nextVertices.length; seg2++) {
							if (seg2 == 3) {
								next2 = 0;
							}
							else {
								next2 = seg2 + 1;
							}
							if (intersect(nextVertices[seg2].x,nextVertices[seg2].y,nextVertices[next2].x,nextVertices[next2].y,mapData['vertices'][wall][seg].x,mapData['vertices'][wall][seg].y,mapData['vertices'][wall][next].x,mapData['vertices'][wall][next].y)) {
								intersected[seg][next] = true;
								intersected2[seg2][next2] = true;
							}
						}
					}
					
				}
	
				for (var dd in intersected) {
					// debugger;
					if (intersected[0][1]) { //top edge +
						this.ay = 0;
					}
					else { //bottom edge
						if (intersected[2][3]) {
							this.ay = 0;
						}
					}
	
					if (intersected[3][0]) { //left edge
						this.ax = 0;
					}
					else { //right edge
						if (intersected[1][2]) {
							this.ax = 0;
						}
					}
				}
	
				//tl, tr, br, bl
			}
			this.y += this.ay;
			this.x += this.ax;
		}
		// tDelta = Math.sqrt((this.ay) * (this.ay) + (this.ax) * (this.ax))/5;
		if (this.angle !== 0) {
			this.ctx.save();
			this.ctx.translate(this.x + w/2,this.y + h/2);
			this.ctx.rotate(this.angle);
			this.ctx.fillRect(-w/2,-h/2,w,h);
			this.ctx.restore();
		}
		else {
			this.ctx.fillRect(Math.round(this.x),Math.round(this.y),w,h);
		}
	};

	this.keyHandlers = function() {
		if (keys[87]) { //w
			if (this.decaying['y']) {
				this.decaying['y'] = false;
			}

			if (this.ay > -4) {
				var inc = this.ay > 0 ? 1.5 : .6;
				if (keys[65] || keys[68]) {
					inc = inc/1.414;
				}
				this.ay = this.ay - inc;
			}
			else {
				this.ay = -4;
			}
		}

		if (keys[65]) { //a
			if (this.decaying['x']) {
				this.decaying['x'] = false;
			}

			if (this.ax > -4) {
				var inc = this.ax > 0 ? 1.5 : .6;
				if (keys[87] || keys[83]) {
					inc = inc/1.414;
				}
				this.ax = this.ax - inc;
			}
			else {
				this.ax = -4;
			}
		}

		if (keys[83]) { //s
			if (this.decaying['y']) {
				this.decaying['y'] = false;
			}
			if (this.ay < 4) {
				var inc = this.ay < 0 ? 1.5 : .6;
				if (keys[65] || keys[68]) {
					inc = inc/1.414;
				}
				this.ay = this.ay + inc;
			}
			else {
				this.ay = 4;
			}
		}

		if (keys[68]) { //d
			if (this.decaying['x']) {
				this.decaying['x'] = false;
			}
			if (this.ax < 4) {
				var inc = this.ax < 0 ? 1.5 : .6;
				if (keys[87] || keys[83]) {
					inc = inc/1.414;
				}
				this.ax = this.ax + inc;
			}
			else {
				this.ax = 4;
			}
		}
	};

	this.oldPosition = {};
}

function record(rt, player) { //t: Number of seconds the record lasts for, player = player object htat you're recording
	window.recording = 1;
	activePlayer = player;
	activePlayer.oldPosition = {
		x:player.x,
		y:player.y,
		a:player.angle,
		b:[]
	};
	for (var abullet in bullets) {
		activePlayer.oldPosition.b.push({
			x:bullets[abullet].x,
			y:bullets[abullet].y,
			vx:bullets[abullet].vx,
			vy:bullets[abullet].vy
		});
		console.log(bullets[abullet].x,bullets[abullet].y);
	}
	console.log('Recording started');
	window.startTime = new Date().getTime();
	window.recordTime = rt;
	window.recordingData = {
		t:rt,
		movement: [],
		rotation: [],
		bullets: []
	};

	cte = 0;
	tDelta = 1;
}

function recordFrame() {
	if (recording == 1) {
		if (startTime + recordTime * 1000 > new Date().getTime()) {
			recordingData.movement.push([activePlayer.x,activePlayer.y]);
			recordingData.rotation.push(activePlayer.angle);
		}
		else {
			if (activePlayer == player1) {
				player1.x = activePlayer.oldPosition.x;
				player1.y = activePlayer.oldPosition.y;
				player1.angle = activePlayer.oldPosition.a;

				window.player1RecordedData = {};
				$.extend(player1RecordedData,recordingData);
				recordingData = {};
			}
			else {
				player2.x = activePlayer.oldPosition.x;
				player2.y = activePlayer.oldPosition.y;
				player2.angle = activePlayer.oldPosition.a;

				window.player1RecordedData = {};
				$.extend(player1RecordedData,recordingData);
				recordingData = {};
			}
			bullets = [];
			for (var abullet in activePlayer.oldPosition.b) {
				window.bullets.push({
					x:activePlayer.oldPosition.b[abullet].x,
					y:activePlayer.oldPosition.b[abullet].y,
					vx:activePlayer.oldPosition.b[abullet].vx,
					vy:activePlayer.oldPosition.b[abullet].vy
				});
			}

			tDelta = 0;
			activePlayer = false;
			recording = 0;

			if (activePlayer == player1) {
				
			}
			else {
				
			}
		}
	}
}

function playBack(player) {
	window.playingBack = 1;
	window.cIndex = 0;
	window.activePlayer = player;
}

function playBackFrame() {
	if (playingBack == 1) {
		if (cIndex < recordingData.movement.length) {
			tDelta = 1;
			activePlayer.y = recordingData.movement[cIndex][1];
			activePlayer.x = recordingData.movement[cIndex][0];
			if (recordingData.bullets[cIndex]) {
				bullets.push(recordingData.bullets[cIndex]);
			}
			cIndex++;
		}
		else {
			playingBack = 0;
			tDelta = 0;
			activePlayer = false;
		}
	}
}

function setFiring(plyr) {
	if (firing) {
		if (!(ct % 8)) {
			var diffX = mPos['x'] - plyr.x;
			var diffY = mPos['y'] - plyr.y;

			var normX = diffX/Math.sqrt(diffX * diffX + diffY * diffY);
			var normY = diffY/Math.sqrt(diffX * diffX + diffY * diffY);

			bullets.push({x:plyr.x + 15,y:plyr.y + 15,vx:normX * 20,vy:normY * 20});
			if (recording == 1) {
				recordingData.bullets[cte] = ({x:plyr.x + 15,y:plyr.y + 15,vx:normX * 20,vy:normY * 20});
			}
		}

		ct++;
	}
}