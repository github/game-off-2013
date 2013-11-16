package com.sturdyhelmetgames.roomforchange.entity;

import com.badlogic.gdx.graphics.g2d.Animation;
import com.badlogic.gdx.graphics.g2d.SpriteBatch;
import com.badlogic.gdx.math.MathUtils;
import com.badlogic.gdx.math.Vector2;
import com.sturdyhelmetgames.roomforchange.assets.Assets;
import com.sturdyhelmetgames.roomforchange.level.Level;

public class Mummy extends Entity {

	private float ACCEL_MAX = 1f;
	private float MAX_WALK_TIME = 3f;
	private float walkTime;
	private final Vector2 constantAccel = new Vector2();

	public Mummy(float x, float y, Level level) {
		super(x, y, 1f, 1f, level);
		state = EntityState.WALKING;
	}

	@Override
	public float getMaxVelocity() {
		return 0.03f;
	}

	@Override
	public void render(float delta, SpriteBatch batch) {
		super.render(delta, batch);
		Animation animation = null;

		if (direction == Direction.UP) {
			animation = Assets.mummyWalkBack;
		} else {
			animation = Assets.mummyWalkFront;
		}

		batch.draw(animation.getKeyFrame(stateTime, true), bounds.x, bounds.y,
				width, height);
	}

	private final Vector2 playerPos = new Vector2();
	private final Vector2 mummyPos = new Vector2();

	@Override
	public void update(float fixedStep) {
		super.update(fixedStep);

		level.player.bounds.getPosition(playerPos);
		bounds.getPosition(mummyPos);

		boolean mummyAttacking = playerPos.dst2(mummyPos) < 10f;
		if (mummyAttacking) {
			accel.set(playerPos.sub(mummyPos).scl(0.6f));
		} else {
			if (walkTime > MAX_WALK_TIME) {
				walkTime = 0;
			}
			if (walkTime == 0) {
				if (MathUtils.randomBoolean()) {
					constantAccel.x = ACCEL_MAX;
				} else {
					constantAccel.x = -ACCEL_MAX;
				}
				if (MathUtils.randomBoolean()) {
					constantAccel.y = ACCEL_MAX;
				} else {
					constantAccel.y = -ACCEL_MAX;
				}
			} else {
				accel.set(constantAccel);
			}

			if (walkTime == 0) {

			}
		}
		walkTime += fixedStep;

		if (vel.y <= 0f) {
			direction = Direction.DOWN;
		} else {
			direction = Direction.UP;
		}
	}
}
