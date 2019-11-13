using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Designer;
using HarmonyBridge;
using Newtonsoft.Json;
using OCL;
using Xunit;

namespace AspectTester
{
    public class Tests
    {
        public static void Main(string[] args)
        {
            if (args.Length >= 1 && args[0] == "manual")
            {
                var planner = new Planner();
                Console.WriteLine("Output without Harmony/Aspects");
                planner.Plan();

                Console.ReadLine();
            //     if (!DurationNotNegativ.Apply()) throw new Exception("Applying aspect failed");
            //     if (!OverlappingProdTime.Apply()) throw new Exception("Applying aspect failed");
            //     if (!StartTimeCollision.Apply()) throw new Exception("Applying aspect failed");
            //     if (!EndTimeCollision.Apply()) throw new Exception("Applying aspect failed");
            //     if (!CapacityCheck.Apply()) throw new Exception("Applying aspect failed");
            //     if (!CheckMaterialQuantity.Apply()) throw new Exception("Applying aspect failed");
            //     if (!CheckProductionTime.Apply()) throw new Exception("Applying aspect failed");

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("\nOutput with Harmony/Aspects");
                var planner1 = new Planner();
                planner1.Plan();
            }
            else
            {
                var test = new Tests();
                test.RunAspectTests();
            }
        }

        [Fact]
        private void RunAspectTests()
        {
            CompileOCLs();

            Console.WriteLine("Execute Planning program...");
            var planner = new Planner();
            planner.Plan();

            // Assert.DoesNotContain(gens, gen => gen.HasPlanningError);
        }

        void CompileOCLs()
        {
            var aspects = Test.ScanFile("../../../e1.ocl");

            Console.WriteLine();

            var gens = new List<CodeGenerator>();
            foreach (Aspect aspect in aspects)
            {
                Console.WriteLine("Generating assembly for " + aspect.ConstraintName + ".");
                aspect.Print();
                gens.Add(GenCode(aspect));
            }

            Console.WriteLine();
            Console.WriteLine("Invoking Apply() methods.");
            foreach (var gen in gens)
            {
                gen.InvokeApplyMethod();
            }

            Console.WriteLine();
        }

        public Tests()
        {
            _assembly = typeof(Operation).Assembly;
        }

        private static Assembly _assembly;

        [MethodImpl(MethodImplOptions.NoInlining)]
        private CodeGenerator GenCode(Aspect aspect)
        {
            // Console.WriteLine("Aspect: " + aspect.ToString());

            var gen = new CodeGenerator(
                aspect,
                _assembly
            );
            // gen.InvokeApplyMethod();
            return gen;
        }
    }
}