package com.sturdyhelmetgames.roomforchange.entity;

import com.sturdyhelmetgames.roomforchange.level.Level;

public class Enemy extends Entity {

	public Enemy(float x, float y, float width, float height, Level level) {
		super(x, y, width, height, level);
	}

	@Override
	public void update(float fixedStep) {
		super.update(fixedStep);

		if (bounds.overlaps(level.player.bounds)) {
			level.player.takeDamage();
		}
	}
}
