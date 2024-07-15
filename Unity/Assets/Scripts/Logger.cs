using System;
using System.IO;
using UnityEngine;

public class Logger : MonoBehaviour
{

   public string logNumber = "1";

   public Logger(string logNumber){
      this.logNumber = logNumber;
   }

   public void createLog(string fileName){
      string path = "Assets/Logs/" + fileName + ".txt";
      if (!File.Exists(path))
      {
            File.Create(path).Dispose();
      }
   }

   public void log(string fileName, string logMessage){
         string path = "Assets/Logs/" + fileName + "_" + logNumber + ".txt";
        
         using (StreamWriter writer = File.AppendText(path))
         {
            writer.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " - " + logMessage);
         }
   }
}