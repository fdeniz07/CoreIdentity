using Microsoft.AspNetCore.Identity;

namespace CoreIdentity.CustomValidations
{
    public class CustomIdentityErrorDescriber:IdentityErrorDescriber
    {
        public override IdentityError InvalidUserName(string userName)
        {
            return new IdentityError()
            {
                Code = "InvalidUserName",
                Description = $"Bu kullanici adi ({userName})  gecersizdir."
            };
        }
        
        public override IdentityError DuplicateUserName(string userName)
        {
            return new IdentityError()
            {
                Code = "DublicateUserName",
                Description = $"Bu kullanici adi ({userName}) zaten kullanilmaktadir"
            };
        }

        public override IdentityError DuplicateEmail(string email)
        {
            return new IdentityError()
            {
                Code = "DublicateEmail",
                Description = $"{email} email adresi kullanilmaktadir"
            };
        }

        public override IdentityError PasswordTooShort(int length)
        {
            return new IdentityError()
            {
                Code = "PasswordToShort",
                Description = $"Sifreniz en az {length} kadar olmalidir"
            }; 
        }
    }
}
