using BepInEx;
using BepInEx.IL2CPP;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Essentials.Helpers
{
    public static class PluginHelpers
    {
        /// <summary>
        /// Gets the "Id" string provided to the first derivative class of <see cref="BasePlugin"/> with the attribute <see cref="BepInPlugin"/> in the current call stack.
        /// </summary>
        /// <returns>A plugin id or <see cref="string.Empty"/></returns>
        public static string GetCallingPluginId(int frameIndex = 3)
        {
            //string pluginId = string.Empty;

            StackTrace stackTrace = new StackTrace(frameIndex);
            for (int i = 0; i < stackTrace.GetFrames().Length; i++)
            {
                MethodBase method = stackTrace.GetFrame(i).GetMethod();
                Type type = method.ReflectedType;

                //EssentialsPlugin.Logger.LogInfo($"Frame {frameIndex + i}: {method.Name}, type: {type}, IsClass: {type.IsClass}, IsPlugin: {type.IsClass && type.IsSubclassOf(typeof(BasePlugin))}");

                if (!type.IsClass || !type.IsSubclassOf(typeof(BasePlugin)) || type.IsAbstract) continue;

                //EssentialsPlugin.Logger.LogInfo($"Frame {frameIndex + i}: {method.Name}, type: {type}");

                //EssentialsPlugin.Logger.LogInfo($"Match: {IL2CPPChainloader.Instance?.Plugins?.Values?.Where(x => x?.Instance?.GetType() == type)?.SingleOrDefault()}");
                //return IL2CPPChainloader.Instance.Plugins.Values.SingleOrDefault(x => x.Instance.GetType() == type)?.Metadata?.GUID ?? string.Empty;

                foreach (CustomAttributeData attribute in type.CustomAttributes)
                {
                    //EssentialsPlugin.Logger.LogInfo($"Attribute {attribute.AttributeType}");

                    if (attribute.AttributeType != typeof(BepInPlugin)) continue;

                    CustomAttributeTypedArgument arg = attribute.ConstructorArguments.FirstOrDefault();
                    if (arg == null || arg.ArgumentType != typeof(string) || arg.Value is not string value) continue;

                    //EssentialsPlugin.Logger.LogInfo($"pluginId: {value}");

                    //pluginId = value;
                    return value;
                }
            }

            return string.Empty;//pluginId;
        }
    }
}