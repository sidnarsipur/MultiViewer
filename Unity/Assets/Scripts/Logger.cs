using System;
using System.IO;
using UnityEngine;

public class Logger : MonoBehaviour
{

   public string logString = "1";
   public bool logging;

   public Logger(string logString, bool logging){
      this.logString = logString;
      this.logging = logging;
   }

   public void createLog(string fileName){
      if(logging){
         Debug.Log("Creating Log File for: " + fileName);

         string path = "Assets/Logs/" + fileName + "_" + logString + ".txt";
         if (!File.Exists(path))
         {
               File.Create(path).Dispose();
         }
      }
   }

   public void log(string fileName, string logMessage){
      if(logging){
         string path = "Assets/Logs/" + fileName + "_" + logString + ".txt";
        
         using (StreamWriter writer = File.AppendText(path))
         {
            writer.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " - " + logMessage);
         }
      }
   }
}