package com.sturdyhelmetgames.roomforchange.entity;

import com.badlogic.gdx.graphics.g2d.SpriteBatch;
import com.badlogic.gdx.graphics.g2d.TextureRegion;
import com.badlogic.gdx.utils.Array;
import com.sturdyhelmetgames.roomforchange.assets.Assets;

public class Tile extends Entity {

	public final String assetName;
	public TextureRegion region;

	private Tile(String assetName) {
		super(0f, 0f, 1, 1);
		this.assetName = assetName;
		tiles.add(this);
	}

	public static final Array<Tile> tiles = new Array<Tile>(16);
	public static final Tile ground = new Tile("ground");
	public static final Tile brick = new Tile("brick");

	public void render(float delta, SpriteBatch batch, float x, float y) {
		if (region == null) {
			region = Assets.getGameObject(assetName);
		}
		batch.draw(region, x, y, width, height);
	}

	public static Tile get(int index) {
		return tiles.get(index);
	}

}
