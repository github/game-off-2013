package com.sturdyhelmetgames.roomforchange.util;

import com.sturdyhelmetgames.roomforchange.assets.Assets;
import com.sturdyhelmetgames.roomforchange.entity.Mummy;
import com.sturdyhelmetgames.roomforchange.entity.Player;
import com.sturdyhelmetgames.roomforchange.level.LabyrinthPiece;
import com.sturdyhelmetgames.roomforchange.level.Level;
import com.sturdyhelmetgames.roomforchange.level.Level.LevelTile;
import com.sturdyhelmetgames.roomforchange.level.Level.LevelTileType;
import com.sturdyhelmetgames.roomforchange.level.PieceTemplate;
import com.sturdyhelmetgames.roomforchange.screen.GameScreen;

public class LabyrinthUtil {

	public static Level generateLabyrinth(int width, int height,
			GameScreen gameScreen) {
		final Level level = new Level(gameScreen);
		level.setLabyrinth(new LabyrinthPiece[width][height]);
		level.setTiles(new LevelTile[width * PieceTemplate.WIDTH][height
				* PieceTemplate.HEIGHT]);

		int i = 0;
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				level.getLabyrinth()[x][y] = new LabyrinthPiece(
						Assets.getRandomPieceTemplate(), null, i);
				i++;
			}
		}

		updateLabyrinthTiles(level);

		level.player = new Player(2, 2, level);
		level.entities.add(level.player);

		level.entities.add(new Mummy(10f, 5f, level));

		return level;
	}

	public static void updateLabyrinthTiles(Level level) {

		final LabyrinthPiece[][] labyrinth = level.getLabyrinth();
		final int labyrinthWidth = labyrinth.length;
		for (int x = 0; x < labyrinthWidth; x++) {
			final int labyrinthHeight = labyrinth[0].length;
			for (int y = 0; y < labyrinthHeight; y++) {
				LabyrinthPiece labyrinthPiece = labyrinth[x][y];
				labyrinthPiece.updateBounds(x, y);
				setAllFourDoorsClosedIfNeeded(labyrinth, x, y, labyrinthPiece);
				LevelTile[][] labyrinthTiles = labyrinthPiece.getTiles();
				int labyrinthTilesWidth = labyrinthTiles.length;
				for (int tileX = 0; tileX < labyrinthTilesWidth; tileX++) {
					int labyrinthTilesHeight = labyrinthTiles[0].length;
					for (int tileY = 0; tileY < labyrinthTilesHeight; tileY++) {
						final LevelTile tile = labyrinthTiles[tileX][tileY];
						final int labyrinthFullTileSetWidth = tileX
								+ (LabyrinthPiece.WIDTH * x);
						final int labyrinthFullTilesetHeight = tileY
								+ (LabyrinthPiece.HEIGHT * y);
						level.getTiles()[labyrinthFullTileSetWidth][labyrinthFullTilesetHeight] = tile;
					}
				}
			}
		}

	}

	private static void setAllFourDoorsClosedIfNeeded(
			final LabyrinthPiece[][] labyrinth, int x, int y,
			LabyrinthPiece piece) {

		LabyrinthPiece labyrinthPiece = null;
		try {
			labyrinthPiece = labyrinth[x][y + 1];
		} catch (ArrayIndexOutOfBoundsException e) {

		}
		LabyrinthPiece labyrinthPiece2 = null;
		try {
			labyrinthPiece2 = labyrinth[x + 1][y];
		} catch (ArrayIndexOutOfBoundsException e) {

		}
		LabyrinthPiece labyrinthPiece3 = null;
		try {
			labyrinthPiece3 = labyrinth[x][y - 1];
		} catch (ArrayIndexOutOfBoundsException e) {

		}

		LabyrinthPiece labyrinthPiece4 = null;
		try {
			labyrinthPiece4 = labyrinth[x - 1][y];
		} catch (ArrayIndexOutOfBoundsException e) {

		}

		setDoorClosed(piece, new LabyrinthPiece[] { labyrinthPiece,
				labyrinthPiece2, labyrinthPiece3, labyrinthPiece4 });
	}

	private static void setDoorClosed(LabyrinthPiece piece,
			LabyrinthPiece[] surroundingPieces) {
		final LevelTile[][] tiles = piece.getTiles();

		// up piece
		LabyrinthPiece surroundingPiece = surroundingPieces[0];
		if ((surroundingPiece != null && !surroundingPiece.doorsOpen[2] && piece.doorsOpen[0])
				|| surroundingPiece == null && piece.doorsOpen[0]) {
			tiles[5][7] = new LevelTile(LevelTileType.DOOR);
			tiles[6][7] = new LevelTile(LevelTileType.DOOR);
		} else {
			tiles[5][7] = tiles[5][7].type != LevelTileType.WALL ? new LevelTile(
					LevelTileType.GROUND) : tiles[5][7];
			tiles[6][7] = tiles[6][7].type != LevelTileType.WALL ? new LevelTile(
					LevelTileType.GROUND) : tiles[6][7];
		}
		// right piece
		surroundingPiece = surroundingPieces[1];
		if ((surroundingPiece != null && !surroundingPiece.doorsOpen[3] && piece.doorsOpen[1])
				|| surroundingPiece == null && piece.doorsOpen[1]) {
			tiles[11][3] = new LevelTile(LevelTileType.DOOR);
			tiles[11][4] = new LevelTile(LevelTileType.DOOR);
		} else {
			tiles[11][3] = tiles[11][3].type != LevelTileType.WALL ? new LevelTile(
					LevelTileType.GROUND) : tiles[11][3];
			tiles[11][4] = tiles[11][4].type != LevelTileType.WALL ? new LevelTile(
					LevelTileType.GROUND) : tiles[11][4];
		}

		// down piece
		surroundingPiece = surroundingPieces[2];
		if ((surroundingPiece != null && !surroundingPiece.doorsOpen[0] && piece.doorsOpen[2])
				|| surroundingPiece == null && piece.doorsOpen[2]) {
			tiles[5][0] = new LevelTile(LevelTileType.DOOR);
			tiles[6][0] = new LevelTile(LevelTileType.DOOR);
		} else {
			tiles[5][0] = tiles[5][0].type != LevelTileType.WALL ? new LevelTile(
					LevelTileType.GROUND) : tiles[5][0];
			tiles[6][0] = tiles[6][0].type != LevelTileType.WALL ? new LevelTile(
					LevelTileType.GROUND) : tiles[6][0];
		}

		// left piece
		surroundingPiece = surroundingPieces[3];
		if ((surroundingPiece != null && !surroundingPiece.doorsOpen[1] && piece.doorsOpen[3])
				|| surroundingPiece == null && piece.doorsOpen[3]) {
			tiles[0][3] = new LevelTile(LevelTileType.DOOR);
			tiles[0][4] = new LevelTile(LevelTileType.DOOR);
		} else {
			tiles[0][3] = tiles[0][3].type != LevelTileType.WALL ? new LevelTile(
					LevelTileType.GROUND) : tiles[0][3];
			tiles[0][4] = tiles[0][4].type != LevelTileType.WALL ? new LevelTile(
					LevelTileType.GROUND) : tiles[0][4];
		}

	}
}
