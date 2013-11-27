package com.sturdyhelmetgames.roomforchange.entity;

import com.badlogic.gdx.graphics.g2d.SpriteBatch;
import com.sturdyhelmetgames.roomforchange.assets.Assets;
import com.sturdyhelmetgames.roomforchange.level.Level;

public class Gem extends Item {

	public Gem(float x, float y, Level level) {
		super(x, y, 1f, 1f, level);
	}

	@Override
	public void update(float fixedStep) {
		super.update(fixedStep);
		if (aliveTick < ALIVE_TIME_MAX)
			aliveTick = 1f;
	}

	@Override
	public void render(float delta, SpriteBatch batch) {
		super.render(delta, batch);
		final float scale = getScale();
		batch.draw(Assets.getGameObject("gem"), bounds.x,
				bounds.y + zz, 0f, 0f, 1f, 1f, scale, scale, 0f);
	}

	@Override
	public void collectItem() {
		super.collectItem();
		level.player.gotGem = true;
		aliveTick = ALIVE_TIME_MAX;
	}

}
