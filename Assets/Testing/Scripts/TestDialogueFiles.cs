using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DIALOGUE;
using System.Data;

public class TestDialogues : MonoBehaviour
{
    [SerializeField] private TextAsset fileToRead = null;
    // Start is called before the first frame update
    void Start()
    {
        StartConversation();
    }

    void StartConversation()
    {
        List<string> lines = FileManager.ReadTextAsset(fileToRead);
        
        foreach(string line in lines)
        {
            if(string.IsNullOrWhiteSpace(line))
            {
                continue;
            }
            DIALOGUE_LINE dl = DialogueParser.Parse(line);

            for(int i = 0; i < dl.commandData. commands.Count; i++)
            {
                DL_COMMAND_DATA.Command command = dl.commandData.commands[i];
                Debug.Log($"Command [{i}] '{command.name}' has arguments [{string.Join(", ", command.arguments)}]");
            }
        }
        //DialogueSystem.instance.Say(lines);
    }
}