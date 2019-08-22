using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GuestLogix.Business.Models.Routes;

namespace GuestsLogix.Business.Repository.Routes
{
    public interface IRoutesRepository
    {
        GuestLogix.Business.Models.Routes.Routes GetRoutes(string orign, string derection);
    }
}
