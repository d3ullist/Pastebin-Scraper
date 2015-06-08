using UnityEngine;
using System.Collections;
using System;

public class Initialization : MonoBehaviour 
{
    public void Start()
    {
    }

    public void CreateDesktopFolder(string name)
    {
        var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        if (!(System.IO.Directory.Exists(desktopPath + @"\" + name)))
        {
            System.IO.Directory.CreateDirectory(desktopPath + @"\" + name);
        }
    }
    public void CreateDesktopFile(string name, string type)
    {
        var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        if (!(System.IO.File.Exists(desktopPath + @"\" + name)))
        {
            System.IO.File.Create(desktopPath + @"\" + name + type);
        }
    }
}
