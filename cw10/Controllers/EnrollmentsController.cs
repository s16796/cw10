using cw5.Models;
using cw5.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.AspNetCore.Authorization;
using cw10.Models;

namespace cw5.Controllers
{
    [ApiController]
    [Route("api/enrollments")]

    public class EnrollmentsController : ControllerBase
    {

        readonly private IStudentsDbService _dbService;
        readonly private s16796Context _context;

        public EnrollmentsController(IStudentsDbService dbService, s16796Context context)
        {
            _dbService = dbService;
            _context = context;
        }


        [HttpPost]
        //[Authorize(Roles = "employee")]
        public IActionResult AddStudent(EnrollRequest request)
        {

            try
            {
                return Ok(_dbService.EnrollStudent(request, _context));
            }catch(Exception exc)
            {
                return BadRequest(exc.Message);
            }

        }

        [HttpPost]
        [Route("promotions")]
        //[Authorize(Roles = "employee")]
        public IActionResult PromoteStudents(PromoteRequest request)
        {

            try
            {
                return Ok(_dbService.PromoteStudents(request, _context));
            }catch(Exception exc)
            {
                return BadRequest(exc.Message);
            }
         
        }
    }
}
