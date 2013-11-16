package com.sturdyhelmetgames.roomforchange;

import aurelienribon.tweenengine.Tween;

import com.badlogic.gdx.Game;
import com.badlogic.gdx.math.Vector2;
import com.badlogic.gdx.math.Vector3;
import com.sturdyhelmetgames.roomforchange.assets.Assets;
import com.sturdyhelmetgames.roomforchange.screen.MenuScreen;
import com.sturdyhelmetgames.roomforchange.tween.Vector2Accessor;
import com.sturdyhelmetgames.roomforchange.tween.Vector3Accessor;

public class RoomForChangeGame extends Game {

	@Override
	public void create() {
		Assets.loadGameData();

		Tween.setCombinedAttributesLimit(3);
		Tween.registerAccessor(Vector2.class, new Vector2Accessor());
		Tween.registerAccessor(Vector3.class, new Vector3Accessor());

		setScreen(new MenuScreen(this));
		// setScreen(new GameScreen(this));
	}

	@Override
	public void dispose() {
		super.dispose();
		Assets.clear();
	}

	public boolean isDebug() {
		return true;
	}
}
