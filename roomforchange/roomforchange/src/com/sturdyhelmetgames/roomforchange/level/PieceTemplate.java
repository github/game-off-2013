package com.sturdyhelmetgames.roomforchange.level;

import com.badlogic.gdx.graphics.Pixmap;
import com.sturdyhelmetgames.roomforchange.level.Level.LevelTileType;

public class PieceTemplate {

	public static final int WIDTH = 12;
	public static final int HEIGHT = 8;
	private final Pixmap pixmap;
	private final LevelTileType[][] tileTypes;
	public final boolean[] doorsOpen = new boolean[4];

	public Pixmap getPixmap() {
		return pixmap;
	}

	public LevelTileType[][] getTileTypes() {
		return tileTypes;
	}

	public PieceTemplate(Pixmap pixmap, boolean[] doorsOpen) {
		this.pixmap = pixmap;

		for (int i = 0; i < doorsOpen.length; i++) {
			this.doorsOpen[i] = doorsOpen[i];
		}

		tileTypes = new LevelTileType[WIDTH][HEIGHT];
		if (pixmap.getWidth() > WIDTH || pixmap.getHeight() > HEIGHT) {
			throw new IllegalArgumentException(
					"Template is too big! Should be 12x8px.");
		}

		int yFlip = 0;
		for (int x = 0; x < pixmap.getWidth(); x++) {
			for (int y = pixmap.getHeight() - 1; y >= 0; y--) {
				int pixel = pixmap.getPixel(x, yFlip);
				if (pixel == 255) {
					tileTypes[x][y] = LevelTileType.WALL;
				} else {
					tileTypes[x][y] = LevelTileType.GROUND;
				}
				yFlip++;
			}
			yFlip = 0;
		}
	}

}
