using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandTesting : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Running());
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            CommandManager.instance.Execute("moveCharDemo", "left");
        }
        else if(Input.GetKeyDown(KeyCode.RightArrow))
        {
            CommandManager.instance.Execute("moveCharDemo", "right");
        }
    }

    IEnumerator Running()
    {
        yield return CommandManager.instance.Execute("print");
        yield return CommandManager.instance.Execute("print_1p", "Hello world");
        yield return CommandManager.instance.Execute("print_mp", "For", "the", "Emperor");

        yield return CommandManager.instance.Execute("lambda");
        yield return CommandManager.instance.Execute("lambda_1p", "Hello Warp");
        yield return CommandManager.instance.Execute("lambda_mp", "Death", "to", "False Emperor");

        yield return CommandManager.instance.Execute("process");
        yield return CommandManager.instance.Execute("process_1p", "Hello Tau");
        yield return CommandManager.instance.Execute("process_mp", "For", "the", "Great good");
    }
}
