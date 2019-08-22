using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using GuestsLogix.Business.Repository.Routes;
using GuestLogix.Business.Models.Routes;
using System.Data.SqlClient;
using GuestsLogix.Business.Repository.Exseptions;

namespace GuestLogix.WebApi.Areas.Routes
{
    [RoutePrefix("api/Routes")]
    public class RoutesController : ApiController
    {
        private readonly IRoutesRepository _routesRepository;

        public RoutesController(IRoutesRepository routesRepository)
        {
            _routesRepository = routesRepository;
        }

        // GET /api/routes?origin={origi}&destination={destination}
        [HttpGet]
        public HttpResponseMessage GetShortPath([FromUri]string origin, [FromUri]string destination)
        {
            GuestLogix.Business.Models.Routes.Routes result = null;
            try
            {
                 result = _routesRepository.GetRoutes(origin, destination);
                if (result == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "No Route");
                }
            }
            catch (InvalidOriginExseption)
            {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid Origin");  
            }
            catch (InvalidDestinationExseption)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid Destination");
            }
            catch(Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.ServiceUnavailable, ex);
            }
            
            return Request.CreateResponse(HttpStatusCode.OK, result);

        }
    }
}
