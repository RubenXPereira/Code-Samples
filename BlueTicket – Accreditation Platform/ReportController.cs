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

namespace Acreditation.Controllers
{
    public class ReportController : ApiController
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
                int eventID = TokenFactory.getEventIdPayload(token);

                try
                {
                    DataClasses3DataContext dbContext = new DataClasses3DataContext();

                    var eventPartners = from u in dbContext.TbPartners
                                        where u.EventId == eventID && u.IsStaff == true // Staff only
                                        orderby u.PartnerLabel ascending
                                        select new
                                        {
                                            PartnerID = u.PartnerId,
                                            PartnerLabel = u.PartnerLabel
                                        };

                    var eventProfiles = from u in dbContext.TbProfiles
                                        where u.EventId == eventID && u.Staff == true // Staff only
                                        orderby u.ProfileLabel ascending
                                        select new
                                        {
                                            ProfileID = u.ProfileId,
                                            ProfileLabel = u.ProfileLabel
                                        };


                    ReportingFilterLists reportingFilterLists = new ReportingFilterLists();


                    List<PartnerReportModel> partnerList = new List<PartnerReportModel>();

                    // Default entry
                    PartnerReportModel partnerReportModelDefault = new PartnerReportModel();
                    partnerReportModelDefault.partnerID = -1;
                    partnerReportModelDefault.partnerLabel = "todos";
                    partnerList.Add(partnerReportModelDefault);

                    foreach (var item1 in eventPartners)
                    {
                        PartnerReportModel partnerReportModel = new PartnerReportModel();
                        partnerReportModel.partnerID = item1.PartnerID;
                        partnerReportModel.partnerLabel = item1.PartnerLabel;
                        partnerList.Add(partnerReportModel);
                    }


                    List<ProfileReportModel> profileList = new List<ProfileReportModel>();

                    // Default entry
                    ProfileReportModel profileReportModelDefault = new ProfileReportModel();
                    profileReportModelDefault.profileID = -1;
                    profileReportModelDefault.profileLabel = "todos";
                    profileList.Add(profileReportModelDefault);
                    
                    foreach (var item2 in eventProfiles)
                    {
                        ProfileReportModel profileReportModel = new ProfileReportModel();
                        profileReportModel.profileID = item2.ProfileID;
                        profileReportModel.profileLabel = item2.ProfileLabel;
                        profileList.Add(profileReportModel);
                    }

                    reportingFilterLists.partnerList = partnerList;
                    reportingFilterLists.profileList = profileList;
                    
                    return Ok(reportingFilterLists);
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

        public IHttpActionResult Get(int partnerID, int profileID)
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

                try
                {
                    DataClasses3DataContext dbContext = new DataClasses3DataContext();

                    var eventTagUniverse = from u1 in dbContext.TbTags
                                           join u2 in dbContext.TbUsers on u1.UserId equals u2.UserId
                                           join u3 in dbContext.TbPartners on u2.PartnerId equals u3.PartnerId
                                           where u3.EventId == eventID && u3.IsStaff == true // Staff only
                                           select new
                                           {
                                               EventTag = u1.TagId,
                                               EventTagStatus = u1.Enabled,
                                               PartnerID = u2.PartnerId,
                                               ProfileID = u2.ProfileId
                                           };

                    int nrTagsPerEvent = eventTagUniverse.Count();
                    int nrAccreditatedTagsPerEvent = -1;
                    int nrUnAccreditatedTagsPerEvent = -1;

                    if (partnerID == -1 && profileID == -1) // Without filters
                    {
                        var accreditations = from u in eventTagUniverse
                                             where u.EventTagStatus == true
                                             select new
                                             {
                                                 EventAccreditatedTag = u.EventTag
                                             };

                        nrAccreditatedTagsPerEvent = accreditations.Count();

                        var nonAccreditations = from u in eventTagUniverse
                                             where u.EventTagStatus == null || u.EventTagStatus == false
                                             select new
                                             {
                                                 EventAccreditatedTag = u.EventTag
                                             };

                        nrUnAccreditatedTagsPerEvent = nonAccreditations.Count();
                    }
                    else
                    {
                        if (partnerID > -1 && profileID == -1) // Filtered for partnerID
                        {
                            var accreditations = from u in eventTagUniverse
                                                 where u.EventTagStatus == true && u.PartnerID == partnerID
                                                 select new
                                                 {
                                                     EventAccreditatedTag = u.EventTag
                                                 };

                            nrAccreditatedTagsPerEvent = accreditations.Count();

                            var nonAccreditations = from u in eventTagUniverse
                                                 where (u.EventTagStatus == null || u.EventTagStatus == false) && u.PartnerID == partnerID
                                                 select new
                                                 {
                                                     EventAccreditatedTag = u.EventTag
                                                 };

                            nrUnAccreditatedTagsPerEvent = nonAccreditations.Count();
                        }
                        else
                        {
                            if (partnerID == -1 && profileID > -1) // Filtered for profileID
                            {
                                var accreditations = from u in eventTagUniverse
                                                     where u.EventTagStatus == true && u.ProfileID == profileID
                                                     select new
                                                     {
                                                         EventAccreditatedTag = u.EventTag
                                                     };

                                nrAccreditatedTagsPerEvent = accreditations.Count();

                                var nonAccreditations = from u in eventTagUniverse
                                                     where (u.EventTagStatus == null || u.EventTagStatus == false) && u.ProfileID == profileID
                                                     select new
                                                     {
                                                         EventAccreditatedTag = u.EventTag
                                                     };

                                nrUnAccreditatedTagsPerEvent = nonAccreditations.Count();
                            }
                            else // Filtered for partnerID and profileID
                            {
                                var accreditations = from u in eventTagUniverse
                                                     where u.EventTagStatus == true && u.PartnerID == partnerID && u.ProfileID == profileID
                                                     select new
                                                     {
                                                         EventAccreditatedTag = u.EventTag
                                                     };

                                nrAccreditatedTagsPerEvent = accreditations.Count();

                                var nonAccreditations = from u in eventTagUniverse
                                                     where (u.EventTagStatus == null || u.EventTagStatus == false) && u.PartnerID == partnerID && u.ProfileID == profileID
                                                     select new
                                                     {
                                                         EventAccreditatedTag = u.EventTag
                                                     };

                                nrUnAccreditatedTagsPerEvent = nonAccreditations.Count();
                            }
                        }
                    }

                    if (nrAccreditatedTagsPerEvent == 0 && nrUnAccreditatedTagsPerEvent == 0)
                    {
                        StatusModel statusModel = new StatusModel();
                        statusModel.status = StatusCodes.EmptyDataSet;
                        return Ok(statusModel);
                    }

                    ReportModel newReport = new ReportModel();

                    newReport.reportTitle = "Acreditações";
                    newReport.Xlabel = "Subconjuntos";
                    newReport.Ylabel = "Número de acreditações";

                    List<ReportData> newReportData = new List<ReportData>();

                    ReportData reportData1 = new ReportData();
                    reportData1.Xdata = "Com acreditação";
                    reportData1.Ydata = nrAccreditatedTagsPerEvent;
                    newReportData.Add(reportData1);

                    ReportData reportData2 = new ReportData();
                    reportData2.Xdata = "Sem acreditação";
                    reportData2.Ydata = nrUnAccreditatedTagsPerEvent;
                    newReportData.Add(reportData2);

                    newReport.reportData = newReportData;

                    return Ok(newReport);
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
    }
}