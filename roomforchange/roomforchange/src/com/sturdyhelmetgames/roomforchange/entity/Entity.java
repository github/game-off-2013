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
package com.sturdyhelmetgames.roomforchange.entity;

import com.badlogic.gdx.Gdx;
import com.badlogic.gdx.graphics.g2d.SpriteBatch;
import com.badlogic.gdx.math.Rectangle;
import com.badlogic.gdx.math.Vector2;
import com.sturdyhelmetgames.roomforchange.level.Level;
import com.sturdyhelmetgames.roomforchange.level.Level.LevelTile;

public class Entity {

	public Level level;
	public EntityState state;
	public Direction direction = Direction.DOWN;

	public static final float ACCEL_MAX = 2f;
	public static final float VEL_MAX = 0.05f;
	public static final float INERTIA = 10f;
	private static final float MIN_WALK_VELOCITY = 0.001f;
	protected static final float INVULNERABLE_TIME_MIN = 1.5f;
	protected static final float BLINK_TICK_MAX = 0.1f;

	public final Vector2 accel = new Vector2(0f, 0f);
	public final Vector2 vel = new Vector2(0f, 0f);
	public float stateTime = 0f;
	public float width, height;
	public final Rectangle bounds = new Rectangle();
	public final Rectangle[] r = { new Rectangle(), new Rectangle(),
			new Rectangle(), new Rectangle() };
	public final HoleFallWrapper[] holes = { new HoleFallWrapper(),
			new HoleFallWrapper(), new HoleFallWrapper(), new HoleFallWrapper() };
	public int[][] tiles;

	public float pause = 0f;
	protected float invulnerableTick;
	protected float blinkTick;

	public class HoleFallWrapper {
		public final Rectangle bounds = new Rectangle();

		public void set(float x, float y, float width, float height) {
			bounds.set(x, y, width, height);
		}

		public void unset() {
			bounds.set(-1, -1, 0, 0);
		}
	}

	public float getMaxVelocity() {
		return VEL_MAX;
	}

	public float getInertia() {
		return INERTIA;
	}

	public enum Direction {
		UP, DOWN, LEFT, RIGHT;

		public Direction turnLeft() {
			switch (this) {
			case UP:
				return LEFT;
			case DOWN:
				return RIGHT;
			case LEFT:
				return DOWN;
			case RIGHT:
				return UP;
			}
			return UP;
		}

		public Direction turnRight() {
			switch (this) {
			case UP:
				return RIGHT;
			case DOWN:
				return LEFT;
			case LEFT:
				return UP;
			case RIGHT:
				return DOWN;
			}
			return DOWN;
		}
	}

	public enum EntityState {
		IDLE, WALKING, FALLING, DYING, DEAD;
	}

	public Entity(float x, float y, float width, float height, Level level) {
		this.level = level;
		this.width = width;
		this.height = height;
		bounds.set(x, y, width - 0.2f, height - 0.2f);
		state = EntityState.IDLE;
	}

	public void render(float delta, SpriteBatch batch) {

	}

	public void update(float fixedStep) {
		if (pause > 0f) {
			pause -= fixedStep;
		}

		// tick dying
		if (blinkTick > BLINK_TICK_MAX) {
			blinkTick = 0f;
		}
		// tick alive & dying times
		invulnerableTick -= fixedStep;
		if (invulnerableTick > 0f) {
			blinkTick += fixedStep;
		}
		if (invulnerableTick <= 0f) {
			blinkTick = 0f;
		}

		if (pause <= 0f) {
			tryMove();

			vel.add(accel);
			if (vel.x > getMaxVelocity()) {
				vel.x = getMaxVelocity();
			}
			if (vel.x < -getMaxVelocity()) {
				vel.x = -getMaxVelocity();
			}
			if (vel.y > getMaxVelocity()) {
				vel.y = getMaxVelocity();
			}
			if (vel.y < -getMaxVelocity()) {
				vel.y = -getMaxVelocity();
			}
			accel.scl(fixedStep);

			if (state == EntityState.WALKING) {
				vel.lerp(Vector2.Zero, getInertia() * fixedStep);
			} else {
				vel.scl(fixedStep);
			}

			stateTime += fixedStep;
		}
	}

	public void moveWithAccel(Direction dir) {
		if (dir == Direction.UP) {
			accel.y = ACCEL_MAX;
		} else if (dir == Direction.DOWN) {
			accel.y = -ACCEL_MAX;
		} else if (dir == Direction.LEFT) {
			accel.x = -ACCEL_MAX;
		} else if (dir == Direction.RIGHT) {
			accel.x = ACCEL_MAX;
		}
		direction = dir;
		state = EntityState.WALKING;
	}

