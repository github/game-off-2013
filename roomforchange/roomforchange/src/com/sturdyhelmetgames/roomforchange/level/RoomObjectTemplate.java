package com.sturdyhelmetgames.roomforchange.level;

import com.badlogic.gdx.graphics.Pixmap;
import com.sturdyhelmetgames.roomforchange.level.Level.LevelTileType;

public class RoomObjectTemplate {

	private static final int WIDTH = 10;
	private static final int HEIGHT = 6;
	public final LevelTileType[][] tileTypes;
	public final Pixmap pixmap;

	// TODO add entity and object types also

	public RoomObjectTemplate(Pixmap pixmap) {
		this.pixmap = pixmap;
		tileTypes = new LevelTileType[WIDTH][HEIGHT];
		if (pixmap.getWidth() > WIDTH || pixmap.getHeight() > HEIGHT) {
			throw new IllegalArgumentException(
					"Template is too big! Should be 10x6px.");
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
