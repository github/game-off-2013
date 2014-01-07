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
package com.sturdyhelmetgames.roomforchange.entity;

import aurelienribon.tweenengine.Tween;
import aurelienribon.tweenengine.TweenManager;
import aurelienribon.tweenengine.equations.Back;

import com.badlogic.gdx.graphics.g2d.SpriteBatch;
import com.badlogic.gdx.math.MathUtils;
import com.sturdyhelmetgames.roomforchange.RandomUtil;
import com.sturdyhelmetgames.roomforchange.assets.Assets;
import com.sturdyhelmetgames.roomforchange.level.Level;
import com.sturdyhelmetgames.roomforchange.tween.EntityAccessor;

public class Spider extends Enemy {

	public Spider(float x, float y, Level level) {
		super(x, y, 1f, 1f, level);
		health = 1;
	}

	@Override
	public void render(float delta, SpriteBatch batch) {
		if (blinkTick < BLINK_TICK_MAX) {
			super.render(delta, batch);
			batch.draw(Assets.spiderFront.getKeyFrame(stateTime, true),
					bounds.x - 0.1f, bounds.y - 0.1f, width, height);
			for (int i = 0; i < 20; i++) {
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
			Tween.to(this, EntityAccessor.POSITIONY, 2f)
					.target(level.gameScreen.currentCamPosition.y
							+ MathUtils.random(7f)).ease(Back.INOUT)
					.start(tweenManager);
		}

		if (bounds.overlaps(level.player.bounds)) {
			level.player.takeDamage();
		}
	}

}
