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
package com.sturdyhelmetgames.roomforchange;

import aurelienribon.tweenengine.Tween;

import com.badlogic.gdx.Game;
import com.badlogic.gdx.math.Vector2;
import com.badlogic.gdx.math.Vector3;
import com.sturdyhelmetgames.roomforchange.assets.Assets;
import com.sturdyhelmetgames.roomforchange.entity.Entity;
import com.sturdyhelmetgames.roomforchange.screen.MenuScreen;
import com.sturdyhelmetgames.roomforchange.tween.EntityAccessor;
import com.sturdyhelmetgames.roomforchange.tween.Vector2Accessor;
import com.sturdyhelmetgames.roomforchange.tween.Vector3Accessor;

public class RoomForChangeGame extends Game {

	@Override
	public void create() {
		Assets.loadGameData();

		Tween.setCombinedAttributesLimit(3);
		Tween.registerAccessor(Entity.class, new EntityAccessor());
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
		return false;
	}
}
