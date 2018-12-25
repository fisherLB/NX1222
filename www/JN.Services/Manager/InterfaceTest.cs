using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JN.Services.Manager
{
    class InterfaceTest
    {
       
    }

    interface IDrivingLicenceB
    {
        void GetLicence();
    }
    //IDrivingLicenceA接口继承与IDrivingLicenceB
    interface IDrivingLicenceA : IDrivingLicenceB
    {
        new void GetLicence();
    }
    //老师类
    class Teacher : IDrivingLicenceA
    {
        void IDrivingLicenceB.GetLicence()
        {
            Console.WriteLine("老师获得了B类驾驶证！");
        }

        void IDrivingLicenceA.GetLicence()
        {
            Console.WriteLine("老师获得了A类驾驶证");
        }

        public void GetLincence()
        {
            Console.WriteLine("这个不是接口的方法");
        }
    }

    class Student : IDrivingLicenceB
    {
        void IDrivingLicenceB.GetLicence()
        {
            Console.WriteLine("学生获得了B类型驾驶证");
        }
        public void GetLicence()
        {
            Console.WriteLine("这个不是接口方法");
        }

    }

    class Program 
    {
        static void DriveCar(string name, IDrivingLicenceB o)
        {
            IDrivingLicenceB d1 = o as IDrivingLicenceB;
            if (d1 != null)
            {
                d1.GetLicence();
                Console.WriteLine(name + "开动了车");
            }
            else
           {
               Console.WriteLine(name + "美欧驾驶证不能开卡车");
            }
        }

        static void DriveBus(string name, IDrivingLicenceB o)
        {
            IDrivingLicenceA d1 = o as IDrivingLicenceA;
            if (d1 != null)
            {
                d1.GetLicence();
                Console.WriteLine(name + "开动了公共汽车");
            }
            else
            {
                Console.WriteLine(name + "没有驾驶证，不能开公共汽车");
            }
        }

        static void Main(string[] args)
        {
            Teacher t = new Teacher();
            DriveCar("教师", t);
            DriveBus("教师", t);
            Student s = new Student();
            DriveCar("学生", s);
            DriveBus("学生", s);
            Console.ReadKey();
        }

    }

    
    


}
