using System;
using cw3.Models;
using Microsoft.AspNetCore.Mvc;
using cw5.Services;
using Microsoft.Extensions.Configuration;
using cw7.Services;
using Microsoft.AspNetCore.Authorization;
using cw7.Models;
using cw10.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Linq;

namespace cw3.Controllers
{
    [ApiController]
    [Route("api/students")]

    public class StudentsController : ControllerBase
    {

        private readonly IStudentsDbService _dbService;
        private IJWTauthService _JWTauthService;
        private readonly s16796Context _context;

        public StudentsController(IStudentsDbService dbservice, IConfiguration configuration, IJWTauthService JWTauthService, s16796Context context)
        {
            _context = context;
            _dbService = dbservice;
            _JWTauthService = JWTauthService;
        }

        [HttpGet("{id}")]
        public IActionResult GetStudent(int id)
        {
            return Ok(_dbService.GetStudent(id));
        }

        [HttpGet]
        public IActionResult GetStudents(string orderBy)
        {
            return Ok(_dbService.GetStudents());
        }

        [HttpPost]
        public IActionResult CreateStudent(Studentout student)
        {
            student.indexNumber = $"s{new Random().Next(1, 99999)}";
            return Ok(student);
        }

        [HttpPut("{id}")]
        public IActionResult PutStudent(int id, Studentout student)
        {
            return Ok("Aktualizacja studenta nr " + id + " dokończona.");
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteStudent(int id)
        {
            return Ok("Usuwanie studenta nr " + id + " ukończone.");
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("login")]
        public IActionResult Login(LoginRequest request)
        {
            var token = _JWTauthService.Login(request);
            if (token != null)
            {
                return Ok(token);
            }
            else
            {
                return Unauthorized("Wrong login or password entered");
            }
        }
        [HttpPost]
        [AllowAnonymous]
        [Route("refreshToken")]
        public IActionResult RefreshToken(RefreshRequest request)
        {
            var token = _JWTauthService.RefreshToken(request);
            if (token != null)
            {
                return Ok(token);
            }
            else
            {
                return NotFound("No token like that found in database");
            }
        }

        [HttpGet]
        [Route("entityget")]
        public IActionResult GetStudents() {

            var list = _context.Student.Select(st => new
            {
                IndexNumber = st.IndexNumber,
                FirstName = st.FirstName,
                LastName = st.LastName,
                BirthDate = st.BirthDate,
                IdEnrollment = st.IdEnrollment
            }).ToList();
                       
            return Ok(list);

        }

        [HttpPost("{Ind}")]
        [Route("entityupdate")]
        public IActionResult UpdateStudent([FromQuery] string Ind, [FromBody] Student stud)
        {
            try
            {
                var output = _context.Student.Where(st => st.IndexNumber.Equals(Ind)).FirstOrDefault();
                output.FirstName = stud.FirstName;
                output.LastName = stud.LastName;
                output.BirthDate = stud.BirthDate;
                output.IdEnrollment = stud.IdEnrollment;
                if (stud.Password != null)
                {
                    output.Password = PasswordHasherService.GenerateSaltedHash(stud.Password, output.Salt);
                }
                _context.SaveChanges();
                return Ok("Successfuly edited student!");
            }catch(NullReferenceException ex)
            {
                return BadRequest("No student found");
            }
        }

        [HttpDelete("{Ind}")]
        [Route("entitydelete")]
        public IActionResult DeleteStudent([FromQuery] string Ind)
        {
            var output = _context.Student.Where(st => st.IndexNumber.Equals(Ind)).FirstOrDefault();
            if (output != null)
            {
                _context.Remove(output);
                _context.SaveChanges();
                return Ok("Student removed!");
            }
            else
            {
                return BadRequest("No Student found");
            }
        }

    }
}