using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Driver;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using NHibernate;

namespace NhibernateProva
{
    class Program
    {
        static void Main(string[] args) 
        {
            Configuration cfg = new Configuration(); //configurator

            cfg.DataBaseIntegration( x =>
            {
                //the connection string to the database
                x.ConnectionString = "Data Source=DESKTOP-M5RI2E1\\SQLEXPRESS;Initial Catalog=ProvaSQL;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
                x.Driver<SqlClientDriver>();//the sql client
                x.Dialect<MsSql2008Dialect>();//the sql dialect
            });

            cfg.AddAssembly(Assembly.GetExecutingAssembly()); //in the assembly file the program will find mapping files to go from c# into database tables

            
            var SessionFactory = cfg.BuildSessionFactory(); //compiles all the metadata necessary for initializing NHibernate and start a session

            using (var session = SessionFactory.OpenSession())//opening a new session
            { 
                Program program = new Program();;

                while (true)
                {
                    program.PrintCommands();

                    string written = Console.ReadLine();

                    if (written.ToLower().Equals("exit"))
                        break;

                    if (written.ToLower().Equals("clear"))
                    {
                        Console.Clear();
                        continue;
                    }

                    int choice;
                    if (program.IsInt(written))
                        choice = Convert.ToInt16((written));
                    else
                        continue;

                    Console.WriteLine(" ");

                    using (var transaction = session.BeginTransaction())
                    {

                        if (choice == 1)
                            program.C_operation(session);
                        else if (choice == 2)
                            program.R_operation(session);
                        else if (choice == 3)
                            program.U_operation(session);
                        else if (choice == 4)
                            program.D_operation(session);
                        else
                            continue;

                        Console.ReadKey();
                        Console.WriteLine(" ");
                        transaction.Commit();
                    }
                }
               
            }

        }



        private void PrintCommands() 
        {
            Console.WriteLine("Lista comandi:");
            Console.WriteLine("1 per aggiungere uno studente al database.");
            Console.WriteLine("2 per leggere gli studenti nel database.");
            Console.WriteLine("3 per modificare uno studente.");
            Console.WriteLine("4 per eliminare uno studente.");
            
            Console.WriteLine("'exit' per uscire dal programma(se inserito durante una delle operazioni riporterà al menù dei comandi.)");
            Console.WriteLine("'clear' per pulire la console");
        }

        private Student StudentToAdd()
        {
            Console.Write("\n");

            Console.Write("Inserisci il nome dello studente: ");
            string firstName = Console.ReadLine();

            if (firstName.ToLower().Equals("exit"))
                return null;

            Console.Write("Inserisci il cognome dello studente: ");
            string lastName = Console.ReadLine();

            if (lastName.ToLower().Equals("exit"))
                return null;

            Console.Clear();

            Student student = new Student { FirstName = firstName, LastName = lastName };

            return student;
        }

        private Student GetStudentById(ISession session)
        {
            string input;
            do
            {
                Console.Write("Inserisci l'id dello studente: ");
                input = Console.ReadLine();

            } while (!IsInt(input));

            Int16 idNum = Convert.ToInt16(input);

            bool idPresent = false;
            foreach(Int16 id in GetIdList(session)) 
            {
                if (idNum == id) 
                {
                    idPresent = true;
                    break;
                }
            }
            if (!idPresent) 
            {
                Console.WriteLine("id non presente.");
                return null;
            }

            Student student = session.Get<Student>(idNum);//it returns the instance with the id we put

            return student;
        }

        //CRUD Operation
        private void C_operation(ISession session) 
        {
            Student student = new Student();
            student = StudentToAdd();

            if (student != null)
                session.Save(student);
        }
        private void R_operation(ISession session) 
        {
            IList<Student> studentList = session.CreateCriteria<Student>().List<Student>();//it returns a list with all student objects
            foreach (Student student in studentList)
            {
                Console.Write("ID: " + student.ID + " - ");
                Console.Write("Nome: " + student.FirstName + " - ");
                Console.Write("Cognome: " + student.LastName);
                Console.Write("\n");
            }
        }
        private void U_operation(ISession session) 
        {
            Student student = GetStudentById(session);

            if (student == null) 
                return;
            
            Console.Write("Inserisci il nuovo nome: ");
            string newName = Console.ReadLine();
            Console.Write("Inserisci il nuovo cognome: ");
            string newLastName = Console.ReadLine();

            if (ConfirmedFromUser()) 
            {
                student.FirstName = newName;
                student.LastName = newLastName;
                session.Update(student);
            }
        }
        private void D_operation(ISession session) 
        {
            Student student = GetStudentById((ISession)session);

            if (student == null)
                return;
    
            if(ConfirmedFromUser())
                session.Delete(student);
        }


        private Boolean IsInt(string n) 
        {
            int number;
            if (int.TryParse(n, out number))
                return true;
            else return false;
        }

        private List<Int16> GetIdList(ISession session) 
        {
            List<Int16> list = new List<Int16>();
            foreach (var student in session.CreateCriteria<Student>().List<Student>())
            {
                list.Add(student.ID);
            }
            return list;
        }

        //confirmation block
        private Boolean ConfirmedFromUser() 
        {
            string confirmation;
            do
            {
                Console.Write("Sei sicuro?(S o N): ");
                confirmation = Console.ReadLine();

            } while (!confirmation.ToLower().Equals("s") && !confirmation.ToLower().Equals("n"));

            if (confirmation.ToLower().Equals("s"))
                return true;
            else if (confirmation.ToLower().Equals("n"))
                return false;
            else
                return false;
        }

        private void PrintIdList(List<Int16> idList) 
        {
            foreach(Int16 id in idList) 
            {
                Console.WriteLine(id);
            }
            Console.WriteLine();
        }
    


    
    }
}
