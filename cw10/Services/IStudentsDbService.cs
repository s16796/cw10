using cw10.Models;
using cw3.Models;
using cw5.Models;
using System;
using System.Collections.Generic;

namespace cw5.Services
{
    public interface IStudentsDbService
    {
        EnrollResponse EnrollStudent(EnrollRequest request, s16796Context context);

        PromoteResponse PromoteStudents(PromoteRequest request, s16796Context context);

        Studentout GetStudent(int id);

        IEnumerable<Studentout> GetStudents();

        Boolean Validate(String index);

    }
}
