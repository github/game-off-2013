package com.sturdyhelmetgames.roomforchange.entity;

import com.badlogic.gdx.graphics.g2d.SpriteBatch;
import com.badlogic.gdx.math.Rectangle;
import com.badlogic.gdx.math.Vector2;

public class Entity {

	public static final int DIRECTION_UP = 0;
	public static final int DIRECTION_DOWN = 1;
	public static final int DIRECTION_LEFT = 2;
	public static final int DIRECTION_RIGHT = 3;

	public static final float ACCEL_MAX = 2f;
	public static final float DAMP = 0.1f;

	public final Vector2 accel = new Vector2(0f, 0f);
	public float width, height;
	public final Rectangle bounds = new Rectangle();

	public Entity(float x, float y, float width, float height) {
		this.width = width;
		this.height = height;
		bounds.set(x, y, width, height);
	}

	public void render(float delta, SpriteBatch batch) {

	}

	public void update(float fixedStep) {
		bounds.x += accel.x * fixedStep;
		bounds.y += accel.y * fixedStep;

		accel.x *= DAMP;
		accel.y *= DAMP;
	}

	public void move(int dir) {
		if (dir == DIRECTION_UP) {
			accel.y = ACCEL_MAX;
		} else if (dir == DIRECTION_DOWN) {
			accel.y = -ACCEL_MAX;
		} else if (dir == DIRECTION_LEFT) {
			accel.x = -ACCEL_MAX;
		} else if (dir == DIRECTION_RIGHT) {
			accel.x = ACCEL_MAX;
		}
	}
}
