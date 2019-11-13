
using System;
using Designer;
using HarmonyLib;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace HookClass_Designer
{
public class OverlappingProdTime
    {
private static ConditionalWeakTable<object, object> oset = new ConditionalWeakTable<object, object>();
        public OverlappingProdTime() {}
        public Tuple<HarmonyLib.Harmony, MethodInfo, HarmonyMethod, HarmonyMethod> Apply(System.Type ctx)
        {
            if (ctx == null)
                throw new Exception("[OverlappingProdTime] ctx is null!");
            var harmony = new HarmonyLib.Harmony("OverlappingProdTime");
            var original = ctx.GetMethod("SetTask");
            if (original == null) 
                throw new Exception("[OverlappingProdTime] original method == null.");
            var prefix = typeof(OverlappingProdTime).GetMethod("BeforeCall");
            var postfix = typeof(OverlappingProdTime).GetMethod("AfterCall");
            // harmony.Patch(original, new HarmonyMethod(prefix), new HarmonyMethod(postfix));
            // if (!harmony.HasAnyPatches("OverlappingProdTime"))
            //     throw new Exception("[OverlappingProdTime] applying hook failed.");
            return new Tuple<HarmonyLib.Harmony, MethodInfo, HarmonyMethod, HarmonyMethod>(harmony, original, new HarmonyMethod(prefix), new HarmonyMethod(postfix));
        }

        public static void BeforeCall(Designer.Operation __instance, System.Int32 id, System.Int32 startTime, System.Int32 duration, Designer.Operation predecessor, Designer.Machine machId, System.Collections.Generic.List<System.Tuple<Designer.Material,System.Int32>> requiredMaterials)
        {
            var self = __instance;

            if (!(startTime >= predecessor.EndTime))
            {
                SetPlanningError();
            }

            Type newObjectType = self.GetType();
            object newObject = Activator.CreateInstance(newObjectType);
            foreach (var propInfo in self.GetType().GetProperties())
            {
                object orgValue = propInfo.GetValue(self, null);
                propInfo.SetValue(newObject, orgValue, null);
            }
            oset.Add(self, newObject);
        }
        public static void AfterCall(Designer.Operation __instance, System.Int32 id, System.Int32 startTime, System.Int32 duration, Designer.Operation predecessor, Designer.Machine machId, System.Collections.Generic.List<System.Tuple<Designer.Material,System.Int32>> requiredMaterials)
        {
            var self = __instance;
            object pre;
            oset.TryGetValue(self, out pre);

            if (!(true))
            {
                SetPlanningError();
            }
        }

        public static bool HasPlanningError { get; private set; }
        private static void SetPlanningError()
        {
            Console.WriteLine("Planning Error OverlappingProdTime.");
            HasPlanningError = true;
        }

        
    }


public static class Extensions
{
public static List<dynamic>CastToList(this object self)
{
    return (List<dynamic>) self; // as List<dynamic>;
}

//     public static bool CastAll(this object source, Func<object, bool> predicate)
//     {
// return (source as List<dynamic>).All(predicate);
// }
public static dynamic GetValue(this object instance, string variableName)
        {
            PropertyInfo prop = instance.GetType().GetProperty(variableName,
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty);
            var methInf = prop.GetGetMethod(nonPublic: true);
            var workload = methInf.Invoke(instance, null);
            return workload;
        }
public static List<dynamic>Cast(this object self, Type innerType)
{
    var methodInfo = typeof (Enumerable).GetMethod("Cast");
    var genericMethod = methodInfo.MakeGenericMethod(innerType);
    return genericMethod.Invoke(null, new [] {self}) as List<dynamic>;
}
} 



}
            