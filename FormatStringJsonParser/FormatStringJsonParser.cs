using System;
using System.Reflection;
using HarmonyLib;
using FrooxEngine.LogiX.String;
using NeosModLoader;
using Newtonsoft.Json.Linq;

namespace FormatStringJsonParser
{
    public class FormatStringJsonParserMod : NeosMod
    {
        public override string Name => "FormatStringJsonParser";
        public override string Author => "maksim789456";
        public override string Version => "1.0.1";

        public override void OnEngineInit()
        {
            var harmony = new Harmony("me.maksim789456.FormatStringJsonParser");
            harmony.PatchAll();
        }

        [HarmonyPatch]
        public class FormatStringJsonParser
        {
            private const string StartSeq = "{0$";
            private const string EndSeq = "}";

            public static MethodBase TargetMethod() => AccessTools
                .TypeByName("FrooxEngine.LogiX.String.FormatString")
                .GetMethod("OnEvaluate", BindingFlags.Instance | BindingFlags.NonPublic);

            static bool Prefix(FormatString __instance)
            {
                var format = __instance.Format.EvaluateRaw();
                if (!string.IsNullOrWhiteSpace(format) && format.StartsWith(StartSeq) && format.EndsWith(EndSeq) &&
                    __instance.Parameters.Count == 1)
                {
                    JToken token = null;
                    try
                    {
                        var json = JToken.Parse(__instance.Parameters[0].EvaluateRaw().ToString());
                        var path = format.Substring(StartSeq.Length, format.Length - StartSeq.Length - EndSeq.Length);
                        token = json.SelectToken(path);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    __instance.Str.Value = token?.ToString();

                    return false;
                }

                return true;
            }
        }
    }
}