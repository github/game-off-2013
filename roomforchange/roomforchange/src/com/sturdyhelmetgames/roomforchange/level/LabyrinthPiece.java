package com.sturdyhelmetgames.roomforchange.level;

import com.sturdyhelmetgames.roomforchange.level.Level.LevelTile;
import com.sturdyhelmetgames.roomforchange.level.Level.LevelTileType;

public class LabyrinthPiece {

	public static final int WIDTH = 12;
	public static final int HEIGHT = 8;
	private final LevelTile[][] tiles;

	public LevelTile[][] getTiles() {
		return tiles;
	}

	public LabyrinthPiece(PieceTemplate pieceTemplate,
			RoomObjectTemplate roomObjectTemplate) {
		tiles = new LevelTile[WIDTH][HEIGHT];

		final LevelTileType[][] pcs = pieceTemplate.getTileTypes();
		for (int x = 0; x < PieceTemplate.WIDTH; x++) {
			for (int y = 0; y < PieceTemplate.HEIGHT; y++) {
				tiles[x][y] = new LevelTile(pcs[x][y]);
			}
		}

		// TODO roomObjectTemplate handling

	}
}
