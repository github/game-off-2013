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
