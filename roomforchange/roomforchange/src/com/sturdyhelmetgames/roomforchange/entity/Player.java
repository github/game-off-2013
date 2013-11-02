package com.sturdyhelmetgames.roomforchange.entity;

import com.badlogic.gdx.graphics.g2d.SpriteBatch;
import com.badlogic.gdx.graphics.g2d.TextureRegion;
import com.sturdyhelmetgames.roomforchange.assets.Assets;

public class Player extends Entity {

	private TextureRegion playerRegion;

	public Player(float width, float height) {
		super(width, height);
	}

	@Override
	public void initAssets() {
		super.initAssets();
		playerRegion = Assets.getGameObject("player");
	}

	@Override
	public void render(float delta, SpriteBatch batch) {
		super.render(delta, batch);
		batch.draw(playerRegion, bounds.x, bounds.y, width, height);
	}

	@Override
	public void update(float fixedStep) {
		super.update(fixedStep);
	}

}
