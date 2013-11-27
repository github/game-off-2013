package com.sturdyhelmetgames.roomforchange.screen;

import com.badlogic.gdx.Input.Keys;
import com.sturdyhelmetgames.roomforchange.RoomForChangeGame;
import com.sturdyhelmetgames.roomforchange.assets.Assets;

public class HelpScreen extends Basic2DScreen {

	private final GameScreen gameScreen;

	public HelpScreen(RoomForChangeGame game, GameScreen gameScreen) {
		super(game, 12, 8);
		this.gameScreen = gameScreen;
	}

	@Override
	protected void updateScreen(float fixedStep) {

	}

	@Override
	public void renderScreen(float delta) {
		gameScreen.renderScreen(delta);
		spriteBatch.setProjectionMatrix(camera.combined);
		spriteBatch.begin();

		spriteBatch.draw(Assets.getFullGameObject("help"), -4f, -2f, 8f, 4f);
		spriteBatch.end();

	}

	@Override
	public boolean keyDown(int keycode) {
		if (keycode == Keys.SPACE) {
			game.setScreen(gameScreen);
		}
		return super.keyDown(keycode);
	}

}
