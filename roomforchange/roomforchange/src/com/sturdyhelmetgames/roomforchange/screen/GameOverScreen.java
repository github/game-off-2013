package com.sturdyhelmetgames.roomforchange.screen;

import com.badlogic.gdx.Input.Keys;
import com.badlogic.gdx.graphics.Color;
import com.sturdyhelmetgames.roomforchange.RoomForChangeGame;
import com.sturdyhelmetgames.roomforchange.assets.Assets;

public class GameOverScreen extends Basic2DScreen {

	private final GameScreen gameScreen;

	public GameOverScreen(RoomForChangeGame game, GameScreen gameScreen) {
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
		final Color originalColor = spriteBatch.getColor();
		spriteBatch.setColor(1f, 1f, 1f, 0.5f);
		spriteBatch.draw(Assets.getFullGameObject("black"), -6f, -4f, 12f, 8f);
		spriteBatch.setColor(originalColor);
		spriteBatch
				.draw(Assets.getFullGameObject("gameover"), -2f, -1f, 4f, 2f);
		spriteBatch.end();
	}

	@Override
	public boolean keyDown(int keycode) {
		if (keycode == Keys.Y) {
			Assets.getGameSound(Assets.SOUND_MUSIC).loop(0.4f);
			game.setScreen(new GameScreen(game));
		} else if (keycode == Keys.N) {
			game.setScreen(new MenuScreen(game));
		}
		return super.keyDown(keycode);
	}

	@Override
	public void show() {
		super.show();
		Assets.getGameSound(Assets.SOUND_MUSIC).stop();
	}

}
