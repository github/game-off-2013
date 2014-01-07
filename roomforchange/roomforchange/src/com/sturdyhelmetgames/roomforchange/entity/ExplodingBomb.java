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

import com.badlogic.gdx.graphics.g2d.SpriteBatch;
import com.badlogic.gdx.math.Rectangle;
import com.sturdyhelmetgames.roomforchange.assets.Assets;
import com.sturdyhelmetgames.roomforchange.level.Level;

public class ExplodingBomb extends Bomb {

	private final Rectangle explosionRadius = new Rectangle(0f, 0f, 3.5f, 3.5f);

	public ExplodingBomb(float x, float y, Level level) {
		super(x, y, level);
	}

	@Override
	public void render(float delta, SpriteBatch batch) {
		// super.render(delta, batch);

		// calculate scale
		final float scale = getScale();
		batch.draw(Assets.bomb.getKeyFrame(stateTime), bounds.x, bounds.y
				- 0.5f + zz, 0f, 0f, 1f, 1f, scale, scale, 0f);
	}

	@Override
	public void update(float fixedStep) {
		// super.update(fixedStep);
		stateTime += fixedStep;
		if (stateTime > 2f && !isDead() && !isDying()) {
			aliveTick = ALIVE_TIME_MAX;
			explosionRadius.setPosition(bounds.x - 1.5f, bounds.y - 1.5f);
			level.addParticleEffect(Assets.PARTICLE_EXPLOSION, bounds.x + 0.5f,
					bounds.y + 0.5f);
			Assets.getGameSound(Assets.SOUND_EXPLOSION).play(0.7f);
			for (int i = 0; i < level.entities.size; i++) {
				final Entity entity = level.entities.get(i);
				if (entity != level.player
						&& explosionRadius.overlaps(entity.bounds)) {
					entity.state = EntityState.DYING;
				} else if (explosionRadius.overlaps(level.player.bounds)) {
					level.player.takeDamage();
				}
			}
		}
	}
}
