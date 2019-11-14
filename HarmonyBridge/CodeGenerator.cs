﻿using System;
using System.IO;
using System.CodeDom.Compiler;
using System.Linq;
using Microsoft.CSharp;
using System.Reflection;
using System.Runtime.InteropServices;
using Designer;
using HarmonyLib;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using OCL;
using System.Collections;

namespace HarmonyBridge
{
    public class CodeGenerator
    {
        public struct Options
        {
            public readonly System.Type Context;
            public readonly string ClassName;
            public readonly string BeforeCode;
            public readonly string AfterCode;
            public readonly string HookedFuncName;

            public Options(string className, System.Type ctx, string hookedFuncName,
                string beforeCode, string afterCode)
            {
                Context = ctx;
                ClassName = className;
                BeforeCode = beforeCode;
                AfterCode = afterCode;
                HookedFuncName = hookedFuncName;
            }
        }

        private readonly Options _options;
        private readonly dynamic _runtimeCode;

        public CodeGenerator(Aspect aspect, Assembly target) : this(new Options(aspect.ConstraintName,
            ContextNameToType(target, aspect.ContextName),
            aspect.FunctionName, aspect.BeforeCode, aspect.AfterCode))
        {
        }

        public CodeGenerator(Options options)
        {
            _options = options;
            // // Console.WriteLine(GenerateCodeString());
            // var param = new CompilerParameters
            // {
            //     GenerateExecutable = false,
            //     IncludeDebugInformation = true,
            //     GenerateInMemory = false
            // };

            var dd = typeof(Enumerable).GetTypeInfo().Assembly.Location;
            var coreDir = Directory.GetParent(dd);
            var refsNames = new[]
            {
                // "System.Xml.dll",
                // "System.Data.dll",
                // "System.Core.dll",
                // "System.Xml.Linq.dll",
                // "0Harmony.dll",
                // "0Harmony.dll",
                typeof(HarmonyLib.Harmony).Assembly.Location,
                typeof(Designer.Operation).Assembly.Location,
                typeof(object).Assembly.Location,
                typeof(System.Linq.Enumerable).Assembly.Location,
                typeof(System.Runtime.CompilerServices.DynamicAttribute).Assembly.Location,
                typeof(Console).GetTypeInfo().Assembly.Location,
                typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo).GetTypeInfo().Assembly.Location,
                coreDir.FullName + Path.DirectorySeparatorChar + "mscorlib.dll",
                coreDir.FullName + Path.DirectorySeparatorChar + "netstandard.dll",
                coreDir.FullName + Path.DirectorySeparatorChar + "System.Runtime.dll",
                coreDir.FullName + Path.DirectorySeparatorChar + "Microsoft.CSharp.dll"
                // "Microsoft.CSharp.dll"
            };
            var refs = refsNames.Select(x => MetadataReference.CreateFromFile(x));

            // param.ReferencedAssemblies.Add("System.dll");
            // param.ReferencedAssemblies.Add("System.Xml.dll");
            // param.ReferencedAssemblies.Add("System.Data.dll");
            // param.ReferencedAssemblies.Add("System.Core.dll");
            // param.ReferencedAssemblies.Add("System.Xml.Linq.dll");
            // param.ReferencedAssemblies.Add("0Harmony.dll");
            // param.ReferencedAssemblies.Add(typeof(HarmonyInstance).Assembly.Location);
            // param.ReferencedAssemblies.Add(typeof(Operation).Assembly.Location);
            // param.ReferencedAssemblies.Add("Microsoft.CSharp.dll");
            // // param.ReferencedAssemblies.Add("Designer.dll"); // new System.Uri(System.Reflection.Assembly.GetExecutingAssembly().EscapedCodeBase).LocalPath);

            // var codeProvider = new CSharpCodeProvider();
            var code = GenerateCodeString();
            // var results = codeProvider.CompileAssemblyFromSource(param, code);

            System.IO.File.WriteAllText("_generated.cs", code);


            var typeName = "HookClass_" + _options.Context.Namespace + "." +
                           _options.ClassName;

            SyntaxTree tree = SyntaxFactory.ParseSyntaxTree(code);

            CSharpCompilation compilation = CSharpCompilation.Create(
                typeName + ".dll",
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                syntaxTrees: new[] {tree},
                references: refs); //new[] {MetadataReference.CreateFromFile(typeof(object).Assembly.Location)});

            using (MemoryStream stream = new MemoryStream())
            {
                Microsoft.CodeAnalysis.Emit.EmitResult compileResult = compilation.Emit(stream);
                if (!compileResult.Success)
                {
                    throw new Exception(compileResult.Diagnostics.Join(x => x.ToString()));
                }

                _assembly = Assembly.Load(stream.GetBuffer());
            }

            // Type calculator = _assembly.GetType(typeName);
            // MethodInfo evaluate = calculator.GetMethod("Evaluate");
            // string answer = evaluate.Invoke(null, null).ToString();


