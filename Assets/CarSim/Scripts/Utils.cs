using UnityEngine;
using System;

namespace CarSim {
    public static class Utils {
        readonly static string textureFileArg = "--texture-file";

        public static string GetArg(string name)
        {
            var args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == name && args.Length > i + 1)
                {
                    return args[i + 1];
                }
            }
            return null;
        }

        public static bool ArgExists(string name)
        {
            var args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == name)
                {
                    return true;
                }
            }
            return false;
        }


        public static bool useTextureFiles() {
            return ArgExists(textureFileArg);
        }

        public static string randomFilePath() {
            return GetArg(textureFileArg);
        }
    }
}
