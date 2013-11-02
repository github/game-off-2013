package com.sturdyhelmetgames.roomforchange.assets;

import com.badlogic.gdx.assets.AssetManager;
import com.badlogic.gdx.graphics.Texture;
import com.badlogic.gdx.graphics.g2d.TextureAtlas;
import com.badlogic.gdx.graphics.g2d.TextureRegion;

public class Assets {

	private static final AssetManager assetManager = new AssetManager();

	public static final String ATLAS_FILE_OBJECTS_ALL = "objects_all";
	private static final String FOLDER_DATA = "data//";
	private static final String ATLAS_FILEPATH_OBJECTS_ALL = FOLDER_DATA
			+ ATLAS_FILE_OBJECTS_ALL + ".atlas";

	public static final String TEXTURE_FONT_BIG_BLACK = FOLDER_DATA
			+ "font-big-black.png";
	public static final String TEXTURE_FONT_BIG_WHITE = FOLDER_DATA
			+ "font-big-white.png";
	public static final String TEXTURE_FONT_SMALL_BLACK = FOLDER_DATA
			+ "font-small-black.png";
	public static final String TEXTURE_FONT_SMALL_WHITE = FOLDER_DATA
			+ "font-small-white.png";
	public static TextureRegion[] fontBigBlack;
	public static TextureRegion[] fontBigWhite;
	public static TextureRegion[] fontSmallBlack;
	public static TextureRegion[] fontSmallWhite;

	public static void loadGameData() {
		assetManager.load(ATLAS_FILEPATH_OBJECTS_ALL, TextureAtlas.class);

		assetManager.load(TEXTURE_FONT_BIG_BLACK, Texture.class);
		assetManager.load(TEXTURE_FONT_BIG_WHITE, Texture.class);
		assetManager.load(TEXTURE_FONT_SMALL_BLACK, Texture.class);
		assetManager.load(TEXTURE_FONT_SMALL_WHITE, Texture.class);

		finishLoading();

		fontBigBlack = new TextureRegion(get(TEXTURE_FONT_BIG_BLACK,
				Texture.class)).split(8, 8)[0];
		fontBigWhite = new TextureRegion(get(TEXTURE_FONT_BIG_WHITE,
				Texture.class)).split(8, 8)[0];
		fontSmallBlack = new TextureRegion(get(TEXTURE_FONT_SMALL_BLACK,
				Texture.class)).split(4, 4)[0];
		fontSmallWhite = new TextureRegion(get(TEXTURE_FONT_SMALL_WHITE,
				Texture.class)).split(4, 4)[0];
	}

	public static TextureRegion getGameObject(String objectName) {
		return get(ATLAS_FILEPATH_OBJECTS_ALL, TextureAtlas.class).findRegion(
				objectName);
	}

	public static boolean update() {
		boolean result = assetManager.update();
		System.out.println("AssetManager progress: "
				+ assetManager.getProgress() + " result " + result);
		return result;
	}

	public static <T> T get(String assetFileName, Class<T> type) {
		return assetManager.get(assetFileName, type);
	}

	public static void finishLoading() {
		assetManager.finishLoading();
	}

	public static void clear() {
		assetManager.clear();
	}

}
