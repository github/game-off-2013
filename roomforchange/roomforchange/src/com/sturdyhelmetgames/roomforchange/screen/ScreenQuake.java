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
package com.sturdyhelmetgames.roomforchange.screen;

import com.badlogic.gdx.graphics.Camera;
import com.badlogic.gdx.math.Vector3;
import com.sturdyhelmetgames.roomforchange.RandomUtil;

public class ScreenQuake {

	private float quakeTimeMax = 2f;
	private final Vector3 originalCameraPosition = new Vector3();
	private Camera camera;
	public boolean active;
	public float quakeTime;
	private Runnable callback;

	public ScreenQuake(Camera camera) {
		this.camera = camera;
		quakeTime = 0f;
		active = false;
	}

	public void update(float fixedStep) {
		quakeTime += fixedStep;
		if (active) {
			final Vector3 cameraPos = camera.position;
			cameraPos.set(originalCameraPosition);
			cameraPos.x += RandomUtil.bigRangeRandom(3) * fixedStep;
			cameraPos.y += RandomUtil.bigRangeRandom(3) * fixedStep;
			camera.update();

			if (quakeTime > quakeTimeMax) {
				deactivate();
			}
		}
	}

	public void activate(float quakeTimeMax, Runnable callback) {
		this.quakeTimeMax = quakeTimeMax;
		this.callback = callback;
		this.originalCameraPosition.set(camera.position);
		active = true;
		quakeTime = 0f;
	}

	public void deactivate() {
		active = false;
		quakeTime = 0f;
		camera.position.set(originalCameraPosition);
		camera.update();
		callback.run();
	}

}
