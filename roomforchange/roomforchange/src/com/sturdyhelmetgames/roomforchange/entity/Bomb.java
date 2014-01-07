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
import com.sturdyhelmetgames.roomforchange.assets.Assets;
import com.sturdyhelmetgames.roomforchange.level.Level;

public class Bomb extends Item {

	public Bomb(float x, float y, Level level) {
		super(x, y, 1f, 1f, level);
	}

	@Override
	public void render(float delta, SpriteBatch batch) {
		super.render(delta, batch);

		// calculate scale
		final float scale = getScale();
		if (aliveTick < DYING_TICK_MAX || dyingTick < DYING_TICK_MAX) {
			batch.draw(Assets.getGameObject("bomb-3"), bounds.x, bounds.y
					- 0.5f + zz, 0f, 0f, 1f, 1f, scale, scale, 0f);
		}
	}

	@Override
	public void collectItem() {
		super.collectItem();
		level.player.bombs++;
		this.aliveTick = ALIVE_TIME_MAX;
	}

}
