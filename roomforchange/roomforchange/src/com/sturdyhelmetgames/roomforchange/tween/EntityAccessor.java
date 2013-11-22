package com.sturdyhelmetgames.roomforchange.tween;

import aurelienribon.tweenengine.TweenAccessor;

import com.sturdyhelmetgames.roomforchange.entity.Entity;

public class EntityAccessor implements TweenAccessor<Entity> {

	public static final int POSITIONXY = 1;
	public static final int POSITIONX = 2;
	public static final int POSITIONY = 3;

	@Override
	public int getValues(Entity target, int tweenType, float[] returnValues) {
		switch (tweenType) {
		case POSITIONXY:
			returnValues[0] = target.bounds.x;
			returnValues[1] = target.bounds.y;
			return POSITIONXY;
		case POSITIONX:
			returnValues[0] = target.bounds.x;
			return POSITIONX;
		case POSITIONY:
			returnValues[0] = target.bounds.y;
			return POSITIONY;
		default:
			assert false;
			return -1;
		}
	}

	@Override
	public void setValues(Entity target, int tweenType, float[] newValues) {
		switch (tweenType) {
		case POSITIONXY:
			target.bounds.x = newValues[0];
			target.bounds.y = newValues[1];
			break;
		case POSITIONX:
			target.bounds.x = newValues[0];
			break;
		case POSITIONY:
			target.bounds.y = newValues[0];
			break;
		default:
			assert false;
			break;
		}
	}

}