	protected void tryMove() {
		bounds.y += vel.y;
		fetchCollidableRects();

		for (int i = 0; i < holes.length; i++) {
			HoleFallWrapper hole = holes[i];
			if (bounds.overlaps(hole.bounds)) {
				fall();
			}
		}

		for (int i = 0; i < r.length; i++) {
			Rectangle rect = r[i];
			if (bounds.overlaps(rect)) {
				if (vel.y < 0) {
					bounds.y = rect.y + rect.height + 0.01f;
				} else
					bounds.y = rect.y - bounds.height - 0.01f;
				vel.y = 0;
				hitWallHook();
			}
		}

		bounds.x += vel.x;
		fetchCollidableRects();
		for (int i = 0; i < holes.length; i++) {
			HoleFallWrapper hole = holes[i];
			if (bounds.overlaps(hole.bounds)) {
				fall();
			}
		}

		for (int i = 0; i < r.length; i++) {
			Rectangle rect = r[i];
			if (bounds.overlaps(rect)) {
				if (vel.x < 0)
					bounds.x = rect.x + rect.width + 0.01f;
				else
					bounds.x = rect.x - bounds.width - 0.01f;
				vel.x = 0;
				hitWallHook();
			}
		}
	}

	protected void fall() {
		state = EntityState.FALLING;
	}

	protected void fetchCollidableRects() {
		int p1x = (int) bounds.x;
		int p1y = (int) Math.floor(bounds.y);
		int p2x = (int) (bounds.x + bounds.width);
		int p2y = (int) Math.floor(bounds.y);
		int p3x = (int) (bounds.x + bounds.width);
		int p3y = (int) (bounds.y + bounds.height);
		int p4x = (int) bounds.x;
		int p4y = (int) (bounds.y + bounds.height);

		try {
			LevelTile tile1 = null;
			if (level.getTiles().length >= p1x && p1x >= 0
					&& level.getTiles()[p1x].length >= p1y && p1y >= 0)
				tile1 = level.getTiles()[p1x][p1y];
			LevelTile tile2 = null;
			if (level.getTiles().length >= p2x && p1x >= 0
					&& level.getTiles()[p2x].length >= p2y && p2y >= 0)
				tile2 = level.getTiles()[p2x][p2y];
			LevelTile tile3 = null;
			if (level.getTiles().length >= p3x && p3x >= 0
					&& level.getTiles()[p3x].length >= p3y && p3y >= 0)
				tile3 = level.getTiles()[p3x][p3y];
			LevelTile tile4 = null;
			if (level.getTiles().length >= p4x && p4x >= 0
					&& level.getTiles()[p4x].length >= p4y && p4y >= 0)
				tile4 = level.getTiles()[p4x][p4y];

			if (tile1 != null && tile1.isHole())
				holes[0].set(p1x + 0.4f, p1y + 0.5f, 0.2f, 0.05f);
			else
				holes[0].unset();
			if (tile1 != null && tile1.isCollidable()) {
				r[0].set(p1x, p1y, 1, 1);
			} else {
				r[0].set(-1, -1, 0, 0);
			}

			if (tile2 != null && tile2.isHole())
				holes[1].set(p2x + 0.4f, p2y + 0.5f, 0.2f, 0.05f);
			else
				holes[1].unset();
			if (tile2 != null && tile2.isCollidable()) {
				r[1].set(p2x, p2y, 1, 1);
			} else {
				r[1].set(-1, -1, 0, 0);
			}

			if (tile3 != null && tile3.isHole())
				holes[2].set(p3x + 0.4f, p3y + 0.5f, 0.2f, 0.05f);
			else
				holes[2].unset();
			if (tile3 != null && tile3.isCollidable()) {
				r[2].set(p3x, p3y, 1, 1);

			} else {
				r[2].set(-1, -1, 0, 0);
			}

			if (tile4 != null && tile4.isHole())
				holes[3].set(p4x + 0.4f, p4y + 0.5f, 0.2f, 0.05f);
			else
				holes[3].unset();
			if (tile4 != null && tile4.isCollidable()) {
				r[3].set(p4x, p4y, 1, 1);
			} else {
				r[3].set(-1, -1, 0, 0);
			}
		} catch (ArrayIndexOutOfBoundsException e) {
			Gdx.app.log("Creature", "Creature went off screen");
		}
	}

	public boolean isNotWalking() {
		if (vel.x > -MIN_WALK_VELOCITY && vel.x < MIN_WALK_VELOCITY
				&& vel.y > -MIN_WALK_VELOCITY && vel.y < MIN_WALK_VELOCITY) {
			state = EntityState.IDLE;
			return true;
		}
		return false;
	}

	public void hit(Rectangle hitBounds) {

	}

	public void hitWallHook() {

	}

	public boolean isFalling() {
		return state == EntityState.FALLING;
	}

	public boolean isDying() {
		return state == EntityState.DYING;
	}

	public boolean isDead() {
		return state == EntityState.DEAD;
	}

	public boolean isAlive() {
		return !isDying() && !isDead() && !isFalling();
	}
}
