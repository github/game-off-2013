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
					+ 0.6f - 0.65f + zz, 0f, 0f, 1f, 1f, scale, scale, 0f);
		}
	}

	@Override
	public void collectItem() {
		super.collectItem();
		level.player.bombs++;
		this.aliveTick = ALIVE_TIME_MAX;
	}

}
