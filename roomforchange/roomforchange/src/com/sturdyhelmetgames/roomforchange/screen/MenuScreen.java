package com.sturdyhelmetgames.roomforchange.screen;

import com.badlogic.gdx.Input.Keys;
import com.sturdyhelmetgames.roomforchange.RoomForChangeGame;
import com.sturdyhelmetgames.roomforchange.assets.Assets;

public class MenuScreen extends Basic2DScreen {

	public MenuScreen(RoomForChangeGame game) {
		super(game, 12, 8);
	}

	@Override
	protected void updateScreen(float fixedStep) {

	}

	@Override
	public void renderScreen(float delta) {
		spriteBatch.setProjectionMatrix(camera.combined);
		spriteBatch.begin();
		spriteBatch
				.draw(Assets.getFullGameObject("pyramid"), -6f, -4f, 12f, 8f);
		spriteBatch.end();
	}

	@Override
	public boolean keyDown(int keycode) {
		if (keycode == Keys.SPACE) {
			game.setScreen(new GameScreen(game));
			return true;
		}
		return super.keyDown(keycode);
	}

}
