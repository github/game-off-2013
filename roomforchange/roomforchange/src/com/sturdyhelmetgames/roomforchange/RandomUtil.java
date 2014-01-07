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

import com.badlogic.gdx.math.MathUtils;

/**
 * Utility class for getting randomized values.
 * 
 * @author antti
 * 
 */
public final class RandomUtil extends MathUtils {

	private RandomUtil() {
		// method hidden
	}

	/**
	 * Randomizes a one-of-three value.
	 * 
	 * @return true if randomized value was one of three, false otherwise.
	 */
	public static boolean oneThird() {
		int selector = MathUtils.random(2);
		if (selector == 0) {
			return true;
		}
		return false;
	}

	/**
	 * Randomizes one-of-fifth value.
	 * 
	 * @return returns true 20% of the time, false 80% of the time.
	 */
	public static boolean oneFifth() {
		int selector = MathUtils.random(9);
		if (selector < 2) {
			return true;
		}
		return false;
	}

	/**
	 * Randomizes a fifty-fifty value.
	 * 
	 * @return true half of the time, false half of the time
	 */
	public static boolean fiftyFifty() {
		int selector = MathUtils.random(1);
		if (selector == 0) {
			return true;
		}
		return false;
	}

	/**
	 * Randomizes a "big range" number using {@link #fiftyFifty()}. Half of the
	 * time the randomized number is a negative range, and half of the time a
	 * positive range.
	 * 
	 * @param range
	 * @return a number that can range from -range through range
	 */
	public static float bigRangeRandom(float range) {
		float randomness = MathUtils.random(range);
		if (fiftyFifty()) {
			randomness = -randomness;
		}
		return randomness;
	}

}
