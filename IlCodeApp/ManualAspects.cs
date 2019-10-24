using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Designer;
using Harmony;

namespace HarmonyBridge
{
    public class Tools
    {
        //Help method for getting value of private variables
        public static dynamic GetValue(dynamic instance, string variableName)
        {
            PropertyInfo prop = instance.GetType().GetProperty(variableName,
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty);
            var methInf = prop.GetGetMethod(nonPublic: true);
            var workload = methInf.Invoke(instance, null);
            return workload;
        }
    }

    public class DurationNotNegativ
    {
        //private  static  object PrefixRuntime = CodeGenerator.Gernerate();

        /// <summary>
        /// Applying Aspects to a Methods
        /// </summary>
        /// <returns>return true if correct applied</returns>
        public static bool Apply()
        {
            /// Create a Harmony Instance that has an unique name
            var harmony = HarmonyInstance.Create("DurationNotNegativ");

            //harmony.PatchAll(Assembly.GetExecutingAssembly());

            /// get the method to override
            var original = typeof(Operation).GetMethod("SetTask");

            /// gather the methodInfos for patching
            var prefix = typeof(DurationNotNegativ).GetMethod("BeforeCall");
            var postfix = typeof(DurationNotNegativ).GetMethod("AfterCall");

            /// Patch the method (Apply the aspect)
            harmony.Patch(original, new HarmonyMethod(prefix), new HarmonyMethod(postfix));

            /// Check if methods are applied and return result
            return harmony.HasAnyPatches("DurationNotNegativ");
        }

        public static void BeforeCall(int id, int duration)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            //CONSTRAINT =>  context: Operation inv: _duration > 0
            if (duration < 0)
            {
                Console.WriteLine("PLANNING ERROR: Duration time from ID {0} is negativ.", id);
            }
            Console.ForegroundColor = ConsoleColor.White;
        }

