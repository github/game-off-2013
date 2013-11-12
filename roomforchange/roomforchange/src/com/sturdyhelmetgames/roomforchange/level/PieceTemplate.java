package com.sturdyhelmetgames.roomforchange.level;

import com.badlogic.gdx.graphics.Pixmap;
import com.sturdyhelmetgames.roomforchange.level.Level.LevelTileType;

public class PieceTemplate {

	public static final int WIDTH = 12;
	public static final int HEIGHT = 8;
	private final Pixmap pixmap;
	private final LevelTileType[][] tileTypes;

	public Pixmap getPixmap() {
		return pixmap;
	}

	public LevelTileType[][] getTileTypes() {
		return tileTypes;
	}

	public PieceTemplate(Pixmap pixmap) {
		this.pixmap = pixmap;
		tileTypes = new LevelTileType[WIDTH][HEIGHT];
		if (pixmap.getWidth() > WIDTH || pixmap.getHeight() > HEIGHT) {
			throw new IllegalArgumentException(
					"Template is too big! Should be 12x8px.");
		}
		for (int x = 0; x < pixmap.getWidth(); x++) {
			for (int y = 0; y < pixmap.getHeight(); y++) {
				int pixel = pixmap.getPixel(x, y);
				if (pixel == 255) {
					tileTypes[x][y] = LevelTileType.WALL;
				} else {
					tileTypes[x][y] = LevelTileType.GROUND;
				}
			}
		}
	}

}
