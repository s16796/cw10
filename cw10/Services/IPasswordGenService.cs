using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cw7.Services
{
    interface IPasswordGenService
    {
        byte[] GenerateHashValue(string pswd, byte[] salt);
        string GenerateSaltedHash(string pswd, byte[] salt);
        bool Verify(string passrec, string pass, byte[] salt);
    }
}
