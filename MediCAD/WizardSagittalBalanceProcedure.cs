using Hectec.Core.Helper;
using Hectec.Core.Procedures;
using System;
using System.Windows.Forms;

namespace Hectec.Spine.Procedures.WizardProcedures
{
    class WizardSagittalBalanceProcedure : WizardProcedure
    {
        #region members
        private static readonly string WizardName = "wizard_sagittal_balance";
        public EventHandler<EventArgs> CallerContainerItem;
        private Control ctrlHelpSteps;
        #endregion
        
        #region overrides
        public override object HelpSteps
        {
            get
            {
                return ctrlHelpSteps;
            }
        }

        public override bool ShowPatientsBodySide
        {
            get
            {
                return false;
            }
        }
        
        public override void Activate()
        {
            if (Parameter != null && Parameter.GetType().Equals(typeof(Tuple<Control, EventHandler<EventArgs>>)))
            {
                ctrlHelpSteps = ((Tuple<Control, EventHandler<EventArgs>>)Parameter).Item1;
                CallerContainerItem = ((Tuple<Control, EventHandler<EventArgs>>)Parameter).Item2;
            }            

            Parameter = WizardMeasurementsHelper.GetWizardMeasurements(WizardName);

            base.Activate();
        }

        protected override void Close()
        {
            CallerContainerItem?.Invoke(this, EventArgs.Empty);
            base.Close();
        }
        #endregion
    }
}
