using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace tk2dEditor.SpriteCollectionEditor
{
	public class TextureEditor
	{
		public enum Mode
		{
			None,
			Texture,
			Anchor,
			Collider,
			AttachPoint,
		}
		
		int textureBorderPixels = 16;
		
		Mode mode = Mode.Texture;
		Vector2 textureScrollPos = new Vector2(0.0f, 0.0f);
		bool drawColliderNormals = false;
		
		float editorDisplayScale 
		{
			get { return SpriteCollection.editorDisplayScale; }
			set { SpriteCollection.editorDisplayScale = value; }
		}
		
		Color[] _handleInactiveColors = new Color[] { 
			new Color32(127, 201, 122, 255), // default
			new Color32(180, 0, 0, 255), // red
			new Color32(255, 255, 255, 255), // white
			new Color32(32, 32, 32, 255), // black
		};
		
		Color[] _handleActiveColors = new Color[] {
			new Color32(228, 226, 60, 255),
			new Color32(255, 0, 0, 255),
			new Color32(255, 0, 0, 255),
			new Color32(96, 0, 0, 255),
		};
		
		tk2dSpriteCollectionDefinition.ColliderColor currentColliderColor = tk2dSpriteCollectionDefinition.ColliderColor.Default;
		Color handleInactiveColor { get { return _handleInactiveColors[(int)currentColliderColor]; } }
		Color handleActiveColor { get { return _handleActiveColors[(int)currentColliderColor]; } }

		IEditorHost host;
		public TextureEditor(IEditorHost host)
		{
			this.host = host;
		}
		SpriteCollectionProxy SpriteCollection { get { return host.SpriteCollection; } }
		
		public void SetMode(Mode mode)
		{
			if (this.mode != mode)
			{
				this.mode = mode;
				HandleUtility.Repaint();
			}
		}

		Vector2 ClosestPointOnLine(Vector2 p, Vector2 p1, Vector2 p2)
		{
			float magSq = (p2 - p1).sqrMagnitude;
			if (magSq < float.Epsilon)
				return p1;
			
			float u = ((p.x - p1.x) * (p2.x - p1.x) + (p.y - p1.y) * (p2.y - p1.y)) / magSq;
			if (u < 0.0f || u > 1.0f)
				return p1;
			
			return p1 + (p2 - p1) * u;
		}
		
		void DrawPolygonColliderEditor(Rect r, ref tk2dSpriteColliderIsland[] islands, Texture2D tex, bool forceClosed)
		{
			Vector2 origin = new Vector2(r.x, r.y);
			Vector3 origin3 = new Vector3(r.x, r.y, 0);
			
			// Sanitize
			if (islands == null || islands.Length == 0 ||
				!islands[0].IsValid())
			{
				islands = new tk2dSpriteColliderIsland[1];
				islands[0] = new tk2dSpriteColliderIsland();
				islands[0].connected = true;
				int w = tex.width;
				int h = tex.height;
				
				Vector2[] p = new Vector2[4];
				p[0] = new Vector2(0, 0);
				p[1] = new Vector2(0, h);
				p[2] = new Vector2(w, h);
				p[3] = new Vector2(w, 0);
				islands[0].points = p;
			}
			
			Color previousHandleColor = Handles.color;
			bool insertPoint = false;
			
			if (Event.current.clickCount == 2 && Event.current.type == EventType.MouseDown)
			{
				insertPoint = true;
				Event.current.Use();
			}
			
			if (r.Contains(Event.current.mousePosition) && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.C)
			{
				Vector2 min = Event.current.mousePosition / editorDisplayScale - new Vector2(16.0f, 16.0f);
				Vector3 max = Event.current.mousePosition / editorDisplayScale + new Vector2(16.0f, 16.0f);
				
				min.x = Mathf.Clamp(min.x, 0, tex.width * editorDisplayScale);
				min.y = Mathf.Clamp(min.y, 0, tex.height * editorDisplayScale);
				max.x = Mathf.Clamp(max.x, 0, tex.width * editorDisplayScale);
				max.y = Mathf.Clamp(max.y, 0, tex.height * editorDisplayScale);
				
				tk2dSpriteColliderIsland island = new tk2dSpriteColliderIsland();
				island.connected = true;
				
				Vector2[] p = new Vector2[4];
				p[0] = new Vector2(min.x, min.y);
				p[1] = new Vector2(min.x, max.y);
				p[2] = new Vector2(max.x, max.y);
				p[3] = new Vector2(max.x, min.y);
				island.points = p;
				
				System.Array.Resize(ref islands, islands.Length + 1);
				islands[islands.Length - 1] = island;
				
				Event.current.Use();
			}
			
			// Draw outline lines
			int deletedIsland = -1;
			for (int islandId = 0; islandId < islands.Length; ++islandId)
			{
				float closestDistanceSq = 1.0e32f;
				Vector2 closestPoint = Vector2.zero;
				int closestPreviousPoint = 0;
				
				var island = islands[islandId];
		
				Handles.color = handleInactiveColor;
	
				Vector2 ov = (island.points.Length>0)?island.points[island.points.Length-1]:Vector2.zero;
				for (int i = 0; i < island.points.Length; ++i)
				{
					Vector2 v = island.points[i];
					
					// Don't draw last connection if its not connected
					if (!island.connected && i == 0)
					{
						ov = v;
						continue;
					}
					
					if (insertPoint)
					{
						Vector2 localMousePosition = (Event.current.mousePosition - origin) / editorDisplayScale;
						Vector2 closestPointToCursor = ClosestPointOnLine(localMousePosition, ov, v);
						float lengthSq = (closestPointToCursor - localMousePosition).sqrMagnitude;
						if (lengthSq < closestDistanceSq)
						{
							closestDistanceSq = lengthSq;
							closestPoint = closestPointToCursor;
							closestPreviousPoint = i;
						}
					}
					
					if (drawColliderNormals)
					{
						Vector2 l = (ov - v).normalized;
						Vector2 n = new Vector2(l.y, -l.x);
						Vector2 c = (v + ov) * 0.5f * editorDisplayScale + origin;
						Handles.DrawLine(c, c + n * 16.0f);
					}
					
					Handles.DrawLine(v * editorDisplayScale + origin, ov * editorDisplayScale + origin);
					ov = v;
				}
				Handles.color = previousHandleColor;
				
				if (insertPoint && closestDistanceSq < 16.0f)
				{
					var tmpList = new List<Vector2>(island.points);
					tmpList.Insert(closestPreviousPoint, closestPoint);
					island.points = tmpList.ToArray();
					HandleUtility.Repaint();
				}
				
				int deletedIndex = -1;
				bool flipIsland = false;
				bool disconnectIsland = false;
				
				Event ev = Event.current;

				for (int i = 0; i < island.points.Length; ++i)
				{
					Vector3 cp = island.points[i];
					int id = "tk2dPolyEditor".GetHashCode() + islandId * 10000 + i;
					cp = (tk2dGuiUtility.Handle(tk2dEditorSkin.MoveHandle, id, cp * editorDisplayScale + origin3, true) - origin) / editorDisplayScale;
					
					if (GUIUtility.keyboardControl == id && ev.type == EventType.KeyDown) {

						switch (ev.keyCode) {
							case KeyCode.Backspace: 
							case KeyCode.Delete: {
								GUIUtility.keyboardControl = 0;
								GUIUtility.hotControl = 0;
								deletedIndex = i;
								ev.Use();
								break;
							}

							case KeyCode.X: {
								GUIUtility.keyboardControl = 0;
								GUIUtility.hotControl = 0;
								deletedIsland = islandId;
								ev.Use();
								break;
							}

							case KeyCode.T: {
								if (!forceClosed) {
									GUIUtility.keyboardControl = 0;
									GUIUtility.hotControl = 0;
									disconnectIsland = true;
									ev.Use();
									}
								break;
							}

							case KeyCode.F: {
								flipIsland = true;
								GUIUtility.keyboardControl = 0;
								GUIUtility.hotControl = 0;
								ev.Use();
								break;
							}

							case KeyCode.Escape: {
								GUIUtility.hotControl = 0;
								GUIUtility.keyboardControl = 0;
								ev.Use();
								break;
							}
						}
					}
					
					cp.x = Mathf.Round(cp.x * 2) / 2.0f; // allow placing point at half texel
					cp.y = Mathf.Round(cp.y * 2) / 2.0f;
					
					// constrain
					cp.x = Mathf.Clamp(cp.x, 0.0f, tex.width);
					cp.y = Mathf.Clamp(cp.y, 0.0f, tex.height);

					tk2dGuiUtility.SetPositionHandleValue(id, new Vector2(cp.x, cp.y));
					
					island.points[i] = cp;
				}
				
				if (flipIsland)
				{
					System.Array.Reverse(island.points);
				}
				
				if (disconnectIsland)
				{
					island.connected = !island.connected;
					if (island.connected && island.points.Length < 3)
					{
						Vector2 pp = (island.points[1] - island.points[0]);
						float l = pp.magnitude;
						pp.Normalize();
						Vector2 nn = new Vector2(pp.y, -pp.x);
						nn.y = Mathf.Clamp(nn.y, 0, tex.height);
						nn.x = Mathf.Clamp(nn.x, 0, tex.width);
						System.Array.Resize(ref island.points, island.points.Length + 1);
						island.points[island.points.Length - 1] = (island.points[0] + island.points[1]) * 0.5f + nn * l * 0.5f;
					}
				}
				
				if (deletedIndex != -1 && 
				    ((island.connected && island.points.Length > 3) ||
				    (!island.connected && island.points.Length > 2)) )
				{
					var tmpList = new List<Vector2>(island.points);
					tmpList.RemoveAt(deletedIndex);
					island.points = tmpList.ToArray();
				}			
			}
			
			// Can't delete the last island
			if (deletedIsland != -1 && islands.Length > 1)
			{
				var tmpIslands = new List<tk2dSpriteColliderIsland>(islands);
				tmpIslands.RemoveAt(deletedIsland);
				islands = tmpIslands.ToArray();
			}
		}		
		
		void DrawCustomBoxColliderEditor(Rect r, tk2dSpriteCollectionDefinition param, Texture2D tex)
		{
			Vector2 origin = new Vector2(r.x, r.y);
			
			// sanitize
			if (param.boxColliderMin == Vector2.zero && param.boxColliderMax == Vector2.zero)
			{
				param.boxColliderMax = new Vector2(tex.width, tex.height);
			}
			
			Vector3[] pt = new Vector3[] {
				new Vector3(param.boxColliderMin.x * editorDisplayScale + origin.x, param.boxColliderMin.y * editorDisplayScale + origin.y, 0.0f),
				new Vector3(param.boxColliderMax.x * editorDisplayScale + origin.x, param.boxColliderMin.y * editorDisplayScale + origin.y, 0.0f),
				new Vector3(param.boxColliderMax.x * editorDisplayScale + origin.x, param.boxColliderMax.y * editorDisplayScale + origin.y, 0.0f),
				new Vector3(param.boxColliderMin.x * editorDisplayScale + origin.x, param.boxColliderMax.y * editorDisplayScale + origin.y, 0.0f),
			};
			Color32 transparentColor = handleInactiveColor;
			transparentColor.a = 10;
			Handles.DrawSolidRectangleWithOutline(pt, transparentColor, handleInactiveColor);
			
			// Draw grab handles
			Vector3 handlePos;
			
			int id = 16433;
			
			// Draw top handle
			handlePos = (pt[0] + pt[1]) * 0.5f;
			handlePos = (tk2dGuiUtility.PositionHandle(id + 0, handlePos) - origin) / editorDisplayScale;
			param.boxColliderMin.y = handlePos.y;
			if (param.boxColliderMin.y > param.boxColliderMax.y) param.boxColliderMin.y = param.boxColliderMax.y;
	
			// Draw bottom handle
			handlePos = (pt[2] + pt[3]) * 0.5f;
			handlePos = (tk2dGuiUtility.PositionHandle(id + 1, handlePos) - origin) / editorDisplayScale;
			param.boxColliderMax.y = handlePos.y;
			if (param.boxColliderMax.y < param.boxColliderMin.y) param.boxColliderMax.y = param.boxColliderMin.y;
	
			// Draw left handle
			handlePos = (pt[0] + pt[3]) * 0.5f;
			handlePos = (tk2dGuiUtility.PositionHandle(id + 2, handlePos) - origin) / editorDisplayScale;
			param.boxColliderMin.x = handlePos.x;
			if (param.boxColliderMin.x > param.boxColliderMax.x) param.boxColliderMin.x = param.boxColliderMax.x;
	
			// Draw right handle
			handlePos = (pt[1] + pt[2]) * 0.5f;
			handlePos = (tk2dGuiUtility.PositionHandle(id + 3, handlePos) - origin) / editorDisplayScale;
			param.boxColliderMax.x = handlePos.x;
			if (param.boxColliderMax.x < param.boxColliderMin.x) param.boxColliderMax.x = param.boxColliderMin.x;
	
			param.boxColliderMax.x = Mathf.Round(param.boxColliderMax.x);
			param.boxColliderMax.y = Mathf.Round(param.boxColliderMax.y);
			param.boxColliderMin.x = Mathf.Round(param.boxColliderMin.x);
			param.boxColliderMin.y = Mathf.Round(param.boxColliderMin.y);		
	
			// constrain
			param.boxColliderMax.x = Mathf.Clamp(param.boxColliderMax.x, 0.0f, tex.width);
			param.boxColliderMax.y = Mathf.Clamp(param.boxColliderMax.y, 0.0f, tex.height);
			param.boxColliderMin.x = Mathf.Clamp(param.boxColliderMin.x, 0.0f, tex.width);
			param.boxColliderMin.y = Mathf.Clamp(param.boxColliderMin.y, 0.0f, tex.height);

			tk2dGuiUtility.SetPositionHandleValue(id + 0, new Vector2(0, param.boxColliderMin.y));
			tk2dGuiUtility.SetPositionHandleValue(id + 1, new Vector2(0, param.boxColliderMax.y));
			tk2dGuiUtility.SetPositionHandleValue(id + 2, new Vector2(param.boxColliderMin.x, 0));
			tk2dGuiUtility.SetPositionHandleValue(id + 3, new Vector2(param.boxColliderMax.x, 0));
		}
		
		void HandleKeys()
		{
			if (GUIUtility.keyboardControl != 0)
				return;
			Event evt = Event.current;
			if (evt.type == EventType.KeyUp && evt.shift)
			{
				Mode newMode = Mode.None;
				switch (evt.keyCode)
				{
					case KeyCode.Q: newMode = Mode.Texture; break;
					case KeyCode.W: newMode = Mode.Anchor; break;
					case KeyCode.E: newMode = Mode.Collider; break;
					case KeyCode.R: newMode = Mode.AttachPoint; break;
					case KeyCode.N: drawColliderNormals = !drawColliderNormals; HandleUtility.Repaint(); break;
				}
				if (newMode != Mode.None)
				{
					mode = newMode;
					evt.Use();
				}
			}
		}

		public Vector2 Rotate(Vector2 v, float angle) {
			float angleRad = angle * Mathf.Deg2Rad;
			float cosa = Mathf.Cos(angleRad);
			float sina = -Mathf.Sin(angleRad);
			return new Vector2( v.x * cosa - v.y * sina, v.x * sina + v.y * cosa );
		}

		public void DrawTextureView(tk2dSpriteCollectionDefinition param, Texture2D texture)
		{
			HandleKeys();

			if (mode == Mode.None)
				mode = Mode.Texture;
			
			// sanity check
			if (editorDisplayScale <= 1.0f) editorDisplayScale = 1.0f;
			
			// mirror data
			currentColliderColor = param.colliderColor;
			
			GUILayout.BeginVertical(tk2dEditorSkin.SC_BodyBackground, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
		
			if (texture == null) 
			{
				// Get somewhere to put the texture...
				GUILayoutUtility.GetRect(128.0f, 128.0f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
			}
			else
			{
				bool allowAnchor = param.anchor == tk2dSpriteCollectionDefinition.Anchor.Custom;
				bool allowCollider = (param.colliderType == tk2dSpriteCollectionDefinition.ColliderType.Polygon ||
					param.colliderType == tk2dSpriteCollectionDefinition.ColliderType.BoxCustom);
				if (mode == Mode.Anchor && !allowAnchor) mode = Mode.Texture;
				if (mode == Mode.Collider && !allowCollider) mode = Mode.Texture;

				Rect rect = GUILayoutUtility.GetRect(128.0f, 128.0f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
				tk2dGrid.Draw(rect);
				
				// middle mouse drag and scroll zoom
				if (rect.Contains(Event.current.mousePosition))
				{
					if (Event.current.type == EventType.MouseDrag && Event.current.button == 2)
					{
						textureScrollPos -= Event.current.delta * editorDisplayScale;
						Event.current.Use();
						HandleUtility.Repaint();
					}
					if (Event.current.type == EventType.ScrollWheel)
					{
						editorDisplayScale -= Event.current.delta.y * 0.03f;
						Event.current.Use();
						HandleUtility.Repaint();
					}
				}
				
				bool alphaBlend = true;
				textureScrollPos = GUI.BeginScrollView(rect, textureScrollPos, 
					new Rect(0, 0, textureBorderPixels * 2 + (texture.width) * editorDisplayScale, textureBorderPixels * 2 + (texture.height) * editorDisplayScale));
				Rect textureRect = new Rect(textureBorderPixels, textureBorderPixels, texture.width * editorDisplayScale, texture.height * editorDisplayScale);
				texture.filterMode = FilterMode.Point;
				GUI.DrawTexture(textureRect, texture, ScaleMode.ScaleAndCrop, alphaBlend);

				if (mode == Mode.Collider)
				{
					if (param.colliderType == tk2dSpriteCollectionDefinition.ColliderType.BoxCustom)
						DrawCustomBoxColliderEditor(textureRect, param, texture);
					if (param.colliderType == tk2dSpriteCollectionDefinition.ColliderType.Polygon)
						DrawPolygonColliderEditor(textureRect, ref param.polyColliderIslands, texture, false);
				}
				
				if (mode == Mode.Texture && param.customSpriteGeometry)
				{
					DrawPolygonColliderEditor(textureRect, ref param.geometryIslands, texture, true);
				}
				
				// Anchor
				if (mode == Mode.Anchor)
				{
					Color lineColor = Color.white;
					Vector2 anchor = new Vector2(param.anchorX, param.anchorY);
					Vector2 origin = new Vector2(textureRect.x, textureRect.y);
					
					int id = 99999;
					anchor = (tk2dGuiUtility.PositionHandle(id, anchor * editorDisplayScale + origin) - origin) / editorDisplayScale;
		
					Color oldColor = Handles.color;
					Handles.color = lineColor;
					float w = Mathf.Max(rect.width, texture.width * editorDisplayScale);
					float h = Mathf.Max(rect.height, texture.height * editorDisplayScale);
					
					Handles.DrawLine(new Vector3(textureRect.x, textureRect.y + anchor.y * editorDisplayScale, 0), new Vector3(textureRect.x + w, textureRect.y + anchor.y * editorDisplayScale, 0));
					Handles.DrawLine(new Vector3(textureRect.x + anchor.x * editorDisplayScale, textureRect.y + 0, 0), new Vector3(textureRect.x + anchor.x * editorDisplayScale, textureRect.y + h, 0));
					Handles.color = oldColor;
		
					// constrain
					param.anchorX = Mathf.Clamp(Mathf.Round(anchor.x), 0.0f, texture.width);
					param.anchorY = Mathf.Clamp(Mathf.Round(anchor.y), 0.0f, texture.height);
					
					tk2dGuiUtility.SetPositionHandleValue(id, new Vector2(param.anchorX, param.anchorY));
					
					HandleUtility.Repaint();			
				}

				if (mode == Mode.AttachPoint) {
					Vector2 origin = new Vector2(textureRect.x, textureRect.y);
					int id = "Mode.AttachPoint".GetHashCode();
					foreach (tk2dSpriteDefinition.AttachPoint ap in param.attachPoints) {
						Vector2 apPosition = new Vector2(ap.position.x, ap.position.y);

						if (showAttachPointSprites) {
							tk2dSpriteCollection.AttachPointTestSprite spriteProxy = null;
							if (SpriteCollection.attachPointTestSprites.TryGetValue(ap.name, out spriteProxy) && spriteProxy.spriteCollection != null &&
								spriteProxy.spriteCollection.IsValidSpriteId(spriteProxy.spriteId)) {
								tk2dSpriteDefinition def = spriteProxy.spriteCollection.inst.spriteDefinitions[ spriteProxy.spriteId ];
								tk2dSpriteThumbnailCache.DrawSpriteTextureInRect( textureRect, def, Color.white, ap.position, ap.angle, new Vector2(editorDisplayScale, editorDisplayScale) );
							}
						}

						Vector2 pos = apPosition * editorDisplayScale + origin;
						GUI.color = Color.clear; // don't actually draw the move handle center
						apPosition = (tk2dGuiUtility.PositionHandle(id, pos) - origin) / editorDisplayScale;
						GUI.color = Color.white;

						float handleSize = 30;
						
						Handles.color = Color.green; Handles.DrawLine(pos, pos - Rotate(Vector2.up, ap.angle) * handleSize);
						Handles.color = Color.red; Handles.DrawLine(pos, pos + Rotate(Vector2.right, ap.angle) * handleSize);

						Handles.color = Color.white;
						Handles.DrawWireDisc(pos, Vector3.forward, handleSize);

						// rotation handle
						Vector2 rotHandlePos = pos + Rotate(Vector2.right, ap.angle) * handleSize;
						Vector2 newRotHandlePos = tk2dGuiUtility.Handle(tk2dEditorSkin.RotateHandle, id + 1, rotHandlePos, false);
						if (newRotHandlePos != rotHandlePos) {
							Vector2 deltaRot = newRotHandlePos - pos;
							float angle = -Mathf.Atan2(deltaRot.y, deltaRot.x) * Mathf.Rad2Deg;
							if (Event.current.control) {
								float snapAmount = Event.current.shift ? 15 : 5;
								angle = Mathf.Floor(angle / snapAmount) * snapAmount;
							}
							else if (!Event.current.shift) {
								angle = Mathf.Floor(angle);
							}
							ap.angle = angle;
						}

						Rect r = new Rect(pos.x + 8, pos.y + 6, 1000, 50);
						GUI.Label( r, ap.name, EditorStyles.whiteMiniLabel );

						ap.position.x = Mathf.Round(apPosition.x);
						ap.position.y = Mathf.Round(apPosition.y);
						tk2dGuiUtility.SetPositionHandleValue(id, new Vector2(ap.position.x, ap.position.y));

						id += 2;
					}
					Handles.color = Color.white;
				}

				if (mode == Mode.Texture) {
					if (param.dice) {
						Handles.color = Color.red;
						Vector3 p1, p2;
						int q, dq;

						p1 = new Vector3(textureRect.x, textureRect.y, 0);
						p2 = new Vector3(textureRect.x, textureRect.y + textureRect.height, 0);
						q = 0;
						dq = param.diceUnitX;
						if (dq > 0) {
							while (q <= texture.width) {
								Handles.DrawLine(p1, p2);
								int q0 = q;
								if (q < texture.width && (q + dq) > texture.width)
									q = texture.width;
								else
									q += dq;
								p1.x += (float)(q - q0) * editorDisplayScale;
								p2.x += (float)(q - q0) * editorDisplayScale;
							}
						}
						p1 = new Vector3(textureRect.x, textureRect.y + textureRect.height, 0);
						p2 = new Vector3(textureRect.x + textureRect.width, textureRect.y + textureRect.height, 0);
						q = 0;
						dq = param.diceUnitY;
						if (dq > 0) {
							while (q <= texture.height) {
								Handles.DrawLine(p1, p2);
								int q0 = q;
								if (q < texture.height && (q + dq) > texture.height)
									q = texture.height;
								else
									q += dq;
								p1.y -= (float)(q - q0) * editorDisplayScale;
								p2.y -= (float)(q - q0) * editorDisplayScale;
							}
						}

						Handles.color = Color.white;
					}
				}

				GUI.EndScrollView();
			}
				
			// Draw toolbar
			DrawToolbar(param, texture);
			
			GUILayout.EndVertical();
		}

		public void DrawToolbar(tk2dSpriteCollectionDefinition param, Texture2D texture)
		{
			bool allowAnchor = param.anchor == tk2dSpriteCollectionDefinition.Anchor.Custom;
			bool allowCollider = (param.colliderType == tk2dSpriteCollectionDefinition.ColliderType.Polygon ||
				param.colliderType == tk2dSpriteCollectionDefinition.ColliderType.BoxCustom);
			bool allowAttachPoint = true;

			GUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
			mode = GUILayout.Toggle((mode == Mode.Texture), new GUIContent("Sprite", "Shift+Q"), EditorStyles.toolbarButton)?Mode.Texture:mode;
			if (allowAnchor)
				mode = GUILayout.Toggle((mode == Mode.Anchor), new GUIContent("Anchor", "Shift+W"), EditorStyles.toolbarButton)?Mode.Anchor:mode;
			if (allowCollider)
				mode = GUILayout.Toggle((mode == Mode.Collider), new GUIContent("Collider", "Shift+E"), EditorStyles.toolbarButton)?Mode.Collider:mode;
			if (allowAttachPoint)
				mode = GUILayout.Toggle((mode == Mode.AttachPoint), new GUIContent("AttachPoint", "Shift+R"), EditorStyles.toolbarButton)?Mode.AttachPoint:mode;
			GUILayout.FlexibleSpace();
			
			if (tk2dGuiUtility.HasActivePositionHandle)
			{
				string str = "X: " + tk2dGuiUtility.ActiveHandlePosition.x + " Y: " + tk2dGuiUtility.ActiveHandlePosition.y;
				GUILayout.Label(str, EditorStyles.toolbarTextField);
			}
			
			if ((mode == Mode.Collider && param.colliderType == tk2dSpriteCollectionDefinition.ColliderType.Polygon) ||
				(mode == Mode.Texture && param.customSpriteGeometry))
			{
				drawColliderNormals = GUILayout.Toggle(drawColliderNormals, new GUIContent("Show Normals", "Shift+N"), EditorStyles.toolbarButton);
			}
			if (mode == Mode.Texture && texture != null) {
				GUILayout.Label(string.Format("W: {0} H: {1}", texture.width, texture.height));
			}
			GUILayout.EndHorizontal();			
		}
		
		public void DrawEmptyTextureView()
		{
			mode = Mode.None;
			GUILayout.FlexibleSpace();
		}
		
		tk2dSpriteDefinition.AttachPoint editingAttachPointName = null;
		bool showAttachPointSprites = false;
		void AttachPointSpriteHandler(tk2dSpriteCollectionData newSpriteCollection, int newSpriteId, object callbackData) {
			string attachPointName = (string)callbackData;
			tk2dSpriteCollection.AttachPointTestSprite proxy = null;
			if (SpriteCollection.attachPointTestSprites.TryGetValue(attachPointName, out proxy)) {
				proxy.spriteCollection = newSpriteCollection;
				proxy.spriteId = newSpriteId;
				HandleUtility.Repaint();
			}
		}

		public void DrawAttachPointInspector(tk2dSpriteCollectionDefinition param, Texture2D texture) {
			// catalog all names
			HashSet<string> apHashSet = new HashSet<string>();
			foreach (tk2dSpriteCollectionDefinition def in SpriteCollection.textureParams) {
				foreach (tk2dSpriteDefinition.AttachPoint currAp in def.attachPoints) {
					apHashSet.Add( currAp.name );
				}
			}
			Dictionary<string, int> apNameLookup = new Dictionary<string, int>();
			List<string> apNames = new List<string>( apHashSet );
			for (int i = 0; i < apNames.Count; ++i) {
				apNameLookup.Add( apNames[i], i );
			}
			apNames.Add( "Create..." );

			int toDelete = -1;
			tk2dSpriteGuiUtility.showOpenEditShortcuts = false;
			tk2dSpriteDefinition.AttachPoint newEditingAttachPointName = editingAttachPointName;
			int apIdx = 0;
			foreach (var ap in param.attachPoints) {
				GUILayout.BeginHorizontal();

				if (editingAttachPointName == ap) {
					if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return) {
						newEditingAttachPointName = null;
						HandleUtility.Repaint();
						GUIUtility.keyboardControl = 0;
					}
					ap.name = GUILayout.TextField(ap.name);
				}
				else {
					int sel = EditorGUILayout.Popup(apNameLookup[ap.name], apNames.ToArray());
					if (sel == apNames.Count - 1) {
						newEditingAttachPointName = ap;
						HandleUtility.Repaint();
					}
					else {
						ap.name = apNames[sel];
					}
				}

				ap.angle = EditorGUILayout.FloatField(ap.angle, GUILayout.Width(45));

				if (GUILayout.Button("x", GUILayout.Width(22))) {
					toDelete = apIdx;
				}
				GUILayout.EndHorizontal();

				if (showAttachPointSprites) {
					bool pushGUIEnabled = GUI.enabled;
					
					string tmpName;
					if (editingAttachPointName != ap) {
						tmpName = ap.name;
					} else {
						tmpName = "";
						GUI.enabled = false;
					}

					tk2dSpriteCollection.AttachPointTestSprite spriteProxy = null;
					if (!SpriteCollection.attachPointTestSprites.TryGetValue(tmpName, out spriteProxy)) {
						spriteProxy = new tk2dSpriteCollection.AttachPointTestSprite();
						SpriteCollection.attachPointTestSprites.Add( tmpName, spriteProxy );
					}

					tk2dSpriteGuiUtility.SpriteSelector( spriteProxy.spriteCollection, spriteProxy.spriteId, AttachPointSpriteHandler, tmpName );
					
					GUI.enabled = pushGUIEnabled;
				}

				editingAttachPointName = newEditingAttachPointName;
				++apIdx;
			}

			if (GUILayout.Button("Add AttachPoint")) {
				// Find an unused attach point name
				string unused = "";
				foreach (string n in apHashSet) {
					bool used = false;
					for (int i = 0; i < param.attachPoints.Count; ++i) {
						if (n == param.attachPoints[i].name) {
							used = true;
							break;
						}
					}
					if (!used) {
						unused = n;
						break;
					}
				}
				tk2dSpriteDefinition.AttachPoint ap = new tk2dSpriteDefinition.AttachPoint();
				ap.name = unused;
				ap.position = Vector3.zero;
				param.attachPoints.Add(ap);

				if (unused == "") {
					editingAttachPointName = ap;
				}
			}

			if (toDelete != -1) {
				param.attachPoints.RemoveAt(toDelete);
				HandleUtility.Repaint();
			}

			showAttachPointSprites = GUILayout.Toggle(showAttachPointSprites, "Preview", "button");
			tk2dSpriteGuiUtility.showOpenEditShortcuts = true;
		}

		public void DrawTextureInspector(tk2dSpriteCollectionDefinition param, Texture2D texture)
		{
			if (mode == Mode.Collider && param.colliderType == tk2dSpriteCollectionDefinition.ColliderType.Polygon)
			{
				param.colliderColor = (tk2dSpriteCollectionDefinition.ColliderColor)EditorGUILayout.EnumPopup("Display Color", param.colliderColor);
				
				tk2dGuiUtility.InfoBox("Points" +
										  "\nClick drag - move point" +
										  "\nClick hold + delete/bkspace - delete point" +
										  "\nDouble click on line - add point", tk2dGuiUtility.WarningLevel.Info);
	
				tk2dGuiUtility.InfoBox("Islands" +
										  "\nClick hold point + X - delete island" +
										  "\nPress C - create island at cursor" + 
							              "\nClick hold point + T - toggle connected" +
							              "\nClick hold point + F - flip island", tk2dGuiUtility.WarningLevel.Info);
			}
			if (mode == Mode.Texture && param.customSpriteGeometry)
			{
				param.colliderColor = (tk2dSpriteCollectionDefinition.ColliderColor)EditorGUILayout.EnumPopup("Display Color", param.colliderColor);

				tk2dGuiUtility.InfoBox("Points" +
										  "\nClick drag - move point" +
										  "\nClick hold + delete/bkspace - delete point" +
										  "\nDouble click on line - add point", tk2dGuiUtility.WarningLevel.Info);
	
				tk2dGuiUtility.InfoBox("Islands" +
										  "\nClick hold point + X - delete island" +
										  "\nPress C - create island at cursor" + 
							              "\nClick hold point + F - flip island", tk2dGuiUtility.WarningLevel.Info);
			}
			if (mode == Mode.AttachPoint) {
				DrawAttachPointInspector( param, texture );
			}
		}
	}
}
