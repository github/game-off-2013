package com.sturdyhelmetgames.roomforchange.entity;

import com.badlogic.gdx.graphics.g2d.SpriteBatch;
import com.badlogic.gdx.graphics.g2d.TextureRegion;
import com.sturdyhelmetgames.roomforchange.assets.Assets;

public class Player extends Entity {

	private TextureRegion playerRegion;

	public Player(float x, float y) {
		super(x, y, 1f, 1f);
	}

	@Override
	public void render(float delta, SpriteBatch batch) {
		super.render(delta, batch);

		if (playerRegion == null) {
			playerRegion = Assets.getGameObject("player");
		}

		batch.draw(playerRegion, bounds.x, bounds.y, width, height);
	}

	@Override
	public void update(float fixedStep) {
		super.update(fixedStep);
	}

}
