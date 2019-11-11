using Hectec.Core.MPL.Elements.Osteotomy;
using Hectec.Core.Procedures;
using Hectec.Core.Session;
using Hectec.Spine.Controls.Submodules;
using System;
using System.Windows.Forms;

namespace Hectec.SpineDesktop.Controls.Submodules
{
    public partial class ctrlOsteotomies : ctrlSubmoduleBaseControl
    {
        #region ctor
        public ctrlOsteotomies()
        {
            InitializeComponent();
        }
        #endregion

        #region methods
        public void Init()
        {
            if (cvNewOsteotomyBaseConsoleView != null)
            {
                ProcedureContext pcx = DocSession.ProcedureContexts[DocumentSession.Layout.Planning.ToString()];
                cvNewOsteotomyBaseConsoleView.Init(pcx);
            }
        }
        #endregion

        #region events
        protected override void btnSubmoduleSelection_Click(object sender, EventArgs e)
        {
            base.btnSubmoduleSelection_Click(sender, e);
        }

        private void btnOsteotomy_Click(object sender, EventArgs e)
        {
            Control btn = (Control)sender;
            object parameter = null;

            if (DocSession.CurrentContext.ActiveWorkView != null)
            {
                if (btn == btnManualCutOsteotomy)
                {
                    parameter = typeof(OsteotomyCutWithLineElement);
                }
                else if (btn == btnOpeningWedgeCutOsteotomy)
                {
                    parameter = typeof(OsteotomyOpenWedgeElement);
                }
                else if (btn == btnClosingWedgeCutOsteotomy)
                {
                    parameter = typeof(OsteotomyClosedWedgeElement);
                }
                else if (btn == btnFragmentPositioningOsteotomy)
                {
                    parameter = typeof(OsteotomyCutWithAreaElement);
                }

                DocSession.CurrentContext.BeginProcedure("OsteotomySpine", parameter, null);
            }
        }
        #endregion
    }
}