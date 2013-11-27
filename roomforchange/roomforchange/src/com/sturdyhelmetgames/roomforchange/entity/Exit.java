package com.sturdyhelmetgames.roomforchange.entity;

import com.badlogic.gdx.graphics.g2d.SpriteBatch;
import com.sturdyhelmetgames.roomforchange.assets.Assets;
import com.sturdyhelmetgames.roomforchange.level.Level;

public class Exit extends Item {

	public Exit(float x, float y, Level level) {
		super(x, y, 1f, 1f, level);
	}

	@Override
	public void update(float fixedStep) {
		super.update(fixedStep);
		aliveTick = 1f;
	}

	@Override
	public void render(float delta, SpriteBatch batch) {
		super.render(delta, batch);
		batch.draw(Assets.getGameObject("exit"), bounds.x, bounds.y, width,
				height);
	}

	@Override
	public void collectItem() {
		if (level.player.canFinishGame()) {
			// level.gameScreen.finishGame();
			// TODO finish game
		}
	}

}
