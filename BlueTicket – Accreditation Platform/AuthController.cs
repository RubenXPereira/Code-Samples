using Acreditation.Functions;
using Acreditation.Models;
using Acreditation.Utils;
using Acreditation.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace Acreditation.Controllers
{
    public class AuthController : ApiController
    {
        public IHttpActionResult Get()
        {
            // Validate the Header and return the token
            string token = ValidateHeader.validate(Request);
            if (token == null)
            {
                return BadRequest(HttpErrorMessages.InvalidHeader);
            }

            // Validate the Token and return 'BadRequest' or 'Unauthorized' if it's the case
            int typeOfAuth = 1;
            HttpStatusCode Status = TokenValidation.validatetoken(token, typeOfAuth);
            IHttpActionResult response = ResponseMessage(new HttpResponseMessage(Status));

            // If the Token is Valid, execute service
            if (Status == HttpStatusCode.OK)
            {
                JWTModel jwtmodel = TokenFactory.getJWTmodelPayload(token);
                string partnerusername = jwtmodel.username;
                string userType = jwtmodel.typeofuser;
                int eventID = jwtmodel.eventid;

                try
                {
                    DataClasses3DataContext dbContext = new DataClasses3DataContext();

                    var partnerid = dbContext.TbPartners.FirstOrDefault(a => a.username == partnerusername);

                    if (jwtmodel.typeofuser == "user" || jwtmodel.typeofuser == "admin")
                    {
                        var sessions = from u in dbContext.TbSessions
                                       where u.Event_Id == eventID  // Multi-event
                                       select u;

                        GateControlModel doorsAndSessions = new GateControlModel();
                        List<SessionModel> listOfSessions = new List<SessionModel>();
                        List<Doors> listOfDoors = new List<Doors>();

                        foreach (var session in sessions)
                        {
                            SessionModel mSession = new SessionModel();
                            mSession.sessionName = session.NameSession;
                            mSession.sessionId = session.Session_Id;

                            listOfSessions.Add(mSession);
                        }

                        var doors = from u1 in dbContext.RelAccessDoors
                                    join u2 in dbContext.TbAccesses on u1.Access_Id_CMS equals u2.AccessIdCms
                                    join u3 in dbContext.TbSessions on u2.Session_Id equals u3.Session_Id
                                    where u3.Event_Id == eventID  // Multi-event
                                    select u1;

                        foreach (var door in doors)
                        {
                            Doors doormodel = new Doors();

                            var matchingvalue = from u in listOfDoors
                                                where u.doorId == door.Door_Id
                                                select u;

                            if (matchingvalue.Count() < 1)
                            {
                                doormodel.doorId = door.Door_Id ?? default(int);
                                doormodel.doorLabel = door.Door_Name;
                                listOfDoors.Add(doormodel);
                            }
                        }

                        doorsAndSessions.doors = listOfDoors;

                        doorsAndSessions.sessions = listOfSessions;

                        return Ok(doorsAndSessions);
                    }
                    return Unauthorized();
                }
                catch (Exception e)
                {
                    return InternalServerError(e);
                }
            }
            else
            {
                return response;
            }
        }

        [ResponseType(typeof(ValidationModel))]
        public IHttpActionResult Post(ValidationModel data)
        {
            // Validate the Header and return the token
            string token = ValidateHeader.validate(Request);
            if (token == null)
            {
                return BadRequest(HttpErrorMessages.InvalidHeader);
            }

            // Validate the Token and return 'BadRequest' or 'Unauthorized' if it's the case
            int typeOfAuth = 1;
            HttpStatusCode Status = TokenValidation.validatetoken(token, typeOfAuth);
            IHttpActionResult response = ResponseMessage(new HttpResponseMessage(Status));

            // If the Token is Valid, execute service
            if (Status == HttpStatusCode.OK)
            {
                int eventID = TokenFactory.getEventIdPayload(token);

                if (data.state != 0 && data.state != 1)
                {
                    System.ArgumentException argEx = new System.ArgumentException("The state is not valid");
                    return InternalServerError(argEx);
                }

                try
                {
                    DataClasses3DataContext dbContext = new DataClasses3DataContext();

                    var tag = (from u in dbContext.TbTags       // Check if the barcode exists
                               where u.BarCode == data.barcode
                               select u).FirstOrDefault();

                    AuthUserModel userdata = new AuthUserModel();

                    if (tag == null)
                    {
                        userdata.status = StatusCodes.TagNotInDatabase;
                        return Ok(userdata);
                    }

                    if (tag.Enabled != true)
                    {
                        userdata.status = StatusCodes.TagNotEnabled;
                        return Ok(userdata);
                    }

                    // Check if EventID requires Registration
                    var currentEvent = (from u in dbContext.TbEvents
                                        where u.EventId == eventID
                                        select u).FirstOrDefault();

                    if (currentEvent != null && currentEvent.RegistrationRequired == true)
                    {
                        // Check if user registered
                        if (tag.TbUser.vipregistered != true)
                        {
                            userdata.status = StatusCodes.UserNotRegistered;
                            return Ok(userdata);
                        }
                    }

                    // If it exists, search if that tag is related to the access given
                    var accesses = from u1 in dbContext.RelAccessDoors
                                   where u1.Door_Id == data.doorId      // REQUIREMENT: DoorID UNIQUE for multi-event situations
                                   join u2 in dbContext.TbAccesses
                                      on u1.Access_Id_CMS equals u2.AccessIdCms
                                   where u2.Session_Id == data.sessionId
                                   join u3 in dbContext.RelTagVouchers
                                      on u2.AccessId equals u3.AccessId
                                   where u3.TagId == tag.TagId
                                   select u3;

                    var doorAccesses = from u in dbContext.RelAccessDoors
                                       where u.Door_Id == data.doorId
                                       select u;

                    if (accesses.Count() > 0 && accesses.Count() >= doorAccesses.Count())
                    {
                        // Ad-Hoc - Porta 9 (Restaurante Staff Parceiros)
                        if (data.doorId == 22)
                        {
                            var entriesAtThisDoor = from u in dbContext.TbLogs
                                                    where u.DoorId == 22 && u.SessionId == data.sessionId
                                                    select u;

                            if (entriesAtThisDoor.Count() >= 70)
                            {
                                userdata.status = StatusCodes.MaxDoorEntrancesReached;
                                return Ok(userdata);
                            }
                        }

                        foreach (var acc in accesses)
                        {
                            if (acc.TbAccess.Reentry == true && acc.State == 1 && data.state == 0)
                            {// If the user tries to exit on a non-reentry zone
                                userdata.status = StatusCodes.ExitNonReentryZone;
                                return Ok(userdata);
                            }

                            if (acc.TbAccess.Reentry == true && acc.State == 1 && data.state == 1)
                            {// If the user tries to enter on a non-reentry zone
                                userdata.status = StatusCodes.NonReentryZone;
                                return Ok(userdata);
                            }

                            if (acc.State == data.state)
                            {// if the user tries to reenter without being granted exit
                                userdata.status = StatusCodes.ReentranceWithoutPreviousExit;
                                return Ok(userdata);
                            }

                            acc.State = data.state;        // update the state (IN or OUT)
                            acc.TimeStamp = DateTime.Now;

                            if (data.state == 1)
                            {
                                // Update
                                tag.HasEntered = true;
                            }

                            try
                            {
                                dbContext.SubmitChanges();
                            }
                            catch (Exception e)
                            {
                                return InternalServerError(e);
                            }

                            var user = (from u in dbContext.TbTags              // Search for the user associated with the tag
                                        where u.TagId == tag.TagId
                                        select u.TbUser).First();

                            userdata.userID = user.UserId;

                            var userdatas = from u in dbContext.TbUsersDatas    // Search for the data associated with that user
                                            where u.UserId == user.UserId
                                            select u;

                            var partner = (from u in dbContext.TbPartners       // Search for the partner associated with that user
                                           where u.PartnerId == user.PartnerId
                                           select u).First();

                            var profile = (from u in dbContext.TbProfiles
                                           where u.ProfileId == user.ProfileId  // Search for the profile associated with that user
                                           select u).First();

                            userdata.profileLabel = partner.PartnerLabel;

                            if (profile.Staff == true) // Checks if user is staff or vip
                                userdata.type = 1;
                            else
                                userdata.type = 0;

                            userdata.partnerLabel = partner.PartnerLabel;

                            foreach (var udata in userdatas) // Fills the json with user info
                            {
                                if (udata.DataName == "Foto")
                                {
                                    if (!string.IsNullOrEmpty(udata.Value))
                                    {
                                        string output = udata.Value.Substring(udata.Value.IndexOf(',') + 1);
                                        userdata.photo = output;
                                    }
                                }

                                if (udata.DataName == "Nome")
                                {
                                    string mName = udata.Value;
                                    userdata.name = mName.Count() > 22 ? mName.Substring(0, 22) + "..." : mName;
                                }

                                userdata.status = StatusCodes.Success;
                            }
                        }

                        // Log Types
                        // 1 - Entrance; 2 - Exit
                        int logType = data.state == 1 ? (int)Enums.LogTypes.Entrance : (int)Enums.LogTypes.Exist;
                        dbContext.InsertLogEntry(eventID, userdata.userID, data.doorId, data.sessionId, logType);

                        return Ok(userdata);
                    }
                    else
                    {
                        userdata.status = StatusCodes.NotAllowed;
                        return Ok(userdata);
                    }
                }
                catch (Exception)
                {
                    AuthUserModel userdata = new AuthUserModel();
                    userdata.status = StatusCodes.GenericError;
                    return Ok(userdata);
                }
            }
            else
            {
                return response;
            }
        }
    }
}