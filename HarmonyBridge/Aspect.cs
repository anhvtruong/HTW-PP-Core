using System;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;

namespace HarmonyBridge
{
    internal class AspectJson
    {
    }

    public class Aspect
    {
        public string ConstraintName;
        public string ContextName;
        public string FunctionName;
        public string BeforeCode;
        public string AfterCode;

        public static Aspect OclToAspect(string ocl)
        {
            var p = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    FileName = "../../../../Parser" + (!IsLinux ? ".exe" : ""),
                    Arguments = @"-d """ + ocl + @""""
                }
            };
            p.Start();
            var json = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            var deserializedAspect = JsonConvert.DeserializeObject<Aspect>(json);
            deserializedAspect.ContextName = "Designer." + deserializedAspect.ContextName;
            return deserializedAspect;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented).ToString();
        }

        public static bool IsLinux
        {
            get
            {
                var p = (int) Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }
    }
}