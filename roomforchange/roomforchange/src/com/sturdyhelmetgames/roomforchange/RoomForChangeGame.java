package com.sturdyhelmetgames.roomforchange;

import com.badlogic.gdx.Game;
import com.sturdyhelmetgames.roomforchange.assets.Assets;
import com.sturdyhelmetgames.roomforchange.screen.GameScreen;

public class RoomForChangeGame extends Game {

	@Override
	public void create() {
		Assets.loadGameData();
		setScreen(new GameScreen(this));
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
