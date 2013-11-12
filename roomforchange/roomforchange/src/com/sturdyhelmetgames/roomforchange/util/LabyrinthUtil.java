package com.sturdyhelmetgames.roomforchange.util;

import com.sturdyhelmetgames.roomforchange.assets.Assets;
import com.sturdyhelmetgames.roomforchange.entity.Player;
import com.sturdyhelmetgames.roomforchange.level.LabyrinthPiece;
import com.sturdyhelmetgames.roomforchange.level.Level;
import com.sturdyhelmetgames.roomforchange.level.Level.LevelTile;
import com.sturdyhelmetgames.roomforchange.level.PieceTemplate;

public class LabyrinthUtil {

	public static Level generateLabyrinth(int width, int height) {
		final Level level = new Level();
		level.setLabyrinth(new LabyrinthPiece[width][height]);
		level.setTiles(new LevelTile[width * PieceTemplate.WIDTH][height
				* PieceTemplate.HEIGHT]);

		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				level.getLabyrinth()[x][y] = new LabyrinthPiece(
						Assets.getRandomPieceTemplate(), null);
			}
		}

		updateLabyrinthTiles(level);

		level.player = new Player(7, 5, level);
		level.entities.add(level.player);

		return level;
	}

	public static void updateLabyrinthTiles(Level level) {

		final LabyrinthPiece[][] labyrinth = level.getLabyrinth();
		final int labyrinthWidth = labyrinth.length;
		for (int x = 0; x < labyrinthWidth; x++) {
			final int labyrinthHeight = labyrinth[0].length;
			for (int y = 0; y < labyrinthHeight; y++) {
				LevelTile[][] labyrinthTiles = labyrinth[x][y].getTiles();
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
}
