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
    public class AllPartnersController : ApiController
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

                    var partnersQuery = from u in dbContext.TbPartners   // Search all partners in the database, with criteria
                                        join u2 in dbContext.TbPartnerDatas on u.PartnerId equals u2.PartnerId
                                        join u3 in dbContext.TbTexts on u2.TextCode equals u3.TextCode
                                        where u.IsStaff == true && u.EventId == eventID  // Multi-event
                                        orderby u.PartnerLabel ascending
                                        select new
                                        {
                                            partnerId = u.PartnerId,
                                            partnerIdLabel = u.PartnerLabel,
                                            sentEmail = u.SentMail,
                                            isAdmin = u.IsAdmin,
                                            fieldname = u3.TextLabel,
                                            value = u2.Value,
                                            type = u2.Type,
                                            order = u2.Order,
                                            mandatory = u2.Mandatory,
                                            textCode = u2.TextCode
                                        };

                    Dictionary<int, Partner> partnersDict = new Dictionary<int, Partner>();

                    foreach (var partnerQuery in partnersQuery)
                    {
                        if (partnerQuery.isAdmin == true)
                        {
                            continue;
                        }

                        if (!partnersDict.ContainsKey(partnerQuery.partnerId))
                        {
                            Partner partner = new Partner();
                            partner.partnerId = partnerQuery.partnerId;
                            partner.partnerLabel = partnerQuery.partnerIdLabel;
                            partner.sentmail = partnerQuery.sentEmail ?? default(bool);
                            partner.list = new List<PartnerModel>();
                            partnersDict.Add(partnerQuery.partnerId, partner);
                        }

                        PartnerModel data = new PartnerModel();
                        data.fieldname = partnerQuery.fieldname;
                        data.value = partnerQuery.value;
                        data.type = partnerQuery.type;
                        data.order = partnerQuery.order;
                        data.id = partnerQuery.partnerId;
                        data.mandatory = partnerQuery.mandatory;
                        data.fieldcode = partnerQuery.textCode;
                        partnersDict[partnerQuery.partnerId].list.Add(data);
                    }

                    Partners partnerslist = new Partners();
                    partnerslist.partners = partnersDict.Select(d => d.Value).ToList();

                    return Ok(partnerslist);
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
