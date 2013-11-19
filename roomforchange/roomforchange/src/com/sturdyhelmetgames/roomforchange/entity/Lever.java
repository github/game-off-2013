package com.sturdyhelmetgames.roomforchange.entity;

import com.badlogic.gdx.graphics.g2d.SpriteBatch;
import com.badlogic.gdx.math.Rectangle;
import com.sturdyhelmetgames.roomforchange.assets.Assets;
import com.sturdyhelmetgames.roomforchange.level.Level;

public class Lever extends Entity {

	public Lever(float x, float y, Level level) {
		super(x, y, 1f, 1f, level);
	}

	@Override
	public void render(float delta, SpriteBatch batch) {
		batch.draw(Assets.getGameObject("lever"), bounds.x, bounds.y, width,
				height);
	}

	@Override
	public void hit(Rectangle hitBounds) {
		if (hitBounds.overlaps(bounds)) {
			level.gameScreen.openLeverScreen();
		}
	}

}
