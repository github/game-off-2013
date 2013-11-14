package com.sturdyhelmetgames.roomforchange.assets;

import java.io.File;

import com.badlogic.gdx.Gdx;
import com.badlogic.gdx.assets.AssetManager;
import com.badlogic.gdx.files.FileHandle;
import com.badlogic.gdx.graphics.Pixmap;
import com.badlogic.gdx.graphics.Texture;
import com.badlogic.gdx.graphics.g2d.Animation;
import com.badlogic.gdx.graphics.g2d.TextureAtlas;
import com.badlogic.gdx.graphics.g2d.TextureRegion;
import com.badlogic.gdx.math.MathUtils;
import com.badlogic.gdx.utils.Array;
import com.sturdyhelmetgames.roomforchange.level.PieceTemplate;
import com.sturdyhelmetgames.roomforchange.level.RoomObjectTemplate;

public class Assets {

	private static final AssetManager assetManager = new AssetManager();

	public static final String ATLAS_FILE_OBJECTS_ALL = "objects_all";
	private static final String FOLDER_DATA = "data/";
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

	public static Animation mummyWalkFront;
	public static Animation mummyWalkBack;

	public static Animation playerWalkFront;
	public static Animation playerWalkBack;
	public static Animation playerWalkRight;
	public static Animation playerWalkLeft;

	public static final Array<PieceTemplate> pieceTemplates = new Array<PieceTemplate>();
	public static final Array<RoomObjectTemplate> roomObjectTemplates = new Array<RoomObjectTemplate>();

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

		mummyWalkFront = new Animation(0.2f,
				new TextureRegion[] { getGameObject("mummy-front-1"),
						getGameObject("mummy-front-2"),
						getGameObject("mummy-front-3"),
						getGameObject("mummy-front-4"), });

		mummyWalkBack = new Animation(0.2f, new TextureRegion[] {
				getGameObject("mummy-back-1"), getGameObject("mummy-back-2"),
				getGameObject("mummy-back-3"), getGameObject("mummy-back-4"), });

		playerWalkFront = new Animation(0.2f, new TextureRegion[] {
				getGameObject("player-front-1"),
				getGameObject("player-front-2"), });
		playerWalkBack = new Animation(0.2f,
				new TextureRegion[] { getGameObject("player-back-1"),
						getGameObject("player-back-2"), });
		playerWalkRight = new Animation(0.2f, new TextureRegion[] {
				getGameObject("player-right-1"),
				getGameObject("player-right-2"), });
		playerWalkLeft = new Animation(0.2f,
				new TextureRegion[] { getGameObject("player-left-1"),
						getGameObject("player-left-2"), });

		final FileHandle[] pieceTemplateHandles = new FileHandle[15];
		for (int i = 1; i <= 15; i++) {
			pieceTemplateHandles[i - 1] = Gdx.files.internal(FOLDER_DATA
					+ "piecetemplates" + File.separator + i + "_piece.png");
		}
		for (FileHandle handle : pieceTemplateHandles) {
			final Pixmap pixmap = new Pixmap(handle);
			pieceTemplates.add(new PieceTemplate(pixmap));
		}

		final FileHandle[] roomObjectHandles = new FileHandle[0];
		for (FileHandle handle : roomObjectHandles) {
			final Pixmap pixmap = new Pixmap(handle);
			roomObjectTemplates.add(new RoomObjectTemplate(pixmap));
		}
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

		for (int i = 0; i < pieceTemplates.size; i++) {
			pieceTemplates.get(i).getPixmap().dispose();
		}
		pieceTemplates.clear();

		for (int i = 0; i < roomObjectTemplates.size; i++) {
			roomObjectTemplates.get(i).pixmap.dispose();
		}
		roomObjectTemplates.clear();
	}

	public static PieceTemplate getRandomPieceTemplate() {
		return pieceTemplates.get(MathUtils.random(pieceTemplates.size - 1));
	}

	public static RoomObjectTemplate getRandomRoomObjectTemplate() {
		return roomObjectTemplates.get(MathUtils
				.random(roomObjectTemplates.size));
	}

}
