using System;
using System.Data.Entity;
using System.Threading.Tasks;
using Imas.Persistence;
using Imas.Domain.Entities;

namespace Imas.Business.Commands
{
    public class AddUserSignature : AbstractCommand
    {
        public AddUserSignature(IDbFacade dbFacade)
            : base(dbFacade)
        { }

        public async Task ExecuteAsync(string userPIN, string signatureImage)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userPIN))
                    throw new ArgumentNullException(nameof(userPIN));

                if (string.IsNullOrWhiteSpace(signatureImage))
                    throw new ArgumentNullException(nameof(signatureImage));

                using (var dbSession = dbFacade.Transaction())
                {
                    var user = await dbSession.Set<User>().FirstOrDefaultAsync(u => u.Pin == userPIN);

                    if (user == null)
                    { throw new Exception("User not found"); }

                    user.Signature = signatureImage;
                    await dbSession.SaveChangesAsync();
                }
            }
            catch (Exception e)
            { throw Oops.Rewrap(e); }
        }
    }
}
