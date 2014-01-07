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
package com.sturdyhelmetgames.roomforchange.tween;

import aurelienribon.tweenengine.TweenAccessor;

import com.badlogic.gdx.math.Vector3;

public class Vector3Accessor implements TweenAccessor<Vector3> {

	public static final int POSITION_X = 1;
	public static final int POSITION_Y = 2;
	public static final int POSITION_XY = 3;

	@Override
	public int getValues(Vector3 target, int tweenType, float[] returnValues) {
		switch (tweenType) {
		case POSITION_X:
			returnValues[0] = target.x;
			return 1;
		case POSITION_Y:
			returnValues[0] = target.y;
			return 2;
		case POSITION_XY:
			returnValues[0] = target.x;
			returnValues[1] = target.y;
			return 3;
		default:
			assert false;
			return -1;
		}
	}

	@Override
	public void setValues(Vector3 target, int tweenType, float[] newValues) {
		switch (tweenType) {
		case POSITION_X:
			target.x = newValues[0];
			break;
		case POSITION_Y:
			target.y = newValues[0];
			break;
		case POSITION_XY:
			target.x = newValues[0];
			target.y = newValues[1];
			break;
		default:
			assert false;
			break;
		}

	}

}
