using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using OCL;

namespace HarmonyBridge
{
    public class AspectTester
    {
        public static void AddOclTests(Assembly a, string ocls)
        {
            new AspectTester(a, ocls);
        }
        private void CompileOCLs(string ocls)
        {
            var aspects = OclParser.ScanString(ocls);

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

        private AspectTester(Assembly assembly, string ocls)
        {
            _assembly = assembly;
            
            CompileOCLs(ocls);
        }
        
        private readonly Assembly _assembly;

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