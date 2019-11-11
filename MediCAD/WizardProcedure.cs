using Enums.Procedure;
using Hectec.Core.Helper;
using Hectec.Core.Interfaces.EvtArgs;
using Hectec.Core.Interfaces.ObjectModel;
using Hectec.Core.MPL.Data;
using Hectec.Core.MPL.Enums;
using Hectec.Core.ObjectModel;
using Hectec.Core.Views;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Hectec.Core.Procedures
{
    public class WizardProcedure : Procedure, IWizardProcedure
    {
        #region Fields

        protected IWizardView mWizardView;
        protected bool mbBeginProcInProgress;
        protected bool mbCanClose = true;
        protected ManualMeasurementItem[] mMeasurementsList;
        protected int mCurrentMeasurementIndex = -1;
        protected PatientsBodySide mPatientBodySide;
        protected bool mSkipMeasurementIfCompleted;
        protected List<ManualMeasurementItem> mMeasurementIgnoreList = new List<ManualMeasurementItem>();

        #endregion

        #region Properties

        public virtual object HelpSteps
        {
            get
            {
                return null;
            }
        }

        public virtual bool ShowPatientsBodySide
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// This flag configures the initialization of this Wizard
        /// false -> Wizard initialization is performed by this class
        /// true -> Custom initialization
        /// </summary>
        protected virtual bool InitManually { get; set; } = false;

        #endregion

        #region Overrides

        public override void Activate()
        {
            base.Activate();

            // Save original body side
            mPatientBodySide = Session.PatientBodySide;

            if (!InitManually)
            {
                IEnumerable<ManualMeasurementItem> measurements = null;

                var helper = new WizardMeasurementsHelper();

                if (Parameter is string)
                {
                    var wizard = helper.GetWizard((string)Parameter);

                    if (wizard != null)
                    {
                        measurements = wizard.SelectedMeasurements;
                        mSkipMeasurementIfCompleted = wizard.Skip;
                    }
                }
                else if (Parameter is IEnumerable<ManualMeasurementItem>)
                {
                    measurements = Parameter as IEnumerable<ManualMeasurementItem>;

                    mSkipMeasurementIfCompleted = (helper.GetWizard(measurements.FirstOrDefault()?.WizardName)?.Skip).GetValueOrDefault(false);
                }

                if (ValidateMeasurementsList(measurements))
                {
                    // Init Wizard View
                    if (mWizardView == null && ProcedureContext.AllViews.TryGetValue("Wizard", out ViewRegistrar viewRegistrar))
                    {
                        var parameter = new object[] { };
                        mWizardView = DynamicInitializer.New<IWizardView>(viewRegistrar.ViewType, parameter);
                        mWizardView.Init(this, Session);
                        mWizardView.Position = DockSide.Left;
                        mWizardView.PositionInContainer = DockContainerSide.Center;
                        DockItems = new List<IDockItem>
                        {
                            mWizardView
                        };
                    }

                    ProcedureContext.OnViewComponentChange(new ViewComponentChangeArgs(ViewComponents.OpenCC, 0));

                    // Select first procedure
                    if (NextProcedure())
                    {
                        Session.CurrentContext.ProcedureBegin -= ProcBegin;
                        Session.CurrentContext.ProcedureBegin += ProcBegin;

                        mbCanClose = false;

                        return;
                    }
                }

                Close();
            }
        }

        public override void Resume()
        {
            base.Resume();

            Session.CurrentContext.ProcedureBegin -= ProcBegin;
            Session.CurrentContext.ProcedureBegin += ProcBegin;

            if (mbCanClose || Session.ClosingInProgress)
            {
                Close();
            }
            else if (!mbBeginProcInProgress)
            {
                if (!NextProcedure())
                {
                    Close();
                }
            }
        }

        public override bool PreEnd()
        {
            if (mbCanClose || !mbBeginProcInProgress)
            {
                mbCanClose = true;
                Session.PatientBodySide = mPatientBodySide;
            }

            return mbCanClose;
        }

        public override void Suspend()
        {
            ProcedureContext.OnViewComponentChange(new ViewComponentChangeArgs(ViewComponents.CloseCC, 0));
            Session.CurrentContext.ProcedureBegin -= ProcBegin;
            base.Suspend();
        }

        #endregion

        #region Event handlers

        private void ProcBegin(string procName)
        {
            if (!mbBeginProcInProgress) // Close the WizardProcedure if the procedure begins from the outside
            {
                mbCanClose = true;
            }
        }

        #endregion

        #region Implementation

        /// <summary>
        /// Forms the list with valid procedures
        /// </summary>
        /// <returns>
        /// Returns true if there is at least one valid procedure, false otherwise
        /// </returns>
        protected bool ValidateMeasurementsList(IEnumerable<ManualMeasurementItem> list)
        {
            bool result = false;

            if (list != null && list.Count() > 0)
            {
                List<ManualMeasurementItem> newList = new List<ManualMeasurementItem>();

                foreach (var item in list)
                {
                    if (item != null
                        && ProcedureContext.AllProcedures.ContainsKey(item.ProcName)
                        && !ContainsMeasurement(newList, item.ProcName, Session.PatientBodySide))
                    {
                        if (item.Dependents != null)
                        {
                            bool isBothBodySideRequired = false;

                            foreach (var d in item.Dependents)
                            {
#if DEBUG
                                Debug.Assert(ProcedureContext.AllProcedures.ContainsKey(d.ProcName),
                                    string.Format("Invalid dependents item(procedure:{0})", d.ProcName));
#endif
                                if (d.IsBothBodySideRequired
                                    && list.Where(mi => mi.ProcName == d.ProcName).FirstOrDefault() != null)
                                {
                                    isBothBodySideRequired = true;

#if RELEASE
                                    break;
#endif
                                }
                            }

                            if (isBothBodySideRequired)
                            {
                                if (!ContainsMeasurement(newList, item.ProcName, PatientsBodySide.Left))
                                {
                                    newList.Add(new ManualMeasurementItem(item.Id, item.ProcName, item.TitleResId, item.Parameter, PatientsBodySide.Left, item.Dependents));
                                }

                                if (!ContainsMeasurement(newList, item.ProcName, PatientsBodySide.Right))
                                {
                                    newList.Add(new ManualMeasurementItem(item.Id, item.ProcName, item.TitleResId, item.Parameter, PatientsBodySide.Right, item.Dependents));
                                }
                            }
                            else
                            {
                                newList.Add(item);
                            }
                        }
                        else
                        {
                            newList.Add(item);
                        }
                    }
#if DEBUG
                    else
                    {
                        Debug.Assert(false, "Invalid or already exists measurement item ");
                    }
#endif
                }

                if (newList.Count > 0)
                {
                    mMeasurementsList = newList.ToArray();
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// Lets user to know if specified procedure exist in the list
        /// </summary>
        /// <param name="list">Wizard measurements list</param>
        /// <param name="procName">procedure name</param>
        /// <param name="bodyside">procedure bodyside</param>
        /// <returns>Returns True if procedure with specific body side present in the list, False otherwise</returns>
        protected bool ContainsMeasurement(List<ManualMeasurementItem> list, string procName, PatientsBodySide bodyside)
        {
            bool result = false;

            if (list != null)
            {
                result = list.Where(l => l.ProcName == procName &&
                (l.BodySide == bodyside || (!l.BodySide.HasValue && Session.PatientBodySide == bodyside))).FirstOrDefault() != null;
            }

            return result;
        }

        /// <summary>
        /// Closes the Wizard
        /// </summary>
        protected virtual void Close()
        {
            mbCanClose = true;
            Session?.CurrentContext?.EndCurrentProcedure();
        }

        /// <summary>
        /// Gets info about the current procedure
        /// </summary>
        protected ManualMeasurementItem GetCurrentProcedureInfo()
        {
            return GetProcedureInfo(mCurrentMeasurementIndex);
        }

        /// <summary>
        /// Gets Information about the measurement item
        /// </summary>
        /// <param name="procIndex">Measurement item index</param>
        /// <returns>Returns the measurement item for the passed index</returns>
        protected ManualMeasurementItem GetProcedureInfo(int procIndex)
        {
            if (procIndex >= 0 && procIndex < mMeasurementsList.Length)
            {
                return mMeasurementsList[procIndex];
            }
            else
                return null;
        }

        /// <summary>
        /// Starts a procedure
        /// </summary>
        /// <param name="procToStart">The procedure to start</param>
        protected virtual Procedure BeginProcedure(ManualMeasurementItem procToStart)
        {
            Procedure result = null;

            try
            {
                mbBeginProcInProgress = true;

                if (procToStart != null)
                {
                    UpdateSessionBodySide(procToStart);
                    result = Session.CurrentContext?.BeginProcedure(procToStart.ProcName, procToStart.Parameter, this, GetProcedureElement(procToStart));
                    mWizardView?.UpdateView();
                }
            }
            finally
            {
                mbBeginProcInProgress = false;
            }

            return result;
        }

        /// <summary>
        /// Returns the instance of the element if the procedure is completed, null otherwise
        /// </summary>
        protected virtual Element GetProcedureElement(ManualMeasurementItem proc)
        {
            Element result = null;

            if (proc != null)
            {
                var elements = (Session?.CurrentContext?.ActiveWorkView?.RootItem as Plane)?.GetAllElements();

                if (elements != null)
                {
                    var e = elements.Where(ei => ei.EditProcedure == proc.ProcName
                    &&
                    (
                        (proc.BodySide.HasValue && ei.PatientsBodySide == proc.BodySide.Value) ||
                        (!proc.BodySide.HasValue && ei.PatientsBodySide == Session.PatientBodySide)
                        || ei.PatientsBodySide == PatientsBodySide.None
                    )).FirstOrDefault();

                    if (e != null && e.State == ElementState.Complete)
                        result = e;
                }
            }

            return result;
        }

        /// <summary>
        /// Checks if the procedure has been executed successfully before
        /// </summary>
        protected bool CheckIfExecuted(ManualMeasurementItem item)
        {
            return GetProcedureElement(item) != null;
        }

        /// <summary>
        /// Executes the next procedure
        /// </summary>
        /// <returns>Returns true if the procedure executed successfully, false otherwise</returns>
        protected bool NextProcedure()
        {
            bool result = false;

            if (mMeasurementsList != null && ((IWizardProcedure)this).CanNext)
            {
                mbCanClose = false;

                Procedure procedure = null;
                do
                {
                    AddProcedureToIgnoreList(GetCurrentProcedureInfo());                    
                    mCurrentMeasurementIndex++;

                    if (!SkipIfCompleted(ref mCurrentMeasurementIndex))
                    {
                        Close();
                        break;
                    }

                    for (int i = 0; i < 2; i++)
                    {
                        procedure = BeginProcedure(GetCurrentProcedureInfo());

                        if ((procedure != null && procedure.ProcState == RunState.Running)
                            || mSkipMeasurementIfCompleted)
                        {
                            result = true;
                            break;
                        }
                    }
                }
                while (((IWizardProcedure)this).CanNext && procedure != null && procedure.ProcState != RunState.Running);
            }

            return result;
        }

        /// <summary>
        /// Adds the current <param name="curProc"/> procedure into the Ignore List in case it is not completed, otherwise removes it from the Ignore List.
        /// This method should be used when we skip the procedure (click in the 'Next' button until the procedure has completed)
        /// </summary>
        protected void AddProcedureToIgnoreList(ManualMeasurementItem curProc)
        {
            if (curProc != null)
            {
                var procElement = GetProcedureElement(curProc);
                if (procElement == null || procElement.State != ElementState.Complete)
                {
                    if (!mMeasurementIgnoreList.Contains(curProc))
                    {
                        mMeasurementIgnoreList.Add(curProc);
                    }
                }
                else
                {
                    mMeasurementIgnoreList.Remove(curProc);
                }
            }
        }

        /// <summary>
        /// Move to the next measurement if the current one is completed or added into <see cref="mMeasurementIgnoreList"/>.
        /// </summary>
        /// <param name="curMeasurementIndex">the index of the current measurement</param>
        /// <returns>True if the procedure is not completed, False otherwise</returns>
        private bool SkipIfCompleted(ref int curMeasurementIndex)
        {
            bool result = false;

            ManualMeasurementItem mItem = null;

            if (mSkipMeasurementIfCompleted)
            {
                while ((mItem = GetProcedureInfo(curMeasurementIndex)) != null)
                {
                    UpdateSessionBodySide(mItem);

                    var procElement = GetProcedureElement(mItem);
                    if ((procElement == null || procElement.State != ElementState.Complete)
                        && !IsInIgnoreList(mItem))
                    {
                        result = true;
                        break;
                    }
                    else
                    {
                        curMeasurementIndex++;
                    }
                }
            }
            else
            {
                while ((mItem = GetProcedureInfo(curMeasurementIndex)) != null)
                {
                    var procElement = GetProcedureElement(mItem);
                    if (!IsInIgnoreList(mItem))
                    {
                        result = true;
                        break;
                    }
                    else
                    {
                        curMeasurementIndex++;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Moving to the previous measurement until finding one that is not in the <see cref="mMeasurementIgnoreList"/> or until we reach the end of the list
        /// </summary>
        /// <param name="curMeasurementIndex">Index of the current procedure (updating inside the function)/param>
        /// <returns>True if a suitable procedure is found, False otherwise</returns>
        protected bool SkipPrev(ref int curMeasurementIndex)
        {
            bool result = false;

            ManualMeasurementItem mItem = null;
            while ((mItem = GetProcedureInfo(curMeasurementIndex)) != null)
            {
                UpdateSessionBodySide(mItem);

                var procElement = GetProcedureElement(mItem);
                if (!IsInIgnoreList(mItem))
                {
                    result = true;
                    break;
                }
                else
                {
                    curMeasurementIndex--;
                }
            }

            return result;
        }

        /// <summary>
        /// Checks if the current <param name="item"/> procedure is in the Ignore List (<seealso cref="mMeasurementIgnoreList"/>)
        /// </summary>
        protected bool IsInIgnoreList(ManualMeasurementItem item)
        {
            return IsInIgnoreList(item?.ProcName);
        }

        /// <summary>
        /// Checks if the current <param name="procName"/> procedure is in the Ignore List (<seealso cref="mMeasurementIgnoreList"/>).
        /// </summary>
        protected bool IsInIgnoreList(string procName)
        {
            bool result = false;

            if (!string.IsNullOrWhiteSpace(procName))
            {
                foreach (var ignoreItem in mMeasurementIgnoreList)
                {
                    if (ignoreItem.Dependents != null)
                    {
                        if ((from d in ignoreItem.Dependents
                             where string.Compare(d.ProcName, procName, true) == 0
                             select d).FirstOrDefault() != null)
                        {
                            result = true;
                            break;
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Update session body side for specified measurement<param name="measurement"/>.
        /// </summary>
        protected void UpdateSessionBodySide(ManualMeasurementItem measurement)
        {
            if (measurement != null)
            {
                if (measurement.BodySide.HasValue)
                    Session.PatientBodySide = measurement.BodySide.Value;
                else
                    Session.PatientBodySide = mPatientBodySide;
            }
        }

        /// <summary>
        /// Executes the previous procedure
        /// </summary>
        /// <returns>True if the procedure is successfully executed, false otherwise</returns>
        protected bool PrevProcedure()
        {
            bool result = false;

            if (mMeasurementsList != null && ((IWizardProcedure)this).CanPrev)
            {
                Procedure procedure = null;
                mbCanClose = false;
                mCurrentMeasurementIndex--;

                if (SkipPrev(ref mCurrentMeasurementIndex))
                {
                    for (int i = 0; i < 2; i++)
                    {
                        procedure = BeginProcedure(GetCurrentProcedureInfo());

                        if (procedure != null && procedure.ProcState == RunState.Running)
                        {
                            result = true;
                            break;
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Returns True if the current procedure element is completed or there is no dependent procedures, False otherwise
        /// </summary>
        protected bool CheckDependents()
        {
            bool result = true;

            var proc = GetCurrentProcedureInfo();
            if (GetProcedureElement(proc)?.State != ElementState.Complete)
            {
                if (proc?.Dependents != null && proc.Dependents.Length > 0)
                {
                    for (int i = mCurrentMeasurementIndex + 1, length = mMeasurementsList.Length; i < length; i++)
                    {
                        if (proc.Dependents.Where(d => d.ProcName == mMeasurementsList[i]?.ProcName).FirstOrDefault() != null)
                        {
                            result = false;
                            break;
                        }
                    }
                }
            }

            return result;
        }

        #endregion

        #region Implementation of the IWizardProcedure

        bool IWizardProcedure.CanPrev
        {
            get
            {
                return mCurrentMeasurementIndex > 0;
            }
        }

        bool IWizardProcedure.CanNext
        {
            get
            {
                return mCurrentMeasurementIndex >= -1 && mCurrentMeasurementIndex < (mMeasurementsList.Length - 1);
            }
        }

        string IWizardProcedure.Title
        {
            get
            {
                return string.Format(Session.R("WizardRangeTitle"), mCurrentMeasurementIndex + 1, mMeasurementsList.Length);
            }
        }

        void IWizardProcedure.Prev()
        {
            PrevProcedure();
        }

        void IWizardProcedure.Next()
        {
            NextProcedure();
        }

        void IWizardProcedure.Exit()
        {
            Close();
        }

        #endregion
    }
}
