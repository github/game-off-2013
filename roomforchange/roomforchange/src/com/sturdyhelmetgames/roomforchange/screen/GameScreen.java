package com.sturdyhelmetgames.roomforchange.screen;

import com.badlogic.gdx.Gdx;
import com.badlogic.gdx.Input.Keys;
import com.badlogic.gdx.graphics.OrthographicCamera;
import com.badlogic.gdx.graphics.g2d.SpriteBatch;
import com.sturdyhelmetgames.roomforchange.RoomForChangeGame;
import com.sturdyhelmetgames.roomforchange.entity.Entity;
import com.sturdyhelmetgames.roomforchange.level.Level;
import com.sturdyhelmetgames.roomforchange.util.LabyrinthUtil;

public class GameScreen extends Basic2DScreen {

	public static final int SCALE = 24;
	private OrthographicCamera cameraMiniMap;
	private SpriteBatch batchMiniMap;

	private Level level;

	public GameScreen(RoomForChangeGame game) {
		super(game, 12, 8);

		camera.position.set(6f, 4f, 0f);
		camera.update();

		// setup our mini map camera
		cameraMiniMap = new OrthographicCamera(12, 8);
		cameraMiniMap.zoom = SCALE;
		cameraMiniMap.position.set(-90f, 90f, 0f);
		cameraMiniMap.update();
		batchMiniMap = new SpriteBatch();

		level = LabyrinthUtil.generateLabyrinth(4, 4);
	}

	@Override
	protected void updateScreen(float fixedStep) {
		processKeys();

		camera.position.set(level.player.bounds.x, level.player.bounds.y, 0f);
		camera.update();

		level.update(fixedStep);
	}

	@Override
	public void renderScreen(float delta) {
		spriteBatch.setProjectionMatrix(camera.combined);
		spriteBatch.begin();
		level.render(delta, spriteBatch);
		spriteBatch.end();

		// set the projection matrix for our batch so that it draws
		// with the zoomed out perspective of the minimap camera
		batchMiniMap.setProjectionMatrix(cameraMiniMap.combined);

		// draw the player
		batchMiniMap.begin();
		level.render(delta, batchMiniMap);
		batchMiniMap.end();

	}

	protected void processKeys() {
		if (!paused) {
			// process player movement keys
			if (Gdx.input.isKeyPressed(Keys.UP)) {
				level.player.accel.y = Entity.ACCEL_MAX;
			}
			if (Gdx.input.isKeyPressed(Keys.DOWN)) {
				level.player.accel.y = -Entity.ACCEL_MAX;
			}
			if (Gdx.input.isKeyPressed(Keys.RIGHT)) {
				level.player.accel.x = Entity.ACCEL_MAX;
			}
			if (Gdx.input.isKeyPressed(Keys.LEFT)) {
				level.player.accel.x = -Entity.ACCEL_MAX;
			}

			if (Gdx.input.isKeyPressed(Keys.MINUS)) {
				camera.zoom += 0.1f;
				camera.update();
			}
			if (Gdx.input.isKeyPressed(Keys.PLUS)) {
				camera.zoom -= 0.1f;
				camera.update();
			}

			if (Gdx.input.isKeyPressed(Keys.W)) {

			}

			if (Gdx.input.isKeyPressed(Keys.A)) {

			}

			if (Gdx.input.isKeyPressed(Keys.S)) {

			}

			if (Gdx.input.isKeyPressed(Keys.D)) {

			}
		}
	}

	@Override
	public void dispose() {
		super.dispose();
		batchMiniMap.dispose();
	}

}
