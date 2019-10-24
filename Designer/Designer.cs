using System;
using System.Collections.Generic;

namespace Designer
{
    public class Planner
    {
        private int ProductionTime { get; set; } = 16;  // Zeitspanne für Produktionsplan 

        private List<Operation> Operations { get; } = new List<Operation>();
        private List<Tuple<Material, int>> mats = new List<Tuple<Material, int>>();

        public void Plan()
        {
            
            var ma1 = new Machine(1, "Bohrer", 15);
            var ma2 = new Machine(2, "Fräser", 10);

            var mt1 = new Material(1, "Holzbrett", 15); 
            var mt2 = new Material(2, "Kleber", 50);
            var mt3 = new Material(3, "Schrauben", 50);

            

            Operations.Add(new Operation(0, 0));
            mats = new List<Tuple<Material, int>>
            {
                new Tuple<Material, int>(mt1, 10),
                new Tuple<Material, int>(mt2, 20)
            };
            Operations.Add(new Operation().SetTask(1, 0, 5, Operations[0],ma1, mats));
            Console.WriteLine("\t\tReserve 10 from material Holzbrett");
            Console.WriteLine("\t\tReserve 20 from material Kleber\n");

            mats = new List<Tuple<Material, int>>
            {
                new Tuple<Material, int>(mt1, 10),
                new Tuple<Material, int>(mt3, 20)
            };
            Operations.Add(new Operation().SetTask(2, 5, 10, Operations[1],ma1, mats));
            Console.WriteLine("\t\tReserve 10 from material Holzbrett");
            Console.WriteLine("\t\tReserve 20 from material Schrauben\n");


            mats = new List<Tuple<Material, int>>
            {
                new Tuple<Material, int>(mt3, 30),
                new Tuple<Material, int>(mt2, 10)
            };
            Operations.Add(new Operation().SetTask(3, 14, -5, Operations[2],ma1, mats));
            Console.WriteLine("\t\tReserve 30 from material Schrauben");
            Console.WriteLine("\t\tReserve 20 from material Kleber\n");

        }
    }

    public class Operation
    {
        private int Id { get; set; }
        private int StartTime { get; set; } = 0;
        private int EndTime { get; set; } = 0;
        private int Duration { get; set; } = 0;
        private Operation _predecessor;
        private Machine _machId;
        private List<Tuple<Material, int>> _requiredMaterial { get ; set; }

        public Operation()
        {
        }

        // initial Operation 0
        public Operation(int id, int startTime)
        {
            Id = id;
            StartTime = startTime;
            EndTime = StartTime;
        } 

        public Operation SetTask(int id, int startTime, int duration, Operation predecessor, Machine machId, List<Tuple<Material, int>> requiredMaterials)
        {
            Id = id;
            StartTime = startTime;
            Duration = duration;
            EndTime = StartTime + Duration;
            this._machId = machId;
            this._predecessor = predecessor;

            _requiredMaterial = requiredMaterials;
                           
            Console.WriteLine("setTask::\tOperation id:{0}\tStart Time:{1}\tDuration:{2}\tEnd Time:{3}", Id, StartTime, Duration,EndTime);

            this._machId.SetEntry(this);
            
            foreach (var (mat, requiredCount) in requiredMaterials)
            {
                 mat.AddReservation(new Reservation(this,  requiredCount));
            }
            
            return this;
        }
    }

    public class Machine
    {
        private int Id { get; set; }
        private string Name { get; set; }
        private int Capacity { get; set; }
        private List<Operation> Workload { get; } = new List<Operation>();

        public Machine(int id, string name, int capa)
        {
            Id = id;
            Name = name;
            Capacity = capa;
        }

        public void SetEntry(Operation op)
        {
            Workload.Add(op);
        }
    }

    public class Reservation
    {
        private Operation Operation { get; set; }
        private int Quantity { get; set; }

        public Reservation(Operation operation, int quantity)
        {
            Operation = operation;
            Quantity = quantity;
        }
    }

    public class Material
    {
        private int Id { get; set;  }
        private string Name { get; set; }
        private int Quantity { get; set; }
        
        private List<Reservation> Reservations { get; set; } = new List<Reservation>();
        public Material(int id, string name, int quant)
        {
            Id = id;
            Name = name;
            Quantity = quant;

            Console.WriteLine("Stock for material \t{0}: \t{1}",Name,Quantity);
        }

        public void AddReservation(Reservation res)
        {
            Reservations.Add(res);
        }



    }
}