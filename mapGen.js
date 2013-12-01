function setMap(JSONData) {
	var w = 32;
	var h = 32;
	
	var mw = JSONData['layers'][0]['width'];
	var mh = JSONData['layers'][0]['height'];
	var walls = [];
	var gen = [];
	for (var hI = 0; hI < JSONData['layers'][0]['height']; hI++) {
		for (var wI = 0; wI < JSONData['layers'][0]['width']; wI++) {
			if (JSONData['layers'][0]['data'][hI * mw + wI] !== 0) {
				walls.push([
					{
						x:wI * w,
						y:hI * h
					},
					{
						x:wI * w + w,
						y:hI * h
					},
					{
						x:wI * w + w,
						y:hI * h + h
					},
					{
						x:wI * w,
						y:hI * h + h
					}
				]);
				gen.push({
					x:wI * w,
					y:hI * h
				});
			}
		}
	}

	return {vertices:walls,mapgen:gen,w:w,h:h};
}

function drawMap(d,c) {
	for (var wall in d['mapgen']) {
		c.fillStyle = 'rgb(150,200,200)';
		c.fillRect(d['mapgen'][wall]['x'],d['mapgen'][wall]['y'],d['w'],d['h']);
	}
}