        /// <summary>
        /// Aspect after method call, catch the result
        /// </summary>
        /// <param name="__result">Return value of the Parameter</param>
        public static void AfterCall(Operation __instance)
        {
            // Even Private properties are accessable through reflection.
            PropertyInfo strProperty =
            __instance.GetType().GetProperty("EndTime", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty);
            MethodInfo strGetter = strProperty.GetGetMethod(nonPublic: true);
            int val = (int)strGetter.Invoke(__instance, null);
        }
    }

    public class OverlappingProdTime
    {
        public static bool Apply()
        {
            var harmony = HarmonyInstance.Create("OverlappingProdTime");
            var original = typeof(Operation).GetMethod("SetTask");
            var prefix = typeof(OverlappingProdTime).GetMethod("BeforeCall");
            var postfix = typeof(OverlappingProdTime).GetMethod("AfterCall");
            harmony.Patch(original, new HarmonyMethod(prefix), new HarmonyMethod(postfix));

            return harmony.HasAnyPatches("OverlappingProdTime");
        }
        
        public static void BeforeCall(int id, int startTime, Operation predecessor)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            //CONSTRAINT =>  context: Operation inv: _startTime < predecessor.getEndtime()
            if (startTime < Tools.GetValue(predecessor, "EndTime"))
            {
                Console.WriteLine("PLANNING ERROR: Overlapping production times for ID {0} and ID {1}", id,
                    Tools.GetValue(predecessor, "Id").ToString());
            }

            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void AfterCall(object __instance)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            //SPACE FOR CODE
            Console.ForegroundColor = ConsoleColor.White;
        }
    }

    public class StartTimeCollision
    {
        public static bool Apply()
        {
            var harmony = HarmonyInstance.Create("StartTimeCollision");
            var original = typeof(Machine).GetMethod("SetEntry");
            var prefix = typeof(StartTimeCollision).GetMethod("BeforeCall");
            var postfix = typeof(StartTimeCollision).GetMethod("AfterCall");
            harmony.Patch(original, new HarmonyMethod(prefix), new HarmonyMethod(postfix));

            return harmony.HasAnyPatches("StartTimeCollision");
        }

        public static void BeforeCall(Machine __instance, Operation op)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;

            List<Operation> workload = Tools.GetValue(__instance, "Workload");
            int startTimeToAdd = Tools.GetValue(op, "StartTime");

            
            foreach (var o in workload)
            {
                int startTime = Tools.GetValue(o, "StartTime");
                int endTime = Tools.GetValue(o, "EndTime");

                if (startTimeToAdd > startTime && startTimeToAdd < endTime)
                {
                    Console.WriteLine(
                        "PLANNING ERROR: Start time from Operation-ID {0} is within other production time.",
                        Tools.GetValue(op, "Id").ToString());
                }
            }

            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void AfterCall(Machine __instance)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            //SPACE FOR CODE
            Console.ForegroundColor = ConsoleColor.White;
        }
    }

    public class EndTimeCollision
    {
        public static bool Apply()
        {
            var harmony = HarmonyInstance.Create("EndTimeCollision");
            var original = typeof(Machine).GetMethod("SetEntry");
            var prefix = typeof(EndTimeCollision).GetMethod("BeforeCall");
            var postfix = typeof(EndTimeCollision).GetMethod("AfterCall");
            harmony.Patch(original, new HarmonyMethod(prefix), new HarmonyMethod(postfix));

            return harmony.HasAnyPatches("EndTimeCollision");
        }

        public static void BeforeCall(Machine __instance, Operation op)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;

            List<Operation> workload = Tools.GetValue(__instance, "Workload");
            int endTimeToAdd = Tools.GetValue(op, "EndTime");

            foreach (var o in workload)
            {
                int startTime = Tools.GetValue(o, "StartTime");
                int endTime = Tools.GetValue(o, "EndTime");

                if (endTimeToAdd > startTime && endTimeToAdd < endTime)
                {
                    Console.WriteLine("PLANNING ERROR: End time from Operation-ID {0} is within other production time.",
                        Tools.GetValue(op, "Id").ToString());
                }
            }

            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void AfterCall(Machine __instance)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            //SPACE FOR CODE
            Console.ForegroundColor = ConsoleColor.White;
        }
    }

    public class CapacityCheck
    {
        public static bool Apply()
        {
            var harmony = HarmonyInstance.Create("CapacityCheck");
            var original = typeof(Machine).GetMethod("SetEntry");
            var prefix = typeof(CapacityCheck).GetMethod("BeforeCall");
            var postfix = typeof(CapacityCheck).GetMethod("AfterCall");
            harmony.Patch(original, new HarmonyMethod(prefix), new HarmonyMethod(postfix));

            return harmony.HasAnyPatches("CapacityCheck");
        }

        public static void BeforeCall()
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void AfterCall(Machine __instance, Operation op)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            var sum = 0;
            int capaSum = Tools.GetValue(__instance, "Capacity");
            List<Operation> workload = Tools.GetValue(__instance, "Workload");

            foreach (var o in workload)
            {
                int duration = Tools.GetValue(o, "Duration");
                sum = sum + duration;
                if (sum > capaSum)
                {
                    Console.WriteLine("CAPACITY ERROR: Operation {0} cannot be manufactured on Machine {1}",
                        Tools.GetValue(op, "Id").ToString(), Tools.GetValue(__instance, "Name"));
                }
            }


            Console.ForegroundColor = ConsoleColor.White;
        }
    }


    //CONSTRAINT => context Material::SetReservation(op: Operation, q: int) post CheckMaterialQuantity:self.Quantity >= self.ReservationQu->sum()
    public class CheckMaterialQuantity
    {
        public static bool Apply()
        {
            var harmony = HarmonyInstance.Create("CheckMaterialQuantity");
            var original = typeof(Material).GetMethod("AddReservation");
            var prefix = typeof(CheckMaterialQuantity).GetMethod("BeforeCall");
            var postfix = typeof(CheckMaterialQuantity).GetMethod("AfterCall");
            harmony.Patch(original, new HarmonyMethod(prefix), new HarmonyMethod(postfix));
            return harmony.HasAnyPatches("CheckMaterialQuantity");
        }

        public static void BeforeCall(CheckMaterialQuantity __instance)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void AfterCall(CheckMaterialQuantity __instance)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            List<Reservation> Reservations = Tools.GetValue(__instance, "Reservations");
            var quant = Tools.GetValue(__instance, "Quantity");

            if (Reservations.Sum(op => Tools.GetValue(op, "Quantity")) > quant)
            {
                Console.WriteLine("Planning Error: Not enough material {0} left for operation {1}.", Tools.GetValue(__instance, "Id"), Tools.GetValue(Tools.GetValue(Reservations[Reservations.Count - 1], "Operation"),"Id"));
            }

            Console.ForegroundColor = ConsoleColor.White;
        }
    }

    public class CheckProductionTime
    {
        public static bool Apply()
        {
            var harmony = HarmonyInstance.Create("CheckProductionTime");
            var original = typeof(Planner).GetMethod("Plan");
            var prefix = typeof(CheckProductionTime).GetMethod("BeforeCall");
            var postfix = typeof(CheckProductionTime).GetMethod("AfterCall");
            harmony.Patch(original, new HarmonyMethod(prefix), new HarmonyMethod(postfix));
            return harmony.HasAnyPatches("CheckProductionTime");
        }

        public static void BeforeCall(Planner __instance)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            //SPACE FOR CODE
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void AfterCall(Planner __instance)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            var prodTime = Tools.GetValue(__instance, "ProductionTime");
            List<Operation> list = Tools.GetValue(__instance, "Operations");
            if (list.Sum(op=>Tools.GetValue(op, "Duration")) > prodTime)
            {
                Console.WriteLine("Planning Error: Not enough time for planned operations.");
            }

            Console.ForegroundColor = ConsoleColor.White;
        }
    }


    public class CLASS_NAME{ public void METHODE_NAME(){} }

    public class CONSTRAINT_NAME
    {
        public static bool Apply()
        {
            var harmony = HarmonyInstance.Create("CONSTRAINT_NAME");
            var original = typeof(CLASS_NAME).GetMethod("METHODE_NAME");
            var prefix = typeof(CONSTRAINT_NAME).GetMethod("BeforeCall");
            var postfix = typeof(CONSTRAINT_NAME).GetMethod("AfterCall");
            harmony.Patch(original, new HarmonyMethod(prefix), new HarmonyMethod(postfix));
            return harmony.HasAnyPatches("CONSTRAINT_NAME");
        }

        public static void BeforeCall(CLASS_NAME __instance)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            //SPACE FOR CODE
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void AfterCall(CLASS_NAME __instance)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            //SPACE FOR CODE
            Console.ForegroundColor = ConsoleColor.White;
        }
    }

}

