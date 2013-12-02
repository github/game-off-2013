using Pathfinding;
using UnityEngine;

namespace Pathfinding
{
	/** Holds a coordinate in integers */
	public struct Int3 {
		public int x;
		public int y;
		public int z;
		
		//These should be set to the same value (only PrecisionFactor should be 1 divided by Precision)
		
		/** Precision for the integer coordinates.
		 * One world unit is divided into [value] pieces. A value of 1000 would mean millimeter precision, a value of 1 would mean meter precision (assuming 1 world unit = 1 meter).
		 * This value affects the maximum coordinates for nodes as well as how large the cost values are for moving between two nodes.
		 * A higher value means that you also have to set all penalty values to a higher value to compensate since the normal cost of moving will be higher.
		 */
		public const int Precision = 1000;
		
		/** #Precision as a float */
		public const float FloatPrecision = 1000F;
		
		/** 1 divided by #Precision */
		public const float PrecisionFactor = 0.001F;
		
		/* Factor to multiply cost with */
		//public const float CostFactor = 0.01F;
		
		private static Int3 _zero = new Int3(0,0,0);
		public static Int3 zero { get { return _zero; } }
		
		public Int3 (Vector3 position) {
			x = (int)System.Math.Round (position.x*FloatPrecision);
			y = (int)System.Math.Round (position.y*FloatPrecision);
			z = (int)System.Math.Round (position.z*FloatPrecision);
			//x = Mathf.RoundToInt (position.x);
			//y = Mathf.RoundToInt (position.y);
			//z = Mathf.RoundToInt (position.z);
		}
		
		
		public Int3 (int _x, int _y, int _z) {
			x = _x;
			y = _y;
			z = _z;
		}
		
		public static bool operator == (Int3 lhs, Int3 rhs) {
			return 	lhs.x == rhs.x &&
					lhs.y == rhs.y &&
					lhs.z == rhs.z;
		}
		
		public static bool operator != (Int3 lhs, Int3 rhs) {
			return 	lhs.x != rhs.x ||
					lhs.y != rhs.y ||
					lhs.z != rhs.z;
		}
		
		public static explicit operator Int3 (Vector3 ob) {
			return new Int3 (
				(int)System.Math.Round (ob.x*FloatPrecision),
				(int)System.Math.Round (ob.y*FloatPrecision),
				(int)System.Math.Round (ob.z*FloatPrecision)
				);
			//return new Int3 (Mathf.RoundToInt (ob.x*FloatPrecision),Mathf.RoundToInt (ob.y*FloatPrecision),Mathf.RoundToInt (ob.z*FloatPrecision));
		}
		
		public static explicit operator Vector3 (Int3 ob) {
			return new Vector3 (ob.x*PrecisionFactor,ob.y*PrecisionFactor,ob.z*PrecisionFactor);
		}
		
		public static Int3 operator - (Int3 lhs, Int3 rhs) {
			lhs.x -= rhs.x;
			lhs.y -= rhs.y;
			lhs.z -= rhs.z;
			return lhs;
		}
		
		public static Int3 operator + (Int3 lhs, Int3 rhs) {
			lhs.x += rhs.x;
			lhs.y += rhs.y;
			lhs.z += rhs.z;
			return lhs;
		}
		
		public static Int3 operator * (Int3 lhs, int rhs) {
			lhs.x *= rhs;
			lhs.y *= rhs;
			lhs.z *= rhs;
			
			return lhs;
		}
		
		public static Int3 operator * (Int3 lhs, float rhs) {
			lhs.x = (int)System.Math.Round (lhs.x * rhs);
			lhs.y = (int)System.Math.Round (lhs.y * rhs);
			lhs.z = (int)System.Math.Round (lhs.z * rhs);
			
			return lhs;
		}
		
		public static Int3 operator * (Int3 lhs, Vector3 rhs) {
			lhs.x = (int)System.Math.Round (lhs.x * rhs.x);
			lhs.y =	(int)System.Math.Round (lhs.y * rhs.y);
			lhs.z = (int)System.Math.Round (lhs.z * rhs.z);
			
			return lhs;
		}
		
		public static Int3 operator / (Int3 lhs, float rhs) {
			lhs.x = (int)System.Math.Round (lhs.x / rhs);
			lhs.y = (int)System.Math.Round (lhs.y / rhs);
			lhs.z = (int)System.Math.Round (lhs.z / rhs);
			return lhs;
		}
		
		public int this[int i] {
			get {
				return i == 0 ? x : (i == 1 ? y : z);
			}
		}
		
