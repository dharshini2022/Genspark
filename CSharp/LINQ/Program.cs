namespace LINQ
{
    internal class Program
    {
        List<Student> Students = new List<Student>(
            new Student(1,"Name1","4A",45.5f),
            new Student(2,"Name2","8C",89.3f),
            new Student(3,"Name3","9D",90.6f)
        );

        //group by
        var result = Students.GroupBy(s => s.ClassSection).OrderByDescending(s => s.Marks);
       
    //    foreach(var item in result)
    //    {
            
    //    }
        
    }

    class Student
    {
        public int RollNo {get; set;}
        public string Name {get; set;}
        public string ClassSection {get; set;}
        public float Marks {get; set;}

        public Student(int rollNo, string name, string classSection, float marks)
        {
            RollNo = rollNo;
            Name = name;
            ClassSection = classSection;
            Marks = marks;
        }
    }
}