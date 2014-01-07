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
import com.badlogic.gdx.InputAdapter;
import com.badlogic.gdx.Screen;
import com.badlogic.gdx.graphics.GL10;
import com.badlogic.gdx.graphics.GL20;
import com.badlogic.gdx.graphics.OrthographicCamera;
import com.badlogic.gdx.graphics.g2d.BitmapFont;
import com.badlogic.gdx.graphics.g2d.SpriteBatch;
import com.badlogic.gdx.math.Matrix4;
import com.sturdyhelmetgames.roomforchange.RoomForChangeGame;

/**
 * A basic screen that implements some commonly needed features in all games
 * like {@link InputAdapter} and {@link Screen}.
 * 
 * @author Antti
 * 
 */
public abstract class Basic2DScreen extends InputAdapter implements Screen {

	protected RoomForChangeGame game;
	protected final OrthographicCamera camera;
	protected final SpriteBatch spriteBatch = new SpriteBatch();
	protected final Matrix4 normalProjection = new Matrix4();

	private static final int MAX_FPS = 60;
	private static final int MIN_FPS = 15;
	protected static final float TIME_STEP = 1f / MAX_FPS;
	private static final float MAX_STEPS = 1f + MAX_FPS / MIN_FPS;
	private static final float MAX_TIME_PER_FRAME = TIME_STEP * MAX_STEPS;
	private float stepTimeLeft;
	protected boolean paused = false;

	protected static BitmapFont debugFont;

	public Basic2DScreen() {
		camera = new OrthographicCamera(12, 8);
	}

	public Basic2DScreen(RoomForChangeGame game, int viewPortWidth,
			int viewPortHeight) {
		this.game = game;

		debugFont = new BitmapFont();

		// create and reset camera
		camera = new OrthographicCamera(viewPortWidth, viewPortHeight);

		// set a normal projection matrix
		normalProjection.setToOrtho2D(0, 0, Gdx.graphics.getWidth(),
				Gdx.graphics.getHeight());

		Gdx.input.setInputProcessor(this);
	}

	/**
	 * Does a fixed timestep independently of framerate.
	 * 
	 * @param delta
	 *            Delta time.
	 * @return True if stepped, false otherwise.
	 */
	private void fixedStep(float delta) {
		stepTimeLeft += delta;
		if (stepTimeLeft > MAX_TIME_PER_FRAME)
			stepTimeLeft = MAX_TIME_PER_FRAME;
		while (stepTimeLeft >= TIME_STEP) {
			updateScreen(TIME_STEP);
			stepTimeLeft -= TIME_STEP;
		}
	}

	/**
	 * Updates the screen (game) logic.
	 * 
	 * @param fixedStep
	 *            A fixed timestep.
	 */
	protected abstract void updateScreen(float fixedStep);

	/**
	 * Renders the screen graphics.
	 * 
	 * @param delta
	 *            Delta time.
	 */
	public abstract void renderScreen(float delta);

	@Override
	public void render(float delta) {

		if (!paused) {
			fixedStep(delta);
		}

		if (Gdx.graphics.isGL20Available()) {
			Gdx.graphics.getGL20().glClear(GL20.GL_COLOR_BUFFER_BIT);
		} else {
			Gdx.graphics.getGL10().glClear(GL10.GL_COLOR_BUFFER_BIT);
		}

		renderScreen(delta);

		// render fps if in debug mode
		if (game.isDebug()) {
			spriteBatch.getProjectionMatrix().set(normalProjection);
			spriteBatch.begin();
			debugFont.draw(spriteBatch,
					"FPS: " + Gdx.graphics.getFramesPerSecond(), 0, 20);
			spriteBatch.end();
		}
	}

	@Override
	public void resize(int width, int height) {

	}

	@Override
	public void show() {

	}

	@Override
	public void hide() {

	}

	@Override
	public void pause() {
		paused = true;
	}

	@Override
	public void resume() {
		paused = false;
	}

	@Override
	public void dispose() {

	}

}
