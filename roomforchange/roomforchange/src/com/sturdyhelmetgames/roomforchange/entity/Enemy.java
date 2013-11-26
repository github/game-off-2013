package com.sturdyhelmetgames.roomforchange.entity;

import com.badlogic.gdx.math.Rectangle;
import com.sturdyhelmetgames.roomforchange.RandomUtil;
import com.sturdyhelmetgames.roomforchange.assets.Assets;
import com.sturdyhelmetgames.roomforchange.level.Level;

public class Enemy extends Entity {

	protected int health;
	protected static final float INVULNERABLE_TIME_MIN = 1.5f;
	protected static final float BLINK_TICK_MAX = 0.1f;
	protected float invulnerableTick;
	protected float blinkTick;

	public Enemy(float x, float y, float width, float height, Level level) {
		super(x, y, width, height, level);
	}

	@Override
	public void update(float fixedStep) {
		super.update(fixedStep);

		// tick dying
		if (blinkTick > BLINK_TICK_MAX) {
			blinkTick = 0f;
		}
		// tick alive & dying times
		invulnerableTick -= fixedStep;
		if (invulnerableTick > 0f) {
			blinkTick += fixedStep;
		}
		if (invulnerableTick <= 0f) {
			blinkTick = 0f;
		}

		if (pause <= 0f) {
			if (state == EntityState.DYING) {
				level.addParticleEffect(Assets.PARTICLE_ENEMY_DIE, bounds.x
						+ width / 2, bounds.y + height / 2);

				state = EntityState.DEAD;
				final int random = RandomUtil.random(100);
				if (random < 30) {
					level.entities.add(new Heart(bounds.x, bounds.y, level));
				} else if (random < 60) {
					level.entities.add(new Bomb(bounds.x, bounds.y, level));
				} else if (random < 100) {
				}
			}

			if (bounds.overlaps(level.player.bounds)) {
				level.player.takeDamage();
			}
		}
	}

	public void takeDamage() {
		if (pause <= 0f) {
			health--;
			if (health <= 0) {
				state = EntityState.DYING;
			} else {
				pause = INVULNERABLE_TIME_MIN;
				invulnerableTick = INVULNERABLE_TIME_MIN;
			}
		}
	}

	@Override
	public void hit(Rectangle hitBounds) {
		if (hitBounds.overlaps(bounds)) {
			takeDamage();
		}
	}

	@Override
	protected void fall() {
		// do nothing
	}

}
