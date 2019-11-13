using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Designer;
using HarmonyBridge;
using Newtonsoft.Json;
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
            var ocls = new string[]
            {
                "context Operation::SetTask() pre OverlappingProdTime: startTime >= predecessor.EndTime",
                "context Operation::SetTask() pre DurationNotNegativ: duration >= 0",
                // "context Machine::SetEntry(op: Operation) pre StartTimeCollision: self.Workload->forAll(v|v.StartTime < op.StartTime and v.EndTime > op.StartTime)",
                // "context Machine::SetEntry(op: Operation) pre EndTimeCollision: self.Workload->forAll(v|v.StartTime < op.EndTime and v.EndTime > op.EndTime)",
                // "context Machine::SetEntry() post CapacityCheck: self.Workload.collect(wl|wl.Duration).sum() <= self.Capacity"
                // "context Planner::Plan() post CheckProductionTime: self.Operations.collect(wl|wl.Duration).sum() <= self.ProductionTime"
            };
            if (ocls.Count() > 0)
                CompileOCLs(ocls);

            Console.WriteLine("Execute Planning program...");
            var planner = new Planner();
            planner.Plan();

            // Assert.DoesNotContain(gens, gen => gen.HasPlanningError);
        }

        void CompileOCLs(string[] ocls)
        {
            var aspects = new List<Aspect>();
            foreach (var ocl in ocls)
            {
                Console.WriteLine();
                Console.WriteLine("OCL: " + ocl);
                var aspect = Aspect.OclToAspect(ocl);
                Console.WriteLine("Aspect: " + aspect.ToString());
                aspects.Add(aspect);
            }

            Console.WriteLine();

            var gens = new List<CodeGenerator>();
            foreach (var aspect in aspects)
            {
                Console.WriteLine("Generating assembly for " + aspect.ConstraintName + ".");
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