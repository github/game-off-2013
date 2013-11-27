package com.sturdyhelmetgames.roomforchange.entity;

import aurelienribon.tweenengine.Tween;
import aurelienribon.tweenengine.TweenManager;
import aurelienribon.tweenengine.equations.Quad;

import com.badlogic.gdx.graphics.g2d.SpriteBatch;
import com.badlogic.gdx.math.MathUtils;
import com.sturdyhelmetgames.roomforchange.RandomUtil;
import com.sturdyhelmetgames.roomforchange.assets.Assets;
import com.sturdyhelmetgames.roomforchange.level.Level;
import com.sturdyhelmetgames.roomforchange.tween.EntityAccessor;

public class Spider extends Enemy {

	public Spider(float x, float y, Level level) {
		super(x, y, 1f, 1f, level);
		health = 3;
	}

	@Override
	public void render(float delta, SpriteBatch batch) {
		if (blinkTick < BLINK_TICK_MAX) {
			super.render(delta, batch);
			batch.draw(Assets.spiderFront.getKeyFrame(stateTime, true),
					bounds.x - 0.1f, bounds.y - 0.1f, width, height);
			for (int i = 0; i < 10; i++) {
				batch.draw(Assets.getGameObject("spider-thread"),
						bounds.x - 0.1f, bounds.y + i * height + 1f - 0.1f,
						width, height);
			}
		}
	}

	@Override
	public void update(float fixedStep) {
		if (pause > 0f) {
			pause -= fixedStep;
		}
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
			if (bounds.overlaps(level.player.bounds)) {
				level.player.takeDamage();
			}
			stateTime += fixedStep;

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
		}
		final TweenManager tweenManager = level.entityTweenManager;
		if (!tweenManager.containsTarget(this)) {
			Tween.to(this, EntityAccessor.POSITIONY, MathUtils.random(3f))
					.target(level.gameScreen.currentCamPosition.y
							+ MathUtils.random(4f)).ease(Quad.IN)
					.start(tweenManager);
		}

		if (bounds.overlaps(level.player.bounds)) {
			level.player.takeDamage();
		}
	}

}
