package com.sturdyhelmetgames.roomforchange.entity;

import com.badlogic.gdx.graphics.g2d.Animation;
import com.badlogic.gdx.graphics.g2d.SpriteBatch;
import com.badlogic.gdx.math.MathUtils;
import com.sturdyhelmetgames.roomforchange.assets.Assets;
import com.sturdyhelmetgames.roomforchange.level.Level;

public class Snake extends Enemy {

	public Snake(float x, float y, Level level) {
		super(x, y, 1f, 1f, level);
		health = 1;
	}

	@Override
	public float getMaxVelocity() {
		return 0.08f;
	}

	@Override
	public void render(float delta, SpriteBatch batch) {
		if (blinkTick < BLINK_TICK_MAX) {
			super.render(delta, batch);
			Animation animation = null;

			if (direction == Direction.UP) {
				animation = Assets.snakeWalkBack;
			} else if (direction == Direction.DOWN) {
				animation = Assets.snakeWalkFront;
			} else if (direction == Direction.RIGHT) {
				animation = Assets.snakeWalkRight;
			} else if (direction == Direction.LEFT) {
				animation = Assets.snakeWalkLeft;
			}

			batch.draw(animation.getKeyFrame(stateTime, true), bounds.x - 0.1f,
					bounds.y - 0.1f, width, height);
		}
	}

	@Override
	public void update(float fixedStep) {
		super.update(fixedStep);

		if (isNotWalking()) {

			direction = direction.turnRight();
		}
		moveWithAccel(direction);
	}

	@Override
	public void hitWallHook() {
		direction = MathUtils.randomBoolean() ? direction.turnLeft()
				: direction.turnRight();
	}

	@Override
	public float getInertia() {
		return 20f;
	}

}
