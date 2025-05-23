using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CMD_DatabaseExtension_Examples : CMD_DatabaseExtension
{
    // 명령어를 추가할 때는 반드시 static으로 추가할 것.
    new public static void Extend(CommandDatabase database)
    {
        //Add command with no parameters
        database.AddCommand("print", new Action(PrintDefaultMessage));
    }

    private static void PrintDefaultMessage()
    {
        Debug.Log("Printing a default message to console");
    }
}
