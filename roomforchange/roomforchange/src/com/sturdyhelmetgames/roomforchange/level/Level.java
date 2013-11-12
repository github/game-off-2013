package com.sturdyhelmetgames.roomforchange.level;

import com.badlogic.gdx.graphics.g2d.SpriteBatch;
import com.badlogic.gdx.utils.Array;
import com.sturdyhelmetgames.roomforchange.assets.Assets;
import com.sturdyhelmetgames.roomforchange.entity.Entity;
import com.sturdyhelmetgames.roomforchange.entity.Player;

public class Level {

	private LabyrinthPiece[][] labyrinth;
	private LevelTile[][] tiles;

	public Player player;
	public final Array<Entity> entities = new Array<Entity>();

	public LabyrinthPiece[][] getLabyrinth() {
		return labyrinth;
	}

	public void setLabyrinth(LabyrinthPiece[][] labyrinth) {
		this.labyrinth = labyrinth;
	}

	public LevelTile[][] getTiles() {
		return tiles;
	}

	public void setTiles(LevelTile[][] tiles) {
		this.tiles = tiles;
	}

	public static enum LevelTileType {
		GROUND, WALL, EXIT;

		public boolean isExit() {
			return this == EXIT;
		}

		public boolean isCollidable() {
			return this == WALL;
		}
	}

	public static class LevelTile {
		public final LevelTileType type;

		public LevelTile(LevelTileType type) {
			this.type = type;
		}

		public void render(float delta, SpriteBatch batch, float x, float y) {
			if (type == LevelTileType.GROUND) {
				batch.draw(Assets.getGameObject("ground"), x, y, 1f, 1f);
			} else if (type == LevelTileType.WALL) {
				batch.draw(Assets.getGameObject("brick"), x, y, 1f, 1f);
			}
		}

		public boolean isCollidable() {
			return type.isCollidable();
		}
	}

	public void update(float fixedStep) {
		for (int i = 0; i < entities.size; i++) {
			entities.get(i).update(fixedStep);
		}
	}

	public void render(float delta, SpriteBatch batch) {
		for (int x = 0; x < tiles.length; x++) {
			for (int y = 0; y < tiles[0].length; y++) {
				final LevelTile tile = tiles[x][y];
				tile.render(delta, batch, x, y);
			}
		}
		for (int i = 0; i < entities.size; i++) {
			entities.get(i).render(delta, batch);
		}
	}
	
	public void moveLabyrinthPiece(int dir) {
		
	}

}
