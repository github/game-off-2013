package com.sturdyhelmetgames.roomforchange.level;

import com.badlogic.gdx.graphics.Pixmap;
import com.sturdyhelmetgames.roomforchange.level.Level.LevelTileType;

public class RoomTemplate {

	public static final int WIDTH = 10;
	public static final int HEIGHT = 6;
	private final LevelTileType[][] tileTypes;
	private final Pixmap pixmap;

	public Pixmap getPixmap() {
		return pixmap;
	}

	public LevelTileType[][] getTileTypes() {
		return tileTypes;
	}

	// TODO add entity and object types also

	public RoomTemplate(Pixmap pixmap) {
		this.pixmap = pixmap;
		tileTypes = new LevelTileType[WIDTH][HEIGHT];
		if (pixmap.getWidth() > WIDTH || pixmap.getHeight() > HEIGHT) {
			throw new IllegalArgumentException(
					"Template is too big! Should be 10x6px.");
		}
		for (int x = 0; x < pixmap.getWidth(); x++) {
			for (int y = 0; y < pixmap.getHeight(); y++) {
				int pixel = pixmap.getPixel(x, y);
				if (pixel == Color.BLACK) {
					tileTypes[x][y] = LevelTileType.WALL_FRONT;
				} else if (pixel == Color.RED) {
					tileTypes[x][y] = LevelTileType.HOLE;
				} else if (pixel == Color.BLUE) {
					tileTypes[x][y] = LevelTileType.ROCK;
				} else {
					tileTypes[x][y] = LevelTileType.GROUND;
				}
			}
		}
	}

}
