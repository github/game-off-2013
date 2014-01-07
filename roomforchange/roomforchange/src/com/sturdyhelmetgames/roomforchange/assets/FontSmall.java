/*    Copyright 2013 Antti Kolehmainen

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License. */
package com.sturdyhelmetgames.roomforchange.assets;

import com.badlogic.gdx.graphics.g2d.SpriteBatch;

/**
 * A simple index-based font that can be drawn with the same projection that is
 * used in the game. Small edition.
 * 
 * @author anttik
 * 
 */
public class FontSmall extends FontBig {

	public FontSmall() {
		super();
	}

	public FontSmall(int color) {
		super(color);
	}

	/**
	 * Default scale for {@link FontSmall}.
	 */
	public static final float SCALE_DEFAULT = 0.23f;

	@Override
	protected void drawChar(SpriteBatch spriteBatch, int ix, int i, float x,
			float y, float scale) {
		if (this.color == FONT_COLOR_BLACK)
			spriteBatch.draw(Assets.fontSmallBlack[ix], x + i
					* (scale + 0.1f), y, scale, scale);
		else
			spriteBatch.draw(Assets.fontSmallWhite[ix], x + i
					* (scale + 0.1f), y, scale, scale);
	}

	/**
	 * Returns the default scale {@link FontSmall#SCALE_DEFAULT}.
	 * 
	 * @return Default scale.
	 */
	@Override
	public float getScale() {
		return SCALE_DEFAULT;
	}

}