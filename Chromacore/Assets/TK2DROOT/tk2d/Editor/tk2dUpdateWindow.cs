using UnityEngine;
using UnityEditor;
using System.Xml;

public class tk2dUpdateWindow : EditorWindow 
{
	bool validUpdateData = false;
	
	class ReleaseInfo
	{
		public double version;
		public int id;
		public int sortId;
		public string url;
		public string changelog;
	}
	
	ReleaseInfo[] releases = null;
	
	string updateInfoUrl = "http://www.2dtoolkit.com/updateinfo.xml";
	string allUpdatesUrl = "http://www.2dtoolkit.com/downloads";
	bool showBetaReleases = false;
	bool showOlderVersions = false;
	
	bool errorState = false;
	string errorMessage = "Click refresh to check for updates.";
	string platformError = "Unable to check for updates when the active build platform is set to WebPlayer.\nSwitch to another build platform to check for updates, or click the button below to manually check for updates on the website.";
	Vector2 scrollPosition = Vector2.zero;
	
	int GetSortId(int id)
	{
		int sortId = 0;
		if (id >= 0) sortId = id + 20001; // final / patches
		else if (id < -10000) sortId = -id - 10000; // alpha
		else sortId = -id + 10000; // beta
		return sortId;
	}

	void OnGUI()
	{
		if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.WebPlayer || 
			EditorUserBuildSettings.activeBuildTarget == BuildTarget.WebPlayerStreamed) {
			GUILayout.Label(platformError);

			GUILayout.Space(25);

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Manually check for updates")) {
				Application.OpenURL(allUpdatesUrl);	
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			return;
		}

		if (validUpdateData)
		{
			
		}
		else
		{
			if (GUILayout.Button("Refresh", GUILayout.MaxWidth(100)))
			{
				try
				{
					errorMessage = "";
					errorState = false;
					
					XmlDocument xmlDocument = new XmlDocument();
					xmlDocument.Load(updateInfoUrl);
	
					validUpdateData = false;
					
					System.Xml.XmlNodeList releaseNodes = xmlDocument.SelectNodes("/updateinfo/release");
					releases = new ReleaseInfo[releaseNodes.Count];
					int currentReleaseNode = 0;
					foreach (XmlNode node in releaseNodes)
					{
						ReleaseInfo releaseInfo = new ReleaseInfo();
						releaseInfo.id = int.Parse(node.Attributes["id"].Value, System.Globalization.NumberFormatInfo.InvariantInfo);
						releaseInfo.sortId = GetSortId(releaseInfo.id);
						releaseInfo.version = double.Parse(node.Attributes["version"].Value, System.Globalization.NumberFormatInfo.InvariantInfo);
						releaseInfo.url = node.Attributes["url"].Value;
						releaseInfo.changelog = node.Attributes["changelog"].Value;
						
						releases[currentReleaseNode] = releaseInfo;
						
						++currentReleaseNode;
					}

					// sort releases, newest first
					System.Array.Sort(releases, (ReleaseInfo a, ReleaseInfo b) =>
					{
						if (a.version == b.version)
						{
							return a.sortId.CompareTo(b.sortId);
						}
						else
						{ 
							return b.version.CompareTo(a.version);
						}
					});
				}
				catch
				{
					errorMessage = "Unable to check for updates.\nPlease contact support if this condition persists.";
					errorState = true;
					releases = null;
				}
			}

			if (errorMessage != "")
			{
				GUILayout.Label(errorMessage);
				
				if (errorState)
				{
					GUILayout.Label("You can also download releases from the website.");
					if (GUILayout.Button("All Releases", GUILayout.MaxWidth(100)))
					{
						Application.OpenURL(allUpdatesUrl);	
					}
				}
			}
			else
			{
				EditorGUILayout.Separator();
				showBetaReleases = EditorGUILayout.Toggle("Beta releases", showBetaReleases);
				showOlderVersions = EditorGUILayout.Toggle("Older versions", showOlderVersions);
				EditorGUILayout.Separator();
				
				int installedSortId = GetSortId(tk2dEditorUtility.releaseId);
				if (releases != null && releases.Length > 0)
				{
					scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
					EditorGUILayout.BeginVertical();
				
					foreach (ReleaseInfo release in releases)
					{
						if (showOlderVersions == false && 
							(release.version < tk2dEditorUtility.version ||
							(release.version == tk2dEditorUtility.version && release.sortId < installedSortId)) )
						{
							// stop displaying releases
							break;
						}
						
						if (!showBetaReleases && release.id < 0)
						{
							// dont display beta releases
							continue;
						}
						
						string label = "";
						label += tk2dEditorUtility.ReleaseStringIdentifier(release.version, release.id);
						if (release.version == tk2dEditorUtility.version && release.id == tk2dEditorUtility.releaseId)
						{
							label += " [INSTALLED]";
						}
						
						GUILayout.BeginHorizontal();
						GUILayout.Label(label, GUILayout.ExpandWidth(true));
						if (GUILayout.Button("Download", GUILayout.MaxWidth(100)))
						{
							Application.OpenURL(release.url);
						}
						if (GUILayout.Button("Changelog", GUILayout.MaxWidth(100)))
						{
							Application.OpenURL(release.changelog);
						}					
						GUILayout.EndHorizontal();
					}				
	
					if (GUILayout.Button("More...", GUILayout.MaxWidth(100)))
					{
						Application.OpenURL(allUpdatesUrl);	
					}
					
					EditorGUILayout.EndVertical();
					EditorGUILayout.EndScrollView();
				}				
			}
		}
	}
	
	
	[MenuItem(tk2dMenu.root + "Check for Updates", false, 10100)]
	public static void ShowUpdaterWindow()
	{
		EditorWindow.GetWindow( typeof(tk2dUpdateWindow), true, "2D Toolkit Updater" );
	}	

}
