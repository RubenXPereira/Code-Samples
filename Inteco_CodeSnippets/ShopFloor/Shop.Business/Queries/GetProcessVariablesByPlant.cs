using System;
using System.Linq;
using Imas.Business;
using Imas.Persistence;
using Imas.Domain.Entities;
using Shop.Common;
using System.Collections.Generic;

namespace Shop.Business.Queries
{
    public class GetProcessVariablesByPlant : AbstractQuery
    {
        public GetProcessVariablesByPlant(IDbFacade dbFacade)
            : base(dbFacade)
        { }

        public virtual List<int> Execute(string plantName)
        {
            try
            {
                using (var dbSession = dbFacade.Transaction())
                {
                    return dbSession.Set<ProcessVariable>()
                        .Where(x =>
                            x.Name == plantName + "." + Constants.PROCESS_VARIABLE_CURRENT ||
                            x.Name == plantName + "." + Constants.PROCESS_VARIABLE_ELECTRODE_POSITION ||
                            x.Name == plantName + "." + Constants.PROCESS_VARIABLE_ELECTRODE_WEIGHT ||
                            x.Name == plantName + "." + Constants.PROCESS_VARIABLE_INGOT_WEIGHT ||
                            x.Name == plantName + "." + Constants.PROCESS_VARIABLE_MELTRATE ||
                            x.Name == plantName + "." + Constants.PROCESS_VARIABLE_POWER ||
                            x.Name == plantName + "." + Constants.PROCESS_VARIABLE_RESISTANCE ||
                            x.Name == plantName + "." + Constants.PROCESS_VARIABLE_SWING ||
                            x.Name == plantName + "." + Constants.PROCESS_VARIABLE_VOLTAGE
                        )
                        .Select(x => x.ID)
                        .ToList();
                }
            }
            catch (Exception e)
            { throw Oops.Rewrap(e); }
        }
    }
}
