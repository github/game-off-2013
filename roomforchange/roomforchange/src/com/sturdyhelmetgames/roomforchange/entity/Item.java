package com.sturdyhelmetgames.roomforchange.entity;

import com.sturdyhelmetgames.roomforchange.level.Level;

public abstract class Item extends Entity {

	public boolean collected = false;

	protected static final float ALIVE_TIME_MAX = 5f;
	protected static final float DYING_TIME_MIN = 3.5f;
	protected static final float DYING_TICK_MAX = 0.1f;
	protected float aliveTick;
	protected float dyingTick;
	protected float zz, za;

	/**
	 * Default scale.
	 */
	private static final float SCALE_AMOUNT_DEFAULT = 1f;
	/**
	 * Entity scaling amoung variable.
	 */
	private static final float SCALE_AMOUNT_VERTICAL = 80f;

	public Item(float x, float y, float width, float height, Level level) {
		super(x, y, width, height, level);
		zz = 0.5f;
		za = 0f;
	}

	/**
	 * Resets the {@link CollectableEntity} state.
	 * 
	 * @param x
	 * @param y
	 */
	protected void reset(float x, float y) {
		this.bounds.x = x;
		this.bounds.y = y;
		aliveTick = 0f;
		dyingTick = 0f;
		stateTime = 0f;
		zz = 0.5f;
		za = 0f;
	}

	@Override
	public void update(float fixedStep) {
		super.update(fixedStep);

		// bounce the entity
		zz += za;
		if (zz < 0f) {
			zz = 0f;
			za *= (-50f * fixedStep);
		}
		za -= (1f * fixedStep);

		// tick dying
		if (dyingTick > DYING_TICK_MAX) {
			dyingTick = 0f;
		}

		// tick alive & dying times
		aliveTick += fixedStep;
		if (aliveTick >= DYING_TIME_MIN) {
			dyingTick += fixedStep;
		}

		if (bounds.overlaps(level.player.bounds)) {
			collectItem();
		}
	}

	public abstract void collectItem();

	/**
	 * Checks if the {@link CollectableEntity} is alive or not.
	 * 
	 * @return True if {@link CollectableEntity} is alive, false otherwise.
	 */
	@Override
	public boolean isAlive() {
		if (aliveTick >= ALIVE_TIME_MAX) {
			return false;
		}
		return true;
	}

	/**
	 * Returns the current scale for the entity. Scale calculation is based on
	 * the position of the {@link BasicEntity} on the y-axis.
	 * 
	 * @return Scale
	 */
	protected float getScale() {
		return SCALE_AMOUNT_DEFAULT - bounds.y / SCALE_AMOUNT_VERTICAL;
	}

}
