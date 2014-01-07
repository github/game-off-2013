/*    Copyright 2013 Antti Kolehmainen

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License. */
package com.sturdyhelmetgames.roomforchange.screen;

import com.badlogic.gdx.Gdx;
import com.badlogic.gdx.Input.Keys;
import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.g2d.TextureRegion;
import com.sturdyhelmetgames.roomforchange.RoomForChangeGame;
import com.sturdyhelmetgames.roomforchange.assets.Assets;
import com.sturdyhelmetgames.roomforchange.level.Level;

public class LeverScreen extends Basic2DScreen {

	private final GameScreen gameScreen;

	public static final int UP = 0;
	public static final int DOWN = 1;
	public static final int LEFT = 2;
	public static final int RIGHT = 3;
	public static final int NEUTRAL = 4;

	private int leverDirection = NEUTRAL;
	private float leverStateTime = 0f;
	private float MAX_LEVER_STATETIME = 2.5f;

	public LeverScreen(RoomForChangeGame game, GameScreen gameScreen) {
		super(game, 12, 8);
		this.gameScreen = gameScreen;
	}

	@Override
	protected void updateScreen(float fixedStep) {
		gameScreen.updateScreen(fixedStep);

		if (leverDirection != NEUTRAL) {
			leverStateTime += fixedStep;
			if (leverStateTime > MAX_LEVER_STATETIME) {
				leverStateTime = 0f;
				leverDirection = NEUTRAL;
			}
		}
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

		TextureRegion region = null;
		if (leverDirection == UP) {
			region = Assets.getFullGameObject("lever-up");
		} else if (leverDirection == DOWN) {
			region = Assets.getFullGameObject("lever-down");
		} else if (leverDirection == LEFT) {
			region = Assets.getFullGameObject("lever-left");
		} else if (leverDirection == RIGHT) {
			region = Assets.getFullGameObject("lever-right");
		} else if (leverDirection == NEUTRAL) {
			region = Assets.getFullGameObject("lever-neutral");
		}
		spriteBatch.draw(region, -1f, -1f, 2f, 2f);
		spriteBatch.end();
	}

	@Override
	public boolean keyDown(int keycode) {

		if (leverDirection == NEUTRAL)
			if (keycode == Keys.UP) {
				gameScreen.startScreenQuake(Level.UP);
				leverDirection = UP;
			} else if (keycode == Keys.DOWN) {
				gameScreen.startScreenQuake(Level.DOWN);
				leverDirection = DOWN;
			} else if (keycode == Keys.LEFT) {
				gameScreen.startScreenQuake(Level.LEFT);
				leverDirection = LEFT;
			} else if (keycode == Keys.RIGHT) {
				gameScreen.startScreenQuake(Level.RIGHT);
				leverDirection = RIGHT;
			} else if (keycode == Keys.Z || keycode == Keys.ESCAPE) {
				Gdx.input.setInputProcessor(gameScreen);
				game.setScreen(gameScreen);
			}
		return super.keyDown(keycode);
	}

}
