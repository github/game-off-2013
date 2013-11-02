package com.sturdyhelmetgames.roomforchange;

import static com.sturdyhelmetgames.roomforchange.entity.Entity.DIRECTION_DOWN;
import static com.sturdyhelmetgames.roomforchange.entity.Entity.DIRECTION_LEFT;
import static com.sturdyhelmetgames.roomforchange.entity.Entity.DIRECTION_RIGHT;
import static com.sturdyhelmetgames.roomforchange.entity.Entity.DIRECTION_UP;

import com.badlogic.gdx.ApplicationListener;
import com.badlogic.gdx.Gdx;
import com.badlogic.gdx.Input.Keys;
import com.badlogic.gdx.graphics.GL10;
import com.badlogic.gdx.graphics.OrthographicCamera;
import com.badlogic.gdx.graphics.g2d.SpriteBatch;
import com.badlogic.gdx.math.MathUtils;
import com.badlogic.gdx.utils.Array;
import com.sturdyhelmetgames.roomforchange.assets.Assets;
import com.sturdyhelmetgames.roomforchange.entity.Entity;
import com.sturdyhelmetgames.roomforchange.entity.Player;
import com.sturdyhelmetgames.roomforchange.entity.Tile;

public class RoomForChangeGame implements ApplicationListener {
	private OrthographicCamera camera;
	private SpriteBatch batch;

	private final static int MAX_FPS = 60;
	private final static int MIN_FPS = 15;
	private final static float TIME_STEP = 1f / MAX_FPS;
	private final static float MAX_STEPS = 1f + MAX_FPS / MIN_FPS;
	private final static float MAX_TIME_PER_FRAME = TIME_STEP * MAX_STEPS;
	private float stepTimeLeft;

	public static final float VIEWPORT_WIDTH = 15f;
	public static final float VIEWPORT_HEIGHT = 10f;
	public static final float VIEWPORT_WIDTH_HALF = VIEWPORT_WIDTH / 2f;
	public static final float VIEWPORT_HEIGHT_HALF = VIEWPORT_HEIGHT / 2f;
	public static final float CULLING_WIDTH = VIEWPORT_WIDTH_HALF + 2f;
	public static final float CULLING_HEIGHT = VIEWPORT_HEIGHT_HALF + 2f;

	private final Player player = new Player(1f, 1f);
	private final Array<Entity> entities = new Array<Entity>();
	private final int[][] tiles = new int[150][100];

	/**
	 * Does a fixed timestep independent of framerate.
	 * 
	 * @param delta
	 *            Delta time since the last frame.
	 */
	private void fixedStep(final float delta) {
		stepTimeLeft += delta;
		if (stepTimeLeft > MAX_TIME_PER_FRAME)
			stepTimeLeft = MAX_TIME_PER_FRAME;
		while (stepTimeLeft >= TIME_STEP) {
			update(TIME_STEP);
			stepTimeLeft -= TIME_STEP;
		}
	}

	@Override
	public void create() {
		Assets.loadGameData();

		float w = Gdx.graphics.getWidth();
		float h = Gdx.graphics.getHeight();

		camera = new OrthographicCamera(VIEWPORT_WIDTH, VIEWPORT_HEIGHT);
		batch = new SpriteBatch();

		entities.add(player);

		for (int x = 0; x < 150; x++) {
			for (int y = 0; y < 100; y++) {
				tiles[x][y] = MathUtils.random(1);
			}
		}
	}

	@Override
	public void dispose() {
		Assets.clear();
		batch.dispose();
	}

	@Override
	public void render() {

		final float deltaTime = Gdx.app.getGraphics().getDeltaTime();
		fixedStep(deltaTime);

		Gdx.gl.glClearColor(0.1f, 0.1f, 0.1f, 0.1f);
		Gdx.gl.glClear(GL10.GL_COLOR_BUFFER_BIT);

		batch.setProjectionMatrix(camera.combined);
		batch.begin();
		for (int x = 0; x < 150; x++) {
			for (int y = 0; y < 100; y++) {
				int index = tiles[x][y];
				Tile.get(index).render(deltaTime, batch, x, y);
			}
		}
		for (Entity entity : entities) {
			entity.render(deltaTime, batch);
		}
		batch.end();
	}

	public void update(float fixedStep) {
		camera.position.set(player.bounds.x, player.bounds.y, 0f);
		camera.update();
		processKeys();
		for (Entity entity : entities) {
			entity.update(fixedStep);
		}
	}

	private void processKeys() {
		if (Gdx.input.isKeyPressed(Keys.UP)) {
			player.move(DIRECTION_UP);
		}
		if (Gdx.input.isKeyPressed(Keys.DOWN)) {
			player.move(DIRECTION_DOWN);
		}
		if (Gdx.input.isKeyPressed(Keys.LEFT)) {
			player.move(DIRECTION_LEFT);
		}
		if (Gdx.input.isKeyPressed(Keys.RIGHT)) {
			player.move(DIRECTION_RIGHT);
		}
	}

	@Override
	public void resize(int width, int height) {
	}

	@Override
	public void pause() {
	}

	@Override
	public void resume() {
	}
}
