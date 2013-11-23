package com.sturdyhelmetgames.roomforchange.entity;

import com.badlogic.gdx.graphics.g2d.Animation;
import com.badlogic.gdx.graphics.g2d.SpriteBatch;
import com.badlogic.gdx.math.Rectangle;
import com.sturdyhelmetgames.roomforchange.assets.Assets;
import com.sturdyhelmetgames.roomforchange.level.Level;
import com.sturdyhelmetgames.roomforchange.level.Level.LevelTile;

public class Player extends Entity {

	public float dyingAnimState = 0f;
	public float dyingTime = 0f;
	public float maxDyingTime = 3f;
	public float invulnerableTime = 0f;
	public float maxInvulnerableTime = 4f;
	public int health = 5;
	public int maxHealth = 5;
	private final Rectangle hitBounds = new Rectangle(0f, 0f, 0.8f, 0.8f);

	public Player(float x, float y, Level level) {
		super(x, y, 1f, 0.6f, level);
	}

	@Override
	public void render(float delta, SpriteBatch batch) {
		super.render(delta, batch);

		Animation animation = null;

		if (isFalling()) {
			animation = Assets.playerFalling;
			batch.draw(animation.getKeyFrame(dyingAnimState), bounds.x - 0.1f,
					bounds.y - 0.1f, width, height + 0.4f);
		} else if (isDying() || isDead()) {
			animation = Assets.playerDying;
			batch.draw(animation.getKeyFrame(dyingAnimState), bounds.x - 0.1f,
					bounds.y - 0.1f, width, height + 0.4f);
		} else {
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
				batch.draw(animation.getKeyFrame(0.25f), bounds.x - 0.1f,
						bounds.y - 0.1f, width, height + 0.4f);
			} else {
				batch.draw(animation.getKeyFrame(stateTime, true),
						bounds.x - 0.1f, bounds.y - 0.1f, width, height + 0.4f);
			}
		}

	}

	@Override
	public void update(float fixedStep) {

		if (isDying() || isFalling()) {
			dyingAnimState += fixedStep;
			dyingTime += fixedStep;
			if (dyingTime >= maxDyingTime) {
				state = EntityState.DEAD;
			}
		}

		super.update(fixedStep);

		if (invulnerableTime > 0f) {
			invulnerableTime -= fixedStep;
		}
	}

	private static final float HIT_DISTANCE = 0.5f;

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

		// double the hit distance for tiles
		if (direction == Direction.LEFT) {
			hitBounds.x -= HIT_DISTANCE;
		} else if (direction == Direction.RIGHT) {
			hitBounds.x += HIT_DISTANCE;
		} else if (direction == Direction.UP) {
			hitBounds.y += HIT_DISTANCE;
		} else if (direction == Direction.DOWN) {
			hitBounds.y -= HIT_DISTANCE;
		}

		LevelTile tile = level.getTiles()[(int) hitBounds.x][(int) hitBounds.y];
		if (tile.type == Level.LevelTileType.LEVER) {
			level.gameScreen.openLeverScreen();
		}

	}

	public void takeDamage() {
		if (!isInvulnerable()) {
			health--;
			invulnerableTime = maxInvulnerableTime;
		}
		if (health <= 0) {
			state = EntityState.DYING;
		}
	}

	public void gainHealth() {
		health++;
	}

	public boolean isInvulnerable() {
		return invulnerableTime > 0f;
	}

}
