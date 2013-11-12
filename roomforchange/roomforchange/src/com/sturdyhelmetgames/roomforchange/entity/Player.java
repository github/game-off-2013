package com.sturdyhelmetgames.roomforchange.entity;

import com.badlogic.gdx.graphics.g2d.SpriteBatch;
import com.badlogic.gdx.graphics.g2d.TextureRegion;
import com.sturdyhelmetgames.roomforchange.assets.Assets;
import com.sturdyhelmetgames.roomforchange.level.Level;

public class Player extends Entity {

	private TextureRegion playerRegion;

	public Player(float x, float y, Level level) {
		super(x, y, 1f, 1f, level);
		bounds.set(x, y, width - 0.2f, height - 0.2f);
	}

	@Override
	public void render(float delta, SpriteBatch batch) {
		super.render(delta, batch);

		if (playerRegion == null) {
			playerRegion = Assets.getGameObject("player");
		}

		batch.draw(playerRegion, bounds.x - 0.1f, bounds.y - 0.1f, width,
				height);
	}

}
