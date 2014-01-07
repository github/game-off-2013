/*    Copyright 2013 Antti Kolehmainen

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License. */
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
				if (pixel == Color.BLUE) {
					tileTypes[x][y] = LevelTileType.WALL_CORNER;
				} else if (pixel == Color.BLACK) {
					tileTypes[x][y] = LevelTileType.WALL_FRONT;
				} else if (pixel == Color.GREEN) {
					tileTypes[x][y] = LevelTileType.WALL_RIGHT;
				} else if (pixel == Color.RED) {
					tileTypes[x][y] = LevelTileType.WALL_LEFT;
				} else if (pixel == Color.YELLOW) {
					tileTypes[x][y] = LevelTileType.WALL_BACK;
				} else {
					tileTypes[x][y] = LevelTileType.GROUND;
				}
				yFlip++;
			}
			yFlip = 0;
		}
	}

}
