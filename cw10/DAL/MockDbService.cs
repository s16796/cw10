﻿using System.Collections.Generic;
using cw3.Models;

namespace cw3.DAL
{
    public class MockDbService : IDbService
    {
        private static IEnumerable<Studentout> _students;

        static MockDbService()
        {
            _students = new List<Studentout>
            {
                new Studentout{IdStudent = 1, firstName = "Jan", lastName = "Kowalski"},
                new Studentout{IdStudent = 2, firstName = "Anna", lastName = "Malewska"},
                new Studentout{IdStudent = 3, firstName = "Andrzej", lastName = "andrzejewicz"}
            };
        }
            
        public IEnumerable<Studentout> getStudents()
        {
            return _students;
        }
    }
}