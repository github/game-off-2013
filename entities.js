function mainCharacter(x,y,context,pno) {
	this.x = x;
	this.y = y;
	var maxInc = 35;
	this.angle = 0;
	var w = 30;
	var h = 30;
	this.ax = 0;
	this.ay = 0;
	this.pno = pno;
	this.decaying = {
		x: false,
		y: false
	};

	this.spriteFrames = [];
	if (pno == 1) {
		this.frame1 = new Image();
		this.spriteFrames.push(this.frame1);
		this.frame1.src = 'sprites/character1.png';
	
		this.frame2 = new Image();
		this.spriteFrames.push(this.frame2);
		this.frame2.src = 'sprites/character2.png';
		
		this.frame3 = new Image();
		this.spriteFrames.push(this.frame3);
		this.frame3.src = 'sprites/character3.png';
		
		this.frame4 = new Image();
		this.spriteFrames.push(this.frame4);
		this.frame4.src = 'sprites/character4.png';
	} 
	else {
		this.frame1 = new Image();
		this.spriteFrames.push(this.frame1);
		this.frame1.src = 'sprites/character1_2.png';
	
		this.frame2 = new Image();
		this.spriteFrames.push(this.frame2);
		this.frame2.src = 'sprites/character2_2.png';
		
		this.frame3 = new Image();
		this.spriteFrames.push(this.frame3);
		this.frame3.src = 'sprites/character3_2.png';
		
		this.frame4 = new Image();
		this.spriteFrames.push(this.frame4);
		this.frame4.src = 'sprites/character4_2.png';
	}
	
	this.activeFrame = 0;

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
			this.angle = Math.atan((mPos.y  - this.y - 33)/(mPos.x - this.x - 26)) - (mPos.x - this.x > 0 ? 1.571 : -1.571);
		}
		
		// tDelta = Math.sqrt((this.ay) * (this.ay) + (this.ax) * (this.ax))/5;
		if (this.angle !== 0) {
			this.ctx.save();
			this.ctx.translate(this.x + w/2,this.y + h/2);
			this.ctx.rotate(this.angle);
			this.ctx.drawImage(this.spriteFrames[this.activeFrame],-w/2,-h/2);
			this.ctx.restore();
		}
		else {
			this.ctx.drawImage(this.spriteFrames[this.activeFrame],Math.round(this.x),Math.round(this.y));
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

function playGame() {
	$('#title').hide();
	$('#desc').hide();
	$('#txtbuttons').html("<div onclick='startP1Turn()'>Start Player 1's Turn (Player 2 Look away!)</div>");
}

function startP1Turn() {
	record(3,player1);
	$('#txtbuttons').html("<div>Time left: </div>");
	$('#loader').show();
	$('#loaderinner').show();
	document.getElementById('loaderinner').style.width = 0;

}

function startP2Turn() {
	record(3,player2);
	$('#txtbuttons').html("<div>Time left: </div>");
	$('#loader').show();
	$('#loaderinner').show();
	document.getElementById('loaderinner').style.width = 0;
}

function record(rt, player) { //t: Number of seconds the record lasts for, player = player object htat you're recording
	window.recording = 1;
	activePlayer = player;
	activePlayer.ax = 0;
	activePlayer.ay = 0;
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
			vy:bullets[abullet].vy,
			p:bullets[abullet].p
		});
	}

	window.recordTime = rt * 60;
	window.recordingData = {
		t:rt,
		movement: [],
		rotation: [],
		bullets: []
	};

	cte = 0;
	window.cIndex = 0;
	tDelta = 1;
}

function recordFrame() {
	if (recording == 1) {
		if (recordTime > cte) {
			document.getElementById('loaderinner').style.width = cte;
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

				window.player2RecordedData = {};
				$.extend(player2RecordedData,recordingData);
				recordingData = {};
			}
			bullets = [];
			for (var abullet in activePlayer.oldPosition.b) {
				window.bullets.push({
					x:activePlayer.oldPosition.b[abullet].x,
					y:activePlayer.oldPosition.b[abullet].y,
					vx:activePlayer.oldPosition.b[abullet].vx,
					vy:activePlayer.oldPosition.b[abullet].vy,
					p:activePlayer.oldPosition.b[abullet].p
				});
			}

			tDelta = 0;
			recording = 0;

			if (activePlayer == player1) {
				$('#txtbuttons').html("<div onclick='startP2Turn()'>Start Player 2's Turn (Player 1 Look away!)</div>");
				$('#loader').hide();
				$('#loaderinner').hide();
			}
			else {
				$('#txtbuttons').html("<div onclick='playBack()'>Play concurrent turns!</div>");
				$('#loader').hide();
				$('#loaderinner').hide();
			}
			activePlayer = false;
		}
	}
}

function playBack(player) {
	window.playingBack = 1;
	window.cIndex = 0;
}

function playBackFrame() {
	if (playingBack == 1) {
		if (cIndex < player1RecordedData.movement.length) {
			tDelta = 1;
			player1.y = player1RecordedData.movement[cIndex][1];
			player1.x = player1RecordedData.movement[cIndex][0];
			player1.angle = player1RecordedData.rotation[cIndex];

			player2.y = player2RecordedData.movement[cIndex][1];
			player2.x = player2RecordedData.movement[cIndex][0];
			player2.angle = player2RecordedData.rotation[cIndex];

			if (player1RecordedData.bullets[cIndex]) {
				bullets.push(player1RecordedData.bullets[cIndex]);
			}

			if (player2RecordedData.bullets[cIndex]) {
				bullets.push(player2RecordedData.bullets[cIndex]);
			}
			cIndex++;
		}
		else {
			playingBack = 0;
			tDelta = 0;
			$('#txtbuttons').html("<div onclick='startP1Turn()'>Start Player 1's Turn (Player 2 Look away!)</div>");
		}
	}
}

function setFiring(plyr) {
	if (firing && recording == 1) {
		if (!(ct % 8)) {
			var absAng = Math.abs(plyr.angle);
			var xmod = 26 + 2 * absAng;
			var ymod = 33 + 2 * absAng;
			var rPoints = rotatePoint(plyr.x + xmod, plyr.y + ymod, plyr.x + 16, plyr.y + 16, plyr.angle);
			var diffX = mPos['x'] - rPoints.x;
			var diffY = mPos['y'] - rPoints.y;

			var normX = diffX/Math.sqrt(diffX * diffX + diffY * diffY);
			var normY = diffY/Math.sqrt(diffX * diffX + diffY * diffY);

			bullets.push({x:rPoints.x,y:rPoints.y,vx:normX * 20,vy:normY * 20,p:plyr.pno});
			if (recording == 1) {
				recordingData.bullets[cte] = ({x:rPoints.x,y:rPoints.y,vx:normX * 20,vy:normY * 20,p:activePlayer.pno});
			}
		}

		ct++;
	}
}

function win(qui) {
	stopped = 1;
	ctx.clearRect(0,0,1024,640);
	$('#c').css('background-color','#0C1713');
	$('#c').css('background-image','none');

	$("#title").html("<center>" + qui + " wins!</center>");
	$("#title").css("width","1024px");
	$("#title").css("left","30px");
	$('#title').show();
	$('#title').css('top','50px');
	$('#loader').hide();
	$('#loaderinner').hide();

	$("#desc").html("<center>Reload the page to play again! :)</center>");
	$('#desc').show();

	$("#btns").html('');
}