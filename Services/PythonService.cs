using System.Diagnostics;
using System.IO;

namespace InventorySolution.Services
{
    public static class PythonService
    {
        public static void StartApi()
        {
            try
            {
                var baseDir = Directory.GetCurrentDirectory();
                var apiPath = Path.Combine(baseDir, "PythonAPI");
                var batPath = Path.Combine(apiPath, "run_api.bat");

                if (!File.Exists(batPath)) return;

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/C \"{batPath}\"",
                        WorkingDirectory = apiPath,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        CreateNoWindow = true,
                        UseShellExecute = false
                    }
                };
                process.Start();
            }
            catch
            {
                // Error handling (optional)
            }
        }
    }
}