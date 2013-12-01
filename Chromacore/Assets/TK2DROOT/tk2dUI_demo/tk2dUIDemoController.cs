using UnityEngine;
using System.Collections;

/// <summary>
/// Demo for UI
/// </summary>
[AddComponentMenu("2D Toolkit/Demo/tk2dUIDemoController")]
public class tk2dUIDemoController : tk2dUIBaseDemoController
{
    /// <summary>
    /// Button that will change to next page
    /// </summary>
    public tk2dUIItem nextPage;

    /// <summary>
    /// GameObject containing everything in page 1
    /// </summary>
    public GameObject window1;

    /// <summary>
    /// Button that will change to prev page
    /// </summary>
    public tk2dUIItem prevPage;

    /// <summary>
    /// GameObject containing everything in page 2
    /// </summary>
    public GameObject window2;

    /// <summary>
    /// Used to demo progress bar movement
    /// </summary>
    public tk2dUIProgressBar progressBar;
    private float timeSincePageStart = 0;
    private const float TIME_TO_COMPLETE_PROGRESS_BAR = 2f;
    private float progressBarChaseVelocity = 0.0f;
    public tk2dUIScrollbar slider;

    private GameObject currWindow;

    void Awake()
    {
        ShowWindow(window1.transform);
        HideWindow(window2.transform);
    }

    void OnEnable()
    {
        nextPage.OnClick += GoToPage2;
        prevPage.OnClick += GoToPage1;
    }

    void OnDisable()
    {
        nextPage.OnClick -= GoToPage2;
        prevPage.OnClick -= GoToPage1;
    }


    private void GoToPage1()
    {
        timeSincePageStart = 0;
        AnimateHideWindow(window2.transform);
        AnimateShowWindow(window1.transform);
        currWindow = window1;
    }

    private void GoToPage2()
    {
        timeSincePageStart = 0;
        if (currWindow != window2)
        {
            progressBar.Value = 0;
            currWindow = window2;
            StartCoroutine(MoveProgressBar());
        }
        AnimateHideWindow(window1.transform);
        AnimateShowWindow(window2.transform);
    }

    private IEnumerator MoveProgressBar()
    {
        while (currWindow == window2 && progressBar.Value < 1f)
        {
            progressBar.Value = timeSincePageStart/TIME_TO_COMPLETE_PROGRESS_BAR;
            yield return null;
            timeSincePageStart += tk2dUITime.deltaTime;
        }

        while (currWindow == window2) 
        {
            float smoothTime = 0.5f;
            progressBar.Value = Mathf.SmoothDamp( progressBar.Value, slider.Value, ref progressBarChaseVelocity, smoothTime, Mathf.Infinity, tk2dUITime.deltaTime );

            yield return 0;
        }
    }
}