		public static int Dot (Int3 lhs, Int3 rhs) {
			return
					lhs.x * rhs.x +
					lhs.y * rhs.y +
					lhs.z * rhs.z;
		}
		
		public Int3 NormalizeTo (int newMagn) {
			float magn = magnitude;
			
			if (magn == 0) {
				return this;
			}
			
			x *= newMagn;
			y *= newMagn;
			z *= newMagn;
			
			x = (int)System.Math.Round (x/magn);
			y = (int)System.Math.Round (y/magn);
			z = (int)System.Math.Round (z/magn);
			
			return this;
		}
		
		/** Returns the magnitude of the vector. The magnitude is the 'length' of the vector from 0,0,0 to this point. Can be used for distance calculations:
		  * \code Debug.Log ("Distance between 3,4,5 and 6,7,8 is: "+(new Int3(3,4,5) - new Int3(6,7,8)).magnitude); \endcode
		  */
		public float magnitude {
			get {
				//It turns out that using doubles is just as fast as using ints with Mathf.Sqrt. And this can also handle larger numbers (possibly with small errors when using huge numbers)!
				
				double _x = x;
				double _y = y;
				double _z = z;
				
				return (float)System.Math.Sqrt (_x*_x+_y*_y+_z*_z);
				
				//return Mathf.Sqrt (x*x+y*y+z*z);
			}
		}
		
		/** Magnitude used for the cost between two nodes. The default cost between two nodes can be calculated like this:
		  * \code int cost = (node1.position-node2.position).costMagnitude; \endcode
		  */
		public int costMagnitude {
			get {
				return (int)System.Math.Round (magnitude);
			}
		}
		
		/** The magnitude in world units */
		public float worldMagnitude {
			get {
				double _x = x;
				double _y = y;
				double _z = z;
				
				return (float)System.Math.Sqrt (_x*_x+_y*_y+_z*_z)*PrecisionFactor;
				
				//Scale numbers down
				/*float _x = x*PrecisionFactor;
				float _y = y*PrecisionFactor;
				float _z = z*PrecisionFactor;
				return Mathf.Sqrt (_x*_x+_y*_y+_z*_z);*/
			}
		}
		
		/** The squared magnitude of the vector */
		public float sqrMagnitude {
			get {
				double _x = x;
				double _y = y;
				double _z = z;
				return (float)(_x*_x+_y*_y+_z*_z);
				//return x*x+y*y+z*z;
			}
		}
		
		/** \warning Can cause number overflows if the magnitude is too large */
		public int unsafeSqrMagnitude {
			get {
				return x*x+y*y+z*z;
			}
		}
		
		/** To avoid number overflows. \deprecated Int3.magnitude now uses the same implementation */
		[System.Obsolete ("Same implementation as .magnitude")]
		public float safeMagnitude {
			get {
				//Of some reason, it is faster to use doubles (almost 40% faster)
				double _x = x;
				double _y = y;
				double _z = z;
				
				return (float)System.Math.Sqrt (_x*_x+_y*_y+_z*_z);
				
				//Scale numbers down
				/*float _x = x*PrecisionFactor;
				float _y = y*PrecisionFactor;
				float _z = z*PrecisionFactor;
				//Find the root and scale it up again
				return Mathf.Sqrt (_x*_x+_y*_y+_z*_z)*FloatPrecision;*/
			}
		}
		
		/** To avoid number overflows. The returned value is the squared magnitude of the world distance (i.e divided by Precision) 
		 * \deprecated .sqrMagnitude is now per default safe (Int3.unsafeSqrMagnitude can be used for unsafe operations) */
		[System.Obsolete (".sqrMagnitude is now per default safe (.unsafeSqrMagnitude can be used for unsafe operations)")]
		public float safeSqrMagnitude {
			get {
				float _x = x*PrecisionFactor;
				float _y = y*PrecisionFactor;
				float _z = z*PrecisionFactor;
				return _x*_x+_y*_y+_z*_z;
			}
		}
		
		public static implicit operator string (Int3 ob) {
			return ob.ToString ();
		}
		
		/** Returns a nicely formatted string representing the vector */
		public override string ToString () {
			return "( "+x+", "+y+", "+z+")";
		}
		
		public override bool Equals (System.Object o) {
			
			if (o == null) return false;
			
			Int3 rhs = (Int3)o;
			
			return 	x == rhs.x &&
					y == rhs.y &&
					z == rhs.z;
		}
		
		public override int GetHashCode () {
			return x*9+y*10+z*11;
		}
	}
}

