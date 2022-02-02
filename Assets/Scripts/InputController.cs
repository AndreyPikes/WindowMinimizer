using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    [SerializeField] 
    private UIImageMinimize minimizer;

    [SerializeField] 
    private GameObject objectsOnCanvas;

    [SerializeField] 
    private Canvas canvas;

    public float minimizeTime = 0.5f;
    public float maximizeTime = 0.5f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) 
        {
            MinimizeWindow();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)) 
        {
            MaximizeWindow();
        }
    }

    public void MinimizeWindow()
    {
        StartCoroutine(GetTexture(minimizeTime));
        minimizer.MinimazeWindow(minimizeTime);
    }

    public void MaximizeWindow()
    {
        minimizer.MaximizeWindow(maximizeTime);
        StartCoroutine(SetActive(maximizeTime));
    }

    private IEnumerator GetTexture(float time)
    {
        yield return new WaitForEndOfFrame();
        var screenShot = ScreenCapture.CaptureScreenshotAsTexture();
        minimizer.enabled = true;
        minimizer.texture = screenShot;
        objectsOnCanvas.SetActive(false);
        yield return new WaitForSeconds(time * 1.6f);
        canvas.enabled = false;
        yield return null;
    }

    private IEnumerator SetActive(float time)
    {
        canvas.enabled = true;
        yield return new WaitForSeconds(time * 1.6f);
        objectsOnCanvas.SetActive(true);       
        minimizer.enabled = false;        
        yield return null;
    }
}
