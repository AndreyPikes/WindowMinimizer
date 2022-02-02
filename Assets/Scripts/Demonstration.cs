using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Demonstration : MonoBehaviour
{
    [SerializeField]
    private Text text;

    private int counter = 0;

    public void OnClickCounter()
    {
        counter++;
        text.text = counter.ToString();
    }
}
