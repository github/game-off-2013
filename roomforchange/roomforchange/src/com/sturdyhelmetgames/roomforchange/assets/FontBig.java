package com.sturdyhelmetgames.roomforchange.assets;

import com.badlogic.gdx.graphics.g2d.SpriteBatch;

/**
 * A simple index-based font that can be drawn with the same projection that is
 * used in the game. Big edition.
 * 
 * @author anttik
 * 
 */
public class FontBig {

	/**
	 * Array of supported characters.
	 */
	public static final String CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.,!?'\"-+=/\\%()<>:;";
	/**
	 * Default scale for {@link FontBig}.
	 */
	public static final float SCALE_DEFAULT = 0.25f;

	/**
	 * Black font color.
	 */
	public static final int FONT_COLOR_BLACK = 0;
	/**
	 * White font color.
	 */
	public static final int FONT_COLOR_WHITE = 1;

	/**
	 * Color of this font instance.
	 */
	protected int color;

	/**
	 * Constructs a {@link FontBig} with default color black.
	 */
	public FontBig() {
		color = FONT_COLOR_BLACK;
	}

	/**
	 * Constructs a {@link FontBig} with color.
	 * 
	 * @param color
	 */
	public FontBig(int color) {
		this.color = color;
	}

	/**
	 * Draws the actual character on the screen.
	 * 
	 * @param spriteBatch
	 *            Sprite batch.
	 * @param ix
	 *            Character's index in {@link FontBig#CHARS}.
	 * @param i
	 *            Character's sequential order number.
	 * @param x
	 *            X-coordinate.
	 * @param y
	 *            Y-coordinate.
	 * @param scale
	 *            Drawing scale.
	 */
	protected void drawChar(SpriteBatch spriteBatch, int ix, int i, float x,
			float y, float scale) {
		if (this.color == FONT_COLOR_BLACK)
			spriteBatch.draw(Assets.fontBigBlack[ix], x + i * scale, y,
					scale, scale);
		else
			spriteBatch.draw(Assets.fontBigWhite[ix], x + i * scale, y,
					scale, scale);
	}

	/**
	 * Draws the text on the screen.
	 * 
	 * @param spriteBatch
	 *            Sprite batch.
	 * @param x
	 *            X-coordinate.
	 * @param y
	 *            Y-coordinate.
	 * @param scale
	 *            Drawing scale.
	 */
	public void draw(SpriteBatch spriteBatch, String text, float x, float y,
			float scale) {
		text = text.toUpperCase();
		for (int i = 0; i < text.length(); i++) {
			int ix = CHARS.indexOf(text.charAt(i));
			if (ix >= 0) {
				drawChar(spriteBatch, ix, i, x, y, scale);
			}
		}
	}

	/**
	 * Draws the text on the screen with default scale
	 * {@link FontBig#SCALE_DEFAULT}.
	 * 
	 * @param spriteBatch
	 *            Sprite batch.
	 * @param x
	 *            X-coordinate.
	 * @param y
	 *            Y-coordinate.
	 */
	public void draw(SpriteBatch spriteBatch, String text, float x, float y) {
		draw(spriteBatch, text, x, y, getScale());
	}

	/**
	 * Draws the text array on the screen on multiple lines.
	 * 
	 * @param spriteBatch
	 *            Sprite batch.
	 * @param texts
	 * @param x
	 *            X-coordinate.
	 * @param y
	 *            Y-coordinate.
	 * @param scale
	 */
	public void drawMultiLine(SpriteBatch spriteBatch, String[] texts, float x,
			float y) {
		for (int i = 0; i < texts.length; i++) {
			draw(spriteBatch, texts[i], x, y - i * (getScale() + 0.05f),
					getScale());
		}
	}

	/**
	 * Returns the default scale {@link FontBig#SCALE_DEFAULT}.
	 * 
	 * @return Default scale.
	 */
	public float getScale() {
		return SCALE_DEFAULT;
	}

}