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

public class KingSpider extends Spider {

	public KingSpider(float x, float y, Level level) {
		super(x, y, level);
		health = 2;
	}

	@Override
	public void render(float delta, SpriteBatch batch) {
		if (blinkTick < BLINK_TICK_MAX) {
			super.render(delta, batch);
			batch.draw(Assets.kingSpiderFront.getKeyFrame(stateTime, true),
					bounds.x - 0.1f, bounds.y - 0.1f, width, height);
			for (int i = 0; i < 20; i++) {
				batch.draw(Assets.getGameObject("spider-thread"),
						bounds.x - 0.1f, bounds.y + i * height + 1f - 0.1f,
						width, height);
			}
		}
	}

}
