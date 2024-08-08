using System.Diagnostics;
using UnityEngine;
using System.IO;
public class PythonRunner : MonoBehaviour
{
    public void RunPythonScript(string argument)
    {
        // string pythonPath = @"C:\Users\xSkul\AppData\Local\Programs\Python\Python311\python.exe"; // Python exe path
        // string scriptPath = @"C:\Users\xSkul\OneDrive\Documents\Projects\Wheat\wheat\Wheat Simulation\Assets\Scripts\Python\updatejson.py"; // Script path

        string pythonPath = @"C:\Users\Skull\AppData\Local\Programs\Python\Python311\python.exe"; // Python exe path
        string scriptPath = @"C:\Users\Skull\UnityProjects\Wheat\wheat\Wheat Simulation\Assets\Scripts\Python\updatejson.py"; // Script path

        string args = $"\"{scriptPath}\" \"{argument}\"";

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = pythonPath,
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process process = new Process { StartInfo = startInfo })
        {
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (!string.IsNullOrEmpty(output))
            {
                UnityEngine.Debug.Log($"[Python Output] {output}");
            }

            if (!string.IsNullOrEmpty(error))
            {
                UnityEngine.Debug.Log($"[Python Error] {error}");
            }
        }
    }
}
