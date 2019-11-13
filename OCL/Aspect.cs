using System;
using System.Collections;

namespace OCL
{
    public class Aspect
    {
        public int Id { get; set; }
        public string ConstraintName { get; set; }
        public string ContextName { get; set; }
        public string Type { get; set; }
        public string FunctionName { get; set; }
        public ArrayList Code { get; set; } = new ArrayList();
        public string BeforeCode { get; set; }
        public string AfterCode { get; set; }

        public Aspect() { }
        public Aspect(int id, string constraintName, string contextName, string type, string functionName,
                         ArrayList code, string beforeCode, string afterCode)
        {
            Id = id;
            ConstraintName = constraintName;
            ContextName = contextName;
            Type = type;
            FunctionName = functionName;
            Code = code;
            BeforeCode = beforeCode;
            AfterCode = afterCode;
        }

        public void Print()
        {
            Console.WriteLine("Id: {0}", Id);
            Console.WriteLine("ConstraintName: {0}", ConstraintName);
            Console.WriteLine("ContextName: {0}", ContextName);
            Console.WriteLine("Type: {0}", Type);
            Console.WriteLine("FunctionName: {0}", FunctionName);
            Console.Write("Code: "); ArrayPrint(Code);
            Console.WriteLine("BeforeCode: " + BeforeCode);
            Console.WriteLine("AfterCode: "  + AfterCode);
            Console.WriteLine("");
        }
        public string ArrayToString(ArrayList array)
        {
            string s = "";
            foreach (var a in array)
            {
                s += a;
            }
            return s;
        }
        public void ArrayPrint(ArrayList array)
        {
            foreach (var a in array)
            {
                Console.Write(a);
            }
            Console.WriteLine("");
        }
    }
}