            _instance = _assembly.CreateInstance(typeName);
            _runtimeCode = _instance.GetType();
            // throw new Exception("CodeGen failed!");
        }

        private readonly dynamic _instance;
        private readonly Assembly _assembly;

        private string GetMethodArgumentsList()
        {
            var original = _options.Context.GetMethod(_options.HookedFuncName);
            string args = "";
            if (original == null) return args;
            foreach (var pi in original.GetParameters())
            {
                var pt = pi.ParameterType.ToString();
                int pos = -1;
                while (true)
                {
                    pos = pt.IndexOf("`", StringComparison.Ordinal);
                    if (pos > 0)
                        pt = pt.Remove(pos, 2);
                    else break;
                }

                pos = -1;
                while (true)
                {
                    pos = pt.IndexOf("[", StringComparison.Ordinal);
                    if (pos > 0)
                        pt = pt.Substring(0, pos) + "<" + pt.Substring(pos + 1);
                    else break;
                }

                pos = -1;
                while (true)
                {
                    pos = pt.IndexOf("]", StringComparison.Ordinal);
                    if (pos > 0)
                        pt = pt.Substring(0, pos) + ">" + pt.Substring(pos + 1);
                    else break;
                }
                // if (!pt.StartsWith("System.Int"))
                // {
                //     if (pt.StartsWith("List<"))
                //     {
                //         pt = "List<dynamic>";
                //     }
                //     else
                //         pt = "dynamic";
                // }

                args += ", " + pt + " " + pi.Name;
            }

            return args;
        }

        private static System.Type ContextNameToType(Assembly assembly, string ctxName)
        {
            //return assembly.GetType(ctxName, true);
            return assembly.GetType(assembly.FullName.Split(',')[0] + "." + ctxName, true);
        }

        public void InvokeApplyMethod()
        {
            ResolveEventHandler @object = (object obj, ResolveEventArgs args) => _assembly;
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += @object.Invoke;
            var method = _runtimeCode.GetMethod("Apply");
            Tuple<HarmonyLib.Harmony, MethodInfo, HarmonyMethod, HarmonyMethod> tup = method.Invoke(_instance, new object[]
            {
                _options.Context
            });
            tup.Item1.Patch(tup.Item2, tup.Item3, tup.Item4);
        }

        public bool HasPlanningError
        {
            get
            {
                var method = _runtimeCode.GetProperty("HasPlanningError");
                return method.GetValue(_instance);
            }
        }

        private string GenerateCodeString()
        {
            var funcArgsAddStr = GetMethodArgumentsList();
            return @"
using System;
using Designer;
using HarmonyLib;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace HookClass_" + _options.Context.Namespace + @"
{
public class " + _options.ClassName + @"
    {
private static ConditionalWeakTable<object, object> oset = new ConditionalWeakTable<object, object>();
        public " + _options.ClassName + @"() {}
        public Tuple<HarmonyLib.Harmony, MethodInfo, HarmonyMethod, HarmonyMethod> Apply(System.Type ctx)
        {
            if (ctx == null)
                throw new Exception(""[" + _options.ClassName + @"] ctx is null!"");
            var harmony = new HarmonyLib.Harmony(""" + _options.ClassName + @""");
            var original = ctx.GetMethod(""" + _options.HookedFuncName + @""");
            if (original == null) 
                throw new Exception(""[" + _options.ClassName + @"] original method == null."");
            var prefix = typeof(" + _options.ClassName + @").GetMethod(""BeforeCall"");
            var postfix = typeof(" + _options.ClassName + @").GetMethod(""AfterCall"");
            // harmony.Patch(original, new HarmonyMethod(prefix), new HarmonyMethod(postfix));
            // if (!harmony.HasAnyPatches(""" + _options.ClassName + @"""))
            //     throw new Exception(""[" + _options.ClassName + @"] applying hook failed."");
            return new Tuple<HarmonyLib.Harmony, MethodInfo, HarmonyMethod, HarmonyMethod>(harmony, original, new HarmonyMethod(prefix), new HarmonyMethod(postfix));
        }

        public static void BeforeCall(" + _options.Context + " __instance" + funcArgsAddStr + @")
        {
            var self = __instance;

            if (!(" + _options.BeforeCode + @"))
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
        public static void AfterCall(" + _options.Context + " __instance" + funcArgsAddStr + @")
        {
            var self = __instance;
            object pre;
            oset.TryGetValue(self, out pre);

            if (!(" + _options.AfterCode + @"))
            {
                SetPlanningError();
            }
        }

        public static bool HasPlanningError { get; private set; }
        private static void SetPlanningError()
        {
            Console.WriteLine(""Planning Error " + _options.ClassName + @"."");
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
    var methodInfo = typeof (Enumerable).GetMethod(""Cast"");
    var genericMethod = methodInfo.MakeGenericMethod(innerType);
    return genericMethod.Invoke(null, new [] {self}) as List<dynamic>;
}
} 



}
            ";
        }
    }
}