package com.sturdyhelmetgames.roomforchange.entity;

import com.badlogic.gdx.graphics.g2d.Animation;
import com.badlogic.gdx.graphics.g2d.SpriteBatch;
import com.sturdyhelmetgames.roomforchange.assets.Assets;
import com.sturdyhelmetgames.roomforchange.level.Level;

public class Player extends Entity {

	private int health;

	public Player(float x, float y, Level level) {
		super(x, y, 1f, 1f, level);
		bounds.set(x, y, width - 0.2f, height - 0.2f);
	}

	@Override
	public void render(float delta, SpriteBatch batch) {
		super.render(delta, batch);

		Animation animation = null;

		if (direction == Direction.UP) {
			animation = Assets.playerWalkBack;
		} else if (direction == Direction.DOWN) {
			animation = Assets.playerWalkFront;
		} else if (direction == Direction.RIGHT) {
			animation = Assets.playerWalkRight;
		} else if (direction == Direction.LEFT) {
			animation = Assets.playerWalkLeft;
		}

		if (isNotWalking()) {
			batch.draw(animation.getKeyFrame(0.25f), bounds.x, bounds.y, width,
					height);
		} else {
			batch.draw(animation.getKeyFrame(stateTime, true), bounds.x,
					bounds.y, width, height);
		}

	}

}
