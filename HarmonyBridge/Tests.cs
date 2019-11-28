using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Designer;
using HarmonyBridge;
using OCL;
using Xunit;

namespace AspectTester
{
    public class Tests
    {
        public static void Main(string[] args)
        {
            var test = new Tests();
            test.RunAspectTests();
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
            var aspects = OclParser.ScanFile("../../../Tests.ocl");

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