package com.sturdyhelmetgames.roomforchange.util;

import org.junit.Assert;
import org.junit.Test;
import org.junit.runner.RunWith;
import org.mockito.Mockito;
import org.mockito.invocation.InvocationOnMock;
import org.mockito.stubbing.Answer;
import org.powermock.api.mockito.PowerMockito;
import org.powermock.core.classloader.annotations.PrepareForTest;
import org.powermock.modules.junit4.PowerMockRunner;

import com.sturdyhelmetgames.roomforchange.assets.Assets;
import com.sturdyhelmetgames.roomforchange.level.LabyrinthPiece;
import com.sturdyhelmetgames.roomforchange.level.Level;
import com.sturdyhelmetgames.roomforchange.level.Level.LevelTile;
import com.sturdyhelmetgames.roomforchange.level.Level.LevelTileType;
import com.sturdyhelmetgames.roomforchange.level.PieceTemplate;
import com.sturdyhelmetgames.roomforchange.screen.GameScreen;

@RunWith(PowerMockRunner.class)
@PrepareForTest(Assets.class)
public class LabyrinthUtilTest {

	@Test
	public void testUpdateLabyrinthTiles() {

		final LevelTile[][] tiles = new LevelTile[12][8];
		for (int x = 0; x < 12; x++) {
			for (int y = 0; y < 8; y++) {
				tiles[x][y] = new LevelTile(null, LevelTileType.GROUND);
			}
		}

		final LabyrinthPiece[][] labyrinth = new LabyrinthPiece[2][2];
		for (int x = 0; x < 2; x++) {
			for (int y = 0; y < 2; y++) {
				labyrinth[x][y] = Mockito.mock(LabyrinthPiece.class);
				Mockito.when(labyrinth[x][y].getTiles()).thenReturn(tiles);
			}
		}

		Level level = Mockito.mock(Level.class);
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

		Mockito.verify(level);
	}

	public PieceTemplate mockPieceTemplate(int i) {
		LevelTileType[][] tiles = new LevelTileType[12][8];
		for (int x = 0; x < 12; x++) {
			for (int y = 0; y < 8; y++) {
				tiles[x][y] = LevelTileType.GROUND;
			}
		}

		final PieceTemplate pieceTemplate = Mockito.mock(PieceTemplate.class);
		Mockito.when(pieceTemplate.getTileTypes()).thenReturn(tiles);
		return pieceTemplate;
	}

	private int i = 0;

	@Test
	public void testMoveLabyrinthPieceUp() {

		PowerMockito.mockStatic(Assets.class);
		Mockito.when(Assets.getRandomPieceTemplate()).then(
				new Answer<PieceTemplate>() {
					@Override
					public PieceTemplate answer(InvocationOnMock invocation)
							throws Throwable {
						i++;
						return mockPieceTemplate(i);
					}
				});

		GameScreen gameScreenMock = Mockito.mock(GameScreen.class);
		final Level level = LabyrinthUtil.generateLabyrinth(6, 6,
				gameScreenMock);
		level.moveLabyrinthPiece(Level.UP);
		LabyrinthUtil.updateLabyrinthTiles(level);

		Assert.assertEquals("LabyrinthPiece " + 5,
				level.getLabyrinth()[0][0].toString());
		Assert.assertEquals("LabyrinthPiece " + 0,
				level.getLabyrinth()[0][1].toString());
		Assert.assertEquals("LabyrinthPiece " + 1,
				level.getLabyrinth()[0][2].toString());
		Assert.assertEquals("LabyrinthPiece " + 2,
				level.getLabyrinth()[0][3].toString());
		Assert.assertEquals("LabyrinthPiece " + 3,
				level.getLabyrinth()[0][4].toString());
		Assert.assertEquals("LabyrinthPiece " + 4,
				level.getLabyrinth()[0][5].toString());

		PowerMockito.verifyStatic();
	}

	@Test
	public void testMoveLabyrinthPieceDown() throws Exception {

		PowerMockito.mockStatic(Assets.class);
		Mockito.when(Assets.getRandomPieceTemplate()).then(
				new Answer<PieceTemplate>() {
					@Override
					public PieceTemplate answer(InvocationOnMock invocation)
							throws Throwable {
						i++;
						return mockPieceTemplate(i);
					}
				});

		GameScreen gameScreenMock = Mockito.mock(GameScreen.class);
		final Level level = LabyrinthUtil.generateLabyrinth(6, 6,
				gameScreenMock);
		level.moveLabyrinthPiece(Level.DOWN);
		LabyrinthUtil.updateLabyrinthTiles(level);

		Assert.assertEquals("LabyrinthPiece " + 1,
				level.getLabyrinth()[0][0].toString());
		Assert.assertEquals("LabyrinthPiece " + 2,
				level.getLabyrinth()[0][1].toString());
		Assert.assertEquals("LabyrinthPiece " + 3,
				level.getLabyrinth()[0][2].toString());
		Assert.assertEquals("LabyrinthPiece " + 4,
				level.getLabyrinth()[0][3].toString());
		Assert.assertEquals("LabyrinthPiece " + 5,
				level.getLabyrinth()[0][4].toString());
		Assert.assertEquals("LabyrinthPiece " + 0,
				level.getLabyrinth()[0][5].toString());

		PowerMockito.verifyStatic();
	}
}
