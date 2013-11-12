package com.sturdyhelmetgames.roomforchange;

import com.badlogic.gdx.backends.lwjgl.LwjglApplication;
import com.badlogic.gdx.backends.lwjgl.LwjglApplicationConfiguration;

public class Main {
	public static void main(String[] args) {
		LwjglApplicationConfiguration cfg = new LwjglApplicationConfiguration();
		cfg.title = "roomforchange";
		cfg.useGL20 = true;
		cfg.vSyncEnabled = false;
		cfg.foregroundFPS = 10000;
		cfg.width = 480;
		cfg.height = 320;

		new LwjglApplication(new RoomForChangeGame(), cfg);
	}
}
