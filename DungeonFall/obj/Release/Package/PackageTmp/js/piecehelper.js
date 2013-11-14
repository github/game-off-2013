function PieceHelper() {};
PieceHelper.rotateRight = function (tiles) {
    var transformedArray = new Array();
	
    var row = -1;
    for (var i = tiles[0].length - 1; i > -1; i--)
    {
        row++;
        transformedArray[row] = new Array();
		
        for (var j = 0; j < tiles.length; j++)
        {
            transformedArray[row][j] = tiles[j][i];
        }
    }
	
    return transformedArray;
};
PieceHelper.rotateLeft = function (tiles) {
    var transformedArray = new Array();
		
    for ( var i = 0; i < tiles[0].length; i++ )
    {
        transformedArray[i] = new Array();
		
        // fill the row with everything in the appropriate column of the source array
        var transformedArrayColumn = -1;
        for (var j = tiles.length - 1; j > -1; j--)
        {
            transformedArrayColumn++;
            transformedArray[i][transformedArrayColumn] = tiles[j][i]
        }

    }
	
    return transformedArray;
};

    
