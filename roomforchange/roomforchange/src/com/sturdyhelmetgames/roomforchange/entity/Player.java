package com.sturdyhelmetgames.roomforchange.entity;

import com.badlogic.gdx.graphics.g2d.Animation;
import com.badlogic.gdx.graphics.g2d.SpriteBatch;
import com.badlogic.gdx.math.Rectangle;
import com.sturdyhelmetgames.roomforchange.assets.Assets;
import com.sturdyhelmetgames.roomforchange.level.Level;
import com.sturdyhelmetgames.roomforchange.level.Level.LevelTile;

public class Player extends Entity {

	private int health;
	private final Rectangle hitBounds = new Rectangle(0f, 0f, 0.8f, 0.8f);

	public Player(float x, float y, Level level) {
		super(x, y, 1f, 1f, level);
		bounds.set(x, y, width - 0.2f, height - 0.2f);
	}

	@Override
	public void render(float delta, SpriteBatch batch) {
		super.render(delta, batch);

		Animation animation = null;

		if (direction == Direction.UP) {
			animation = Assets.playerWalkBack;
		} else if (direction == Direction.DOWN) {
			animation = Assets.playerWalkFront;
		} else if (direction == Direction.RIGHT) {
			animation = Assets.playerWalkRight;
		} else if (direction == Direction.LEFT) {
			animation = Assets.playerWalkLeft;
		}

		if (isNotWalking()) {
			batch.draw(animation.getKeyFrame(0.25f), bounds.x, bounds.y, width,
					height);
		} else {
			batch.draw(animation.getKeyFrame(stateTime, true), bounds.x,
					bounds.y, width, height);
		}

	}

	private static final float HIT_DISTANCE = 1f;
	private final Rectangle hitTileRect = new Rectangle(0f, 0f, 1f, 1f);

	public void tryHit() {
		hitBounds.x = bounds.x;
		hitBounds.y = bounds.y;
		if (direction == Direction.LEFT) {
			hitBounds.x -= HIT_DISTANCE;
		} else if (direction == Direction.RIGHT) {
			hitBounds.x += HIT_DISTANCE;
		} else if (direction == Direction.UP) {
			hitBounds.y += HIT_DISTANCE;
		} else if (direction == Direction.DOWN) {
			hitBounds.y -= HIT_DISTANCE;
		}

		for (int i = 0; i < level.entities.size; i++) {
			final Entity entity = level.entities.get(i);
			entity.hit(hitBounds);
		}

		LevelTile tile = level.getTiles()[(int) hitBounds.x][(int) hitBounds.y];
		if (tile.type == Level.LevelTileType.LEVER) {
			level.gameScreen.openLeverScreen();
		}

	}

}
