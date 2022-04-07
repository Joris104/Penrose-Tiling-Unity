using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using TMPro;

public class UIHandler : MonoBehaviour
{
    public GameObject Canvas, ScreenCanvas;
    public GameObject text, screenText, menu;

    public void prepScreenshot(){
        StartCoroutine(screenshot());
    }
    public IEnumerator screenshot(){
        Canvas.SetActive(false);
        ScreenCanvas.SetActive(true);
        screenText.GetComponent<TextMeshProUGUI>().text = text.GetComponent<TextMeshProUGUI>().text;
        yield return new WaitForEndOfFrame();
        ScreenCapture.CaptureScreenshot("Penrose.png");
        ScreenCanvas.SetActive(false);
        Canvas.SetActive(true);
    }

    public void restart(){
        
        SceneManager.LoadScene("SampleScene");
    }

    void Update(){
        if(Input.GetKeyDown("escape")){
            toggleMenu();
        }
    }

    public void toggleMenu(){
        Camera.main.GetComponent<GridController>().activeGame = menu.activeSelf;
        menu.SetActive(!menu.activeSelf);
        
    }

    public void exit(){
        Application.Quit();
    }
    

}
