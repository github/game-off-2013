package com.sturdyhelmetgames.roomforchange.level;

import com.badlogic.gdx.math.Rectangle;
import com.sturdyhelmetgames.roomforchange.level.Level.LevelTile;
import com.sturdyhelmetgames.roomforchange.level.Level.LevelTileType;

public class LabyrinthPiece {

	public static final int WIDTH = 12;
	public static final int HEIGHT = 8;
	private final LevelTile[][] tiles;
	private final Rectangle bounds = new Rectangle();
	private final int orderNumber;

	public LevelTile[][] getTiles() {
		return tiles;
	}

	public Rectangle getBounds() {
		return bounds;
	}

	public LabyrinthPiece(PieceTemplate pieceTemplate,
			RoomObjectTemplate roomObjectTemplate, int orderNumber) {
		tiles = new LevelTile[WIDTH][HEIGHT];
		this.orderNumber = orderNumber;

		final LevelTileType[][] pcs = pieceTemplate.getTileTypes();
		for (int x = 0; x < PieceTemplate.WIDTH; x++) {
			for (int y = 0; y < PieceTemplate.HEIGHT; y++) {
				tiles[x][y] = new LevelTile(pcs[x][y]);
			}
		}

		// TODO roomObjectTemplate handling

	}

	public void updateBounds(float x, float y) {
		bounds.set(x, y, WIDTH, HEIGHT);
	}

	@Override
	public String toString() {
		return "LabyrinthPiece " + orderNumber;
	}

}
