package com.sturdyhelmetgames.roomforchange.util;

import java.util.ArrayList;
import java.util.List;

import org.junit.Assert;
import org.junit.Test;
import org.mockito.Mockito;

import com.sturdyhelmetgames.roomforchange.level.LabyrinthPiece;
import com.sturdyhelmetgames.roomforchange.level.Level;
import com.sturdyhelmetgames.roomforchange.level.Level.LevelTile;
import com.sturdyhelmetgames.roomforchange.level.Level.LevelTileType;

public class LabyrinthUtilTest {

	@Test
	public void testUpdateLabyrinthTiles() {
		final List<Object> mocks = new ArrayList<Object>();

		final LevelTile[][] tiles = new LevelTile[12][8];
		for (int x = 0; x < 12; x++) {
			for (int y = 0; y < 8; y++) {
				tiles[x][y] = new LevelTile(LevelTileType.GROUND);
			}
		}

		final LabyrinthPiece[][] labyrinth = new LabyrinthPiece[2][2];
		for (int x = 0; x < 2; x++) {
			for (int y = 0; y < 2; y++) {
				labyrinth[x][y] = Mockito.mock(LabyrinthPiece.class);
				Mockito.when(labyrinth[x][y].getTiles()).thenReturn(tiles);
				mocks.add(labyrinth[x][y]);
			}
		}

		Level level = Mockito.mock(Level.class);
		mocks.add(level);
		Mockito.when(level.getLabyrinth()).thenReturn(labyrinth);

		final LevelTile[][] levelTiles = new LevelTile[24][16];
		Mockito.when(level.getTiles()).thenReturn(levelTiles);

		LabyrinthUtil.updateLabyrinthTiles(level);

		for (int x = 0; x < 24; x++) {
			for (int y = 0; y < 16; y++) {
				Assert.assertNotNull(levelTiles[x][y]);
				Assert.assertEquals(LevelTileType.GROUND, levelTiles[x][y].type);
			}
		}

		for (Object mock : mocks) {
			Mockito.verify(mock);
		}
	}
}
