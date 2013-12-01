using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace tk2dEditor.SpriteCollectionEditor
{
	public class SpriteView
	{
		enum CustomMeshType {
			Default,
			Diced,
			DoubleSided,
			Custom
		};
		
		const int miniButtonWidth = 45;
		
		public SpriteCollectionProxy SpriteCollection { get { return host.SpriteCollection; } }
		TextureEditor textureEditor;
		
		int[] extraPadAmountValues;
		string[] extraPadAmountLabels;
		
		IEditorHost host;
		public SpriteView(IEditorHost host)
		{
			this.host = host;
			
			int MAX_PAD_AMOUNT = 17;
			extraPadAmountValues = new int[MAX_PAD_AMOUNT];
			extraPadAmountLabels = new string[MAX_PAD_AMOUNT];
			for (int i = 0; i < MAX_PAD_AMOUNT; ++i)
			{
				extraPadAmountValues[i] = i;
				extraPadAmountLabels[i] = (i==0)?"None":(i.ToString());
			}
			
			textureEditor = new TextureEditor(host);
		}
		
		void DrawSpriteEditorMultiView(List<SpriteCollectionEditorEntry> entries)
		{
			var param = SpriteCollection.textureParams[entries[0].index];
			EditorGUILayout.BeginHorizontal();
			
			// texture
			textureEditor.DrawEmptyTextureView();
			
			EditorGUILayout.BeginVertical(tk2dEditorSkin.SC_InspectorBG, GUILayout.MaxWidth(260), GUILayout.ExpandHeight(true));
			if (SpriteCollection.premultipliedAlpha)
			{
				param.additive = EditorGUILayout.Toggle("Additive", param.additive);
			}
			EditorGUILayout.EndVertical();
		}
		
		delegate bool SpriteCollectionEntryComparerDelegate(tk2dSpriteCollectionDefinition a, tk2dSpriteCollectionDefinition b);
		delegate void SpriteCollectionEntryAssignerDelegate(tk2dSpriteCollectionDefinition a, tk2dSpriteCollectionDefinition b);
		void HandleMultiSelection(List<SpriteCollectionEditorEntry> entries, SpriteCollectionEntryComparerDelegate comparer, SpriteCollectionEntryAssignerDelegate assigner)
		{
			if (entries.Count <= 1) return;
			var activeSelection = SpriteCollection.textureParams[entries[entries.Count - 1].index];
			
			bool needButton = false;
			foreach (var entry in entries)
			{
				var sel = SpriteCollection.textureParams[entry.index];
				if (sel != activeSelection && !comparer(activeSelection, sel))
				{
					needButton = true;
					break;
				}
			}
			if (needButton) 
			{ 
				GUILayout.BeginHorizontal(); GUILayout.FlexibleSpace();
				if (GUILayout.Button("Apply", EditorStyles.miniButton, GUILayout.Width(miniButtonWidth)))
				{
					foreach (var entry in entries)
					{
						var sel = SpriteCollection.textureParams[entry.index];
						if (sel != activeSelection) assigner(activeSelection, sel);
					}
				}
				GUILayout.EndHorizontal();
			}
		}
		
		// Only call this when both a AND b have poly colliders and all other comparisons 
		// are successful prior to calling this, its a waste of time otherwise
		bool ComparePolyCollider(tk2dSpriteColliderIsland[] a, tk2dSpriteColliderIsland[] b)
		{
			if (a.Length != b.Length)
				return false;
			for (int i = 0; i < a.Length; ++i)
				if (!a[i].CompareTo(b[i])) return false;
			return true;
		}
		
		void CopyPolyCollider(tk2dSpriteColliderIsland[] src, ref tk2dSpriteColliderIsland[] dest)
		{
			dest = new tk2dSpriteColliderIsland[src.Length];
			for (int i = 0; i < dest.Length; ++i)
			{
				dest[i] = new tk2dSpriteColliderIsland();
				dest[i].CopyFrom(src[i]);
			}
		}
	
		public void DrawSpriteEditorInspector(List<SpriteCollectionEditorEntry> entries, bool allowDelete, bool editingSpriteSheet)
		{
			var entry = entries[entries.Count - 1];
			var param = SpriteCollection.textureParams[entry.index];
			var spriteTexture = param.extractRegion?host.GetTextureForSprite(entry.index):SpriteCollection.textureParams[entry.index].texture;

			// Inspector
			EditorGUILayout.BeginVertical();

			// Header
			EditorGUILayout.BeginVertical(tk2dEditorSkin.SC_InspectorHeaderBG, GUILayout.MaxWidth(host.InspectorWidth), GUILayout.ExpandHeight(true));
			if (entries.Count > 1)
			{
				EditorGUILayout.TextField("Name", param.name);
			}
			else
			{
				string name = EditorGUILayout.TextField("Name", param.name);
				if (name != param.name)
				{
					param.name = name;
					entry.name = name;
					host.OnSpriteCollectionSortChanged();
				}
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Sprite Id");
				EditorGUILayout.SelectableLabel(entry.index.ToString(), EditorStyles.textField, GUILayout.ExpandWidth(true), GUILayout.Height(16));
				EditorGUILayout.EndHorizontal();
			}
			GUILayout.BeginHorizontal();
			bool doDelete = false;
			bool doSelect = false;
			bool doSelectSpriteSheet = false;
			if (entries.Count == 1)
			{
				if (param.extractRegion)
					EditorGUILayout.ObjectField("Texture", spriteTexture, typeof(Texture2D), false);
				else
					SpriteCollection.textureParams[entry.index].texture = EditorGUILayout.ObjectField("Texture", spriteTexture, typeof(Texture2D), false) as Texture2D;
				GUILayout.FlexibleSpace();
				GUILayout.BeginVertical();
				if (editingSpriteSheet && GUILayout.Button("Edit...", EditorStyles.miniButton, GUILayout.Width(miniButtonWidth))) doSelect = true;
				if (!editingSpriteSheet && param.hasSpriteSheetId && GUILayout.Button("Source", EditorStyles.miniButton, GUILayout.Width(miniButtonWidth))) doSelectSpriteSheet = true;
				if (allowDelete && GUILayout.Button("Delete", EditorStyles.miniButton, GUILayout.Width(miniButtonWidth))) doDelete = true;
				GUILayout.EndVertical();
			}
			else
			{
				string countLabel = (entries.Count > 1)?entries.Count.ToString() + " sprites selected":"";
				GUILayout.Label(countLabel);
				GUILayout.FlexibleSpace();
				if (editingSpriteSheet && GUILayout.Button("Edit...", EditorStyles.miniButton, GUILayout.Width(miniButtonWidth))) doSelect = true;
				if (!editingSpriteSheet && param.hasSpriteSheetId)
				{
					int id = param.spriteSheetId;
					foreach (var v in entries)
					{
						var p = SpriteCollection.textureParams[v.index];
						if (!p.hasSpriteSheetId ||
							p.spriteSheetId != id) 
						{ 
							id = -1; 
							break; 
						}
					}
					if (id != -1 && GUILayout.Button("Source", EditorStyles.miniButton, GUILayout.Width(miniButtonWidth))) doSelectSpriteSheet = true;
				}
				if (allowDelete && GUILayout.Button("Delete", EditorStyles.miniButton, GUILayout.Width(miniButtonWidth))) doDelete = true;
			}
			GUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();

			// make dragable
			tk2dPreferences.inst.spriteCollectionInspectorWidth -= (int)tk2dGuiUtility.DragableHandle(4819284, GUILayoutUtility.GetLastRect(), 0, tk2dGuiUtility.DragDirection.Horizontal);

			// Body
			EditorGUILayout.BeginVertical(tk2dEditorSkin.SC_InspectorBG, GUILayout.MaxWidth(host.InspectorWidth), GUILayout.ExpandHeight(true));
			
			if (SpriteCollection.AllowAltMaterials && SpriteCollection.altMaterials.Length > 1)
			{
				List<int> altMaterialIndices = new List<int>();
				List<string> altMaterialNames = new List<string>();
				for (int i = 0; i < SpriteCollection.altMaterials.Length; ++i)
				{
					var mat = SpriteCollection.altMaterials[i];
					if (mat == null) continue;
					altMaterialIndices.Add(i);
					altMaterialNames.Add(mat.name);
				}
				
				GUILayout.BeginHorizontal();
				param.materialId = EditorGUILayout.IntPopup("Material", param.materialId, altMaterialNames.ToArray(), altMaterialIndices.ToArray());
				if (GUILayout.Button("Select", EditorStyles.miniButton, GUILayout.Width(miniButtonWidth)))
				{
					List<int> spriteIdList = new List<int>();
					for (int i = 0; i < SpriteCollection.textureParams.Count; ++i)
						if (SpriteCollection.textureParams[i].materialId == param.materialId)
							spriteIdList.Add(i);
					host.SelectSpritesFromList(spriteIdList.ToArray());
				}
				GUILayout.EndHorizontal();
				HandleMultiSelection(entries, (a,b) => a.materialId == b.materialId, (a,b) => b.materialId = a.materialId);
			}
			
			if (SpriteCollection.premultipliedAlpha)
			{
				param.additive = EditorGUILayout.Toggle("Additive", param.additive);
				HandleMultiSelection(entries, (a,b) => a.additive == b.additive, (a,b) => b.additive = a.additive);
			}
			// fixup
			if (param.scale == Vector3.zero)
				param.scale = Vector3.one;
			param.scale = EditorGUILayout.Vector3Field("Scale", param.scale);
			HandleMultiSelection(entries, (a,b) => a.scale == b.scale, (a,b) => b.scale = a.scale);
			
			// Anchor
			var newAnchor = (tk2dSpriteCollectionDefinition.Anchor)EditorGUILayout.EnumPopup("Anchor", param.anchor);
			if (param.anchor != newAnchor)
			{
				// When anchor type is changed to custom, switch the editor to edit anchors
				if (newAnchor == tk2dSpriteCollectionDefinition.Anchor.Custom)
					textureEditor.SetMode(tk2dEditor.SpriteCollectionEditor.TextureEditor.Mode.Anchor);
				param.anchor = newAnchor;
			}

			if (param.anchor == tk2dSpriteCollectionDefinition.Anchor.Custom)
			{
				EditorGUI.indentLevel++;
				EditorGUILayout.BeginHorizontal();
				param.anchorX = EditorGUILayout.FloatField("X", param.anchorX);
				bool roundAnchorX = GUILayout.Button("R", EditorStyles.miniButton, GUILayout.MaxWidth(24));
				EditorGUILayout.EndHorizontal();
	
				EditorGUILayout.BeginHorizontal();
				param.anchorY = EditorGUILayout.FloatField("Y", param.anchorY);
				bool roundAnchorY = GUILayout.Button("R", EditorStyles.miniButton, GUILayout.MaxWidth(24));
				EditorGUILayout.EndHorizontal();
				
				if (roundAnchorX) param.anchorX = Mathf.Round(param.anchorX);
				if (roundAnchorY) param.anchorY = Mathf.Round(param.anchorY);
				EditorGUI.indentLevel--;
				EditorGUILayout.Separator();
				
				HandleMultiSelection(entries, 
					(a,b) => (a.anchor == b.anchor && a.anchorX == b.anchorX && a.anchorY == b.anchorY),
					delegate(tk2dSpriteCollectionDefinition a, tk2dSpriteCollectionDefinition b) {
						b.anchor = a.anchor;
						b.anchorX = a.anchorX;
						b.anchorY = a.anchorY;
					});				
			}
			else
			{
				HandleMultiSelection(entries, (a,b) => a.anchor == b.anchor, (a,b) => b.anchor = a.anchor);
			}
	
			var newColliderType = (tk2dSpriteCollectionDefinition.ColliderType)EditorGUILayout.EnumPopup("Collider Type", param.colliderType);
			if (param.colliderType != newColliderType)
			{
				// when switching to custom collider mode, automatically switch editor mode
				if (newColliderType == tk2dSpriteCollectionDefinition.ColliderType.BoxCustom ||
					newColliderType == tk2dSpriteCollectionDefinition.ColliderType.Polygon)
					textureEditor.SetMode(tk2dEditor.SpriteCollectionEditor.TextureEditor.Mode.Collider);
				param.colliderType = newColliderType;
			}

			if (param.colliderType == tk2dSpriteCollectionDefinition.ColliderType.BoxCustom)
			{
				EditorGUI.indentLevel++;
				param.boxColliderMin = EditorGUILayout.Vector2Field("Min", param.boxColliderMin);
				param.boxColliderMax = EditorGUILayout.Vector2Field("Max", param.boxColliderMax);
				EditorGUI.indentLevel--;
				EditorGUILayout.Separator();

				HandleMultiSelection(entries, 
					(a,b) => (a.colliderType == b.colliderType && a.boxColliderMin == b.boxColliderMin && a.boxColliderMax == b.boxColliderMax),
					delegate(tk2dSpriteCollectionDefinition a, tk2dSpriteCollectionDefinition b) {
						b.colliderType = a.colliderType;
						b.boxColliderMin = a.boxColliderMin;
						b.boxColliderMax = a.boxColliderMax;
					});				
			}
			else if (param.colliderType == tk2dSpriteCollectionDefinition.ColliderType.Polygon)
			{
				EditorGUI.indentLevel++;
				param.polyColliderCap = (tk2dSpriteCollectionDefinition.PolygonColliderCap)EditorGUILayout.EnumPopup("Collider Cap", param.polyColliderCap);
				param.colliderConvex = EditorGUILayout.Toggle("Convex", param.colliderConvex);
				param.colliderSmoothSphereCollisions = EditorGUILayout.Toggle(new GUIContent("SmoothSphereCollisions", "Smooth Sphere Collisions"), param.colliderSmoothSphereCollisions);
				EditorGUI.indentLevel--;
				EditorGUILayout.Separator();

				HandleMultiSelection(entries, 
					(a,b) => (a.colliderType == b.colliderType && a.polyColliderCap == b.polyColliderCap 
							&& a.colliderConvex == b.colliderConvex && a.colliderSmoothSphereCollisions == b.colliderSmoothSphereCollisions
							&& ComparePolyCollider(a.polyColliderIslands, b.polyColliderIslands)),
					delegate(tk2dSpriteCollectionDefinition a, tk2dSpriteCollectionDefinition b) {
						b.colliderType = a.colliderType;
						b.polyColliderCap = a.polyColliderCap;
						b.colliderConvex = a.colliderConvex;
						b.colliderSmoothSphereCollisions = a.colliderSmoothSphereCollisions;
						CopyPolyCollider(a.polyColliderIslands, ref b.polyColliderIslands);
					});				
			}
			else
			{
				HandleMultiSelection(entries, (a,b) => a.colliderType == b.colliderType, (a,b) => b.colliderType = a.colliderType);				
			}
			
			// Mesh type
			if (param.dice && param.customSpriteGeometry) // sanity check
				{ param.dice = false; param.customSpriteGeometry = false; }
			CustomMeshType meshType = CustomMeshType.Default;
			if (param.customSpriteGeometry) meshType = CustomMeshType.Custom;
			else if (param.dice) meshType = CustomMeshType.Diced;
			else if (param.doubleSidedSprite) meshType = CustomMeshType.DoubleSided;
			CustomMeshType newMeshType = (CustomMeshType)EditorGUILayout.EnumPopup("Render Mesh", meshType);
			if (newMeshType != meshType)
			{
				// Fix up
				param.customSpriteGeometry = false;
				param.dice = false;
				param.doubleSidedSprite = false;

				switch (newMeshType)
				{
				case CustomMeshType.Custom: param.customSpriteGeometry = true; break;
				case CustomMeshType.Diced:	param.dice = true;	break;
				case CustomMeshType.Default: break;
				case CustomMeshType.DoubleSided: param.doubleSidedSprite = true; break;
				}

				// Automatically switch to custom geom edit mode when explicitly switched
				if (param.customSpriteGeometry)
					textureEditor.SetMode(tk2dEditor.SpriteCollectionEditor.TextureEditor.Mode.Texture);
			}

			// Sanity check dicing & multiple atlases
			if (param.dice && SpriteCollection.allowMultipleAtlases)
			{
				EditorUtility.DisplayDialog("Sprite dicing", 
					"Sprite dicing is unavailable when multiple atlases is enabled. " +
					"Please disable it and try again.", "Ok");
				param.dice = false;
			}
			
			// Dicing parameters
			if (param.dice)
			{
				EditorGUI.indentLevel++;
				param.diceUnitX = EditorGUILayout.IntField("Dice X", param.diceUnitX);
				param.diceUnitY = EditorGUILayout.IntField("Dice Y", param.diceUnitY);
				GUIContent diceFilter = new GUIContent("Dice Filter", 
					"Dice Filter lets you dice and only store a subset of the dices. This lets you perform more optimizations, drawing solid dices with a solid shader.\n\n"+
					"Complete - Draw all dices (Default).\nSolidOnly - Only draw the solid dices.\nTransparent Only - Only draw transparent dices.");
				param.diceFilter = (tk2dSpriteCollectionDefinition.DiceFilter)EditorGUILayout.EnumPopup(diceFilter, param.diceFilter);
				EditorGUI.indentLevel--;
				EditorGUILayout.Separator();
			}
			
			HandleMultiSelection(entries, 
				(a,b) => a.customSpriteGeometry == b.customSpriteGeometry && a.dice == b.dice && a.diceUnitX == b.diceUnitX && a.diceUnitY == b.diceUnitY && a.diceFilter == b.diceFilter, 
				delegate(tk2dSpriteCollectionDefinition a, tk2dSpriteCollectionDefinition b) {
					b.customSpriteGeometry = a.customSpriteGeometry;
					b.dice = a.dice;
					b.diceUnitX = a.diceUnitX;
					b.diceUnitY = a.diceUnitY;
					b.diceFilter = a.diceFilter;
					if (a.customSpriteGeometry) {
						CopyPolyCollider(a.geometryIslands, ref b.geometryIslands);
					}
			});
			

			// Disable trimming
			if (!SpriteCollection.disableTrimming)
			{
				param.disableTrimming = EditorGUILayout.Toggle("Disable Trimming", param.disableTrimming);
				HandleMultiSelection(entries, (a,b) => a.disableTrimming == b.disableTrimming, (a,b) => b.disableTrimming = a.disableTrimming);
			}
			
			// Pad amount
			param.pad = (tk2dSpriteCollectionDefinition.Pad)EditorGUILayout.EnumPopup("Pad method", param.pad);
			HandleMultiSelection(entries, (a,b) => a.pad == b.pad, (a,b) => b.pad = a.pad);
			
			// Extra padding
			param.extraPadding = EditorGUILayout.IntPopup("Extra Padding", param.extraPadding, extraPadAmountLabels, extraPadAmountValues);
			HandleMultiSelection(entries, (a,b) => a.extraPadding == b.extraPadding, (a,b) => b.extraPadding = a.extraPadding);
			GUILayout.FlexibleSpace();
			
			// Draw additional inspector
			textureEditor.DrawTextureInspector(param, spriteTexture);
			
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndVertical(); // inspector

			// make dragable
			tk2dPreferences.inst.spriteCollectionInspectorWidth -= (int)tk2dGuiUtility.DragableHandle(4819284, GUILayoutUtility.GetLastRect(), 0, tk2dGuiUtility.DragDirection.Horizontal);
			
			// Defer delete to avoid messing about anything else
			if (doDelete &&
				EditorUtility.DisplayDialog("Delete sprite", "Are you sure you want to delete the selected sprites?", "Yes", "No"))
			{
				foreach (var e in entries)
				{
					SpriteCollection.textureParams[e.index] = new tk2dSpriteCollectionDefinition();
				}
				SpriteCollection.Trim();
				if (editingSpriteSheet)
					host.OnSpriteCollectionChanged(true);
				else
					host.OnSpriteCollectionChanged(false);
			}
			
			if (doSelect)
			{
				List<int> spriteIdList = new List<int>();
				foreach (var e in entries)
					spriteIdList.Add(e.index);
				host.SelectSpritesFromList(spriteIdList.ToArray());
			}
			
			if (doSelectSpriteSheet)
			{
				List<int> spriteIdList = new List<int>();
				foreach (var e in entries)
					spriteIdList.Add(e.index);
				host.SelectSpritesInSpriteSheet(param.spriteSheetId, spriteIdList.ToArray());
			}
		}
		
		public void DrawSpriteSheetView(List<SpriteCollectionEditorEntry> entries)
		{
			if (entries.Count > 1)
			{
				GUILayout.Label("Multi editing sprite sheets not supported");
				return;
			}
			
			//spriteSheetSelection = DrawSpriteSheetView(spriteSheetSelection);
		}

		void DrawSpriteEditorView(List<SpriteCollectionEditorEntry> entries)
		{
			if (entries.Count == 0)
				return;
			var entry = entries[entries.Count - 1];
			var param = SpriteCollection.textureParams[entry.index];
			var spriteTexture = param.extractRegion?host.GetTextureForSprite(entry.index):SpriteCollection.textureParams[entry.index].texture;
			EditorGUILayout.BeginHorizontal();
	
			// Cache texture or draw it
			textureEditor.DrawTextureView(param, spriteTexture);
			DrawSpriteEditorInspector(entries, true, false);
		
			EditorGUILayout.EndHorizontal();
		}
		
		public void Draw(List<SpriteCollectionEditorEntry> entries)
		{
			EditorGUIUtility.LookLikeControls(110.0f, 100.0f);
			
			GUILayout.BeginVertical(GUILayout.ExpandWidth(false));
			if (entries == null || entries.Count == 0)
			{
				GUILayout.Label("");
			}
			else
			{
				var entryType = entries[0].type;
				switch (entryType)
				{
				case SpriteCollectionEditorEntry.Type.Sprite: DrawSpriteEditorView(entries); break;
				case SpriteCollectionEditorEntry.Type.SpriteSheet: DrawSpriteSheetView(entries); break;
				}
			}
			GUILayout.EndVertical();
		}
	}
}
