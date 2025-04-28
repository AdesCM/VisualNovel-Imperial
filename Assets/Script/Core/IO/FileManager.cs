using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

public class FileManager
{
    public static List<string> ReadTextFile(string filePath, bool includeBlackLinnes = true)
    {
        if(!filePath.StartsWith('/'))
        {
            filePath = FilePaths.root + filePath;
        }

        List<string> lines = new List<string>();
        try
        {
            using(StreamReader sr = new StreamReader(filePath))
            {
                while(!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    if(includeBlackLinnes || !string.IsNullOrWhiteSpace(line))
                    {
                        lines.Add(line);
                    }
                }
            }

        }
        catch(FileNotFoundException ex)
        {
            Debug.LogError($"File not found: '{ex.FileName}'");
        }

        return lines;
        
    }

    public static List<string> ReadTextAsset(string filePath, bool includeBlackLinnes = true)
    {
        TextAsset asset = Resources.Load<TextAsset>(filePath);
        if(asset == null)
        {
            Debug.LogError($"Asset not found: '{filePath}'");
            return null;
        }

        return ReadTextAsset(asset, includeBlackLinnes);

    }

    public static List<string> ReadTextAsset(TextAsset asset ,bool includeBlackLinnes = true)
    {
        List<string> lines = new List<string>();
        using (StringReader sr = new StringReader(asset.text))
        {
            while(sr.Peek() > -1)
            {
                string line = sr.ReadLine();
                if(includeBlackLinnes || !string.IsNullOrWhiteSpace(line))
                {
                    lines.Add(line);
                }
            }
        }
        return lines;
    }
}
