package com.sturdyhelmetgames.roomforchange.client;

import com.sturdyhelmetgames.roomforchange.RoomForChangeGame;
import com.badlogic.gdx.ApplicationListener;
import com.badlogic.gdx.backends.gwt.GwtApplication;
import com.badlogic.gdx.backends.gwt.GwtApplicationConfiguration;

public class GwtLauncher extends GwtApplication {
	@Override
	public GwtApplicationConfiguration getConfig() {
		GwtApplicationConfiguration cfg = new GwtApplicationConfiguration(960,
				640);
		return cfg;
	}

	@Override
	public ApplicationListener getApplicationListener() {
		return new RoomForChangeGame();
	}
}