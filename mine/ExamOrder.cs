using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExamOrder
{
    class Student
    {
        public string ID { get; set; }
    }

    class Course
    {
        public string Name { get; set; }
        public HashSet<Student> Students = new HashSet<Student>();
        public void AddStudent(Student s)
        { Students.Add(s); }
    }

    class ExamBox
    {
        public HashSet<Course> Courses = new HashSet<Course>();
        public bool AddCourse(Course c)
        {
            var students = Courses.SelectMany(x => x.Students).Select(x => x.ID).ToList();
            var pendingStudents = c.Students.Select(x => x.ID).ToList();
            foreach (var s in pendingStudents)
            {
                if (students.IndexOf(s) != -1)
                    return false;
            } // one student cannot take two exam in same time

            Courses.Add(c);
            return true;
        }
    }

    class Exam
    {
        public HashSet<ExamBox> Boxes = new HashSet<ExamBox>();

        public void Arrange(IEnumerable<Course> CourseList)
        {
            var CourseQueue = new Queue<Course>(CourseList);
            int MaxBoxCount = CourseQueue.Count;
            for (int boxIndex = 0; boxIndex < MaxBoxCount; ++boxIndex)
            {
                var box = new ExamBox();

                int PendingCount = CourseQueue.Count;
                for (int i = 0; i < PendingCount; ++i)
                {
                    var CourseToPush = CourseQueue.Dequeue();
                    if (!box.AddCourse(CourseToPush))
                        CourseQueue.Enqueue(CourseToPush); // give it to next box
                }

                if (box.Courses.Count != 0) Boxes.Add(box);
            }
        }
    }

    class ExamOrder
    {
        static void Main(string[] args)
        {
            var Students = new List<string>() { "S1", "S2", "S3", "S4", "S5", "S6", "S7", "S8", "S9", "S10", "S11", "S12", "S13", "S14", "S15", "S16", "S17", "S18", "S19", "S20", "S21", "S22", "S23", "S24", "S25" }
                .Select(x => new Student() { ID = x }).ToList();

            var Courses = new List<string>() { "C1", "C2", "C3", "C4", "C5" }
                .Select(x => new Course() { Name = x }).ToList();

            var rand = new Random();
            foreach (var s in Students)
            {
                for (int i = 0; i < 2; ++i)
                    Courses[rand.Next() % Courses.Count].AddStudent(s);
            }

            var exam = new Exam();
            exam.Arrange(Courses);

            var boxes = exam.Boxes.ToList();
            for (int i = 0; i < boxes.Count; ++i)
            {
                Console.WriteLine("Group {0}:\n----------", i);
                boxes[i].Courses.ToList().ForEach(x => Console.Write(x.Name + " "));
                Console.WriteLine("\n");
            }
            Courses.ForEach(x =>
                {
                    Console.Write(x.Name + ": ");
                    x.Students.ToList().ForEach(y => Console.Write(y.ID + " "));
                    Console.WriteLine();
                });
        }
    }
}
