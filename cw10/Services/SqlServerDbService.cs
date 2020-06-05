using cw10.Models;
using cw3.Models;
using cw5.Models;
using cw7.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;

namespace cw5.Services
{
    public class SqlServerDbService : IStudentsDbService
    {
        public EnrollResponse EnrollStudent(EnrollRequest request, s16796Context context)
        {
            EnrollResponse enrollResponse = new EnrollResponse();

            context.Database.BeginTransaction();

            var test = context.Student.Where(student => student.IndexNumber.Equals(request.IndexNumber)).FirstOrDefault();
            if(test != null)
            {
            throw new ArgumentException("Index taken!");
            }
            enrollResponse.IndexNumber = request.IndexNumber;

            try
            {
                enrollResponse.IdStudies = context.Studies.Where(studies => studies.Name.Equals(request.Studies)).Select(studies => studies.IdStudy).FirstOrDefault();

            }catch(InvalidOperationException ex)
            {
                context.Database.RollbackTransaction();
                throw new ArgumentException("No studies found with that name");
            }
            enrollResponse.Semester = 1;
            enrollResponse.Studies = request.Studies;

            try
            {
                var output = context.Enrollment.Where(enroll => enroll.Semester == 1 && enroll.IdStudy == enrollResponse.IdStudies).Select(enr => new
                {
                    IdEnrollment = enr.IdEnrollment,
                    StartDate = enr.StartDate
                }).First();
                enrollResponse.IdEnrollment = output.IdEnrollment;
                enrollResponse.StartDate = output.StartDate;
            }catch(InvalidOperationException ex)
            {
                enrollResponse.IdEnrollment = context.Enrollment.Max(enr => enr.IdEnrollment) + 1;
                enrollResponse.StartDate = DateTime.Now.Date;
                var enrollmentadd = new Enrollment()
                {
                    IdEnrollment = enrollResponse.IdEnrollment,
                    Semester = 1,
                    IdStudy = enrollResponse.IdStudies,
                    StartDate = enrollResponse.StartDate
                };
                context.Enrollment.Add(enrollmentadd);
            }

            var studentsalt = GetSalt(32);

            var nowystudent = new Student()
            {
                IndexNumber = request.IndexNumber,
                FirstName = request.FirstName,
                LastName = request.LastName,
                BirthDate = DateTime.ParseExact(request.BirthDate, "dd.MM.yyyy", null),
                IdEnrollment = enrollResponse.IdEnrollment,
                Password = PasswordHasherService.GenerateSaltedHash(request.Password, studentsalt),
                Salt = studentsalt
            };

            context.Student.Add(nowystudent);
            context.SaveChanges();
            context.Database.CommitTransaction();

            return enrollResponse;

         


        }

        public Studentout GetStudent(int id)
        {
            var student = new Studentout();
            using (var client = new SqlConnection("Data Source = db-mssql.pjwstk.edu.pl; Initial Catalog = s16796; Integrated Security = True"))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = client;
                    //indexy od 12 do 20, podawane jako np localhost:44312/api/students/13 da studenta s13
                    command.CommandText = "SELECT * FROM Student st JOIN ENROLLMENT enr ON st.IdEnrollment = enr.IdEnrollment JOIN Studies sts on enr.IdStudy = sts.IdStudy WHERE IndexNumber LIKE '%' + CAST(@id AS varchar)";
                    command.Parameters.AddWithValue("id", id);
                    client.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        student.firstName = reader["FirstName"].ToString();
                        student.lastName = reader["LastName"].ToString();
                        student.BirthDate = Convert.ToDateTime(reader["BirthDate"].ToString());
                        student.indexNumber = reader["IndexNumber"].ToString();
                        student.Study = reader["Name"].ToString();
                        student.Semester = Convert.ToInt32(reader["Semester"].ToString());
                    }
                }
            }
            return student;
        }

        public IEnumerable<Studentout> GetStudents()
        {
            var listofstudents = new List<Studentout>();
            using (var client = new SqlConnection("Data Source = db-mssql.pjwstk.edu.pl; Initial Catalog = s16796; Integrated Security = True"))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = client;
                    command.CommandText = "SELECT * FROM Student st JOIN ENROLLMENT enr ON st.IdEnrollment = enr.IdEnrollment JOIN Studies sts on enr.IdStudy = sts.IdStudy";
                    client.Open();
                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        var st = new Studentout();
                        st.firstName = reader["FirstName"].ToString();
                        st.lastName = reader["LastName"].ToString();
                        st.BirthDate = Convert.ToDateTime(reader["BirthDate"].ToString());
                        st.indexNumber = reader["IndexNumber"].ToString();
                        st.Study = reader["Name"].ToString();
                        st.Semester = Convert.ToInt32(reader["Semester"].ToString());

                        listofstudents.Add(st);
                    }
                }
            }
            return listofstudents;
        }

        public PromoteResponse PromoteStudents(PromoteRequest request, s16796Context context)
        {
            PromoteResponse response = new PromoteResponse();

            var paramStudies = request.Studies;
            var paramSemester = request.Semester;

            var result = context.Enrollment.FromSqlRaw("exec Promote {0}, {1}", paramStudies, paramSemester).AsEnumerable().FirstOrDefault();

            if(result != null)
            {
                response.IdEnrollment = result.IdEnrollment;
                response.IdStudy = result.IdStudy;
                response.Semester = result.Semester;
                response.StartDate = result.StartDate;
                return response;
            }
            else
            {
                throw new ArgumentException("Promotion failed, wrong arguments");
            }

        }

        public bool Validate(string index)
        {
            using (var client = new SqlConnection("Data Source = db-mssql.pjwstk.edu.pl; Initial Catalog = s16796; Integrated Security = True"))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = client;
                    client.Open();
                    command.CommandText = "SELECT * FROM Student st WHERE st.IndexNumber Like @Index";
                    command.Parameters.AddWithValue("Index", index);
                    var dr = command.ExecuteReader();
                    if (dr.Read())
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        private static byte[] GetSalt(int maximumSaltLength)
        {
            var salt = new byte[maximumSaltLength];
            using (var random = new RNGCryptoServiceProvider())
            {
                random.GetNonZeroBytes(salt);
            }

            return salt;
        }
    }
}
