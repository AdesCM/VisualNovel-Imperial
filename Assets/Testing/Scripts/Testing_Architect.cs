using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DIALOGUE;

namespace TESTING
{
    public class Testing_Architect : MonoBehaviour
    {
        DialogueSystem ds;
        TextArchitect architect;

        public TextArchitect.BuildMethod bm = TextArchitect.BuildMethod.instant;

        string[] lines = new string[5]
        {
            "hihihi",
            "adfadf",
            "adgadg?",
            "adsgadsg?",
            "adgadsg?"
        };
        // Start is called before the first frame update
        void Start()
        {
            ds = DialogueSystem.instance;
            architect = new TextArchitect(ds.dialogueContainer.dialogueText);
            architect.buildMethod = TextArchitect.BuildMethod.fade;
            //architect.speed = 0.5f; 이런식으로 텍스트 속도 줄이기

        }

        // Update is called once per frame
        void Update()
        {

            if (bm != architect.buildMethod)
            {
                architect.buildMethod = bm;
                architect.Stop();
            }

            if(Input.GetKeyDown(KeyCode.S))
            {
                architect.Stop();
            }
            string longLine = "이것은 오래전 이야기. 호랑이가 담배피면서 도넛모양 만들던 시기의 긴 이야기! ";
            if(Input.GetKeyDown(KeyCode.Space))
            {
                if(architect.isBuilding){
                    if(!architect.hurryUp){
                        architect.hurryUp = true;
                    }
                    else{
                        architect.ForceComplete();
                    }
                }
                else{
                    architect.Build(longLine);
                    //architect.Build(lines[Random.Range(0, lines.Length)]);
                }
            }
            else if(Input.GetKeyDown(KeyCode.A))
            {   
                architect.Append(longLine);
                //architect.Append(lines[Random.Range(0, lines.Length)]);
            }
        }
    }  
}
