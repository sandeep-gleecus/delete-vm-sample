using Inflectra.SpiraTest.Installer.UI;
using System.Collections.Generic;

namespace Inflectra.SpiraTest.Installer.ControlForm
{
    public delegate void ProcedureCompleteDelegate(ItemProgress.ProcessStatusEnum status, string extraMessage);

    public interface IProcedureProcessComponent : IProcedureComponent
    {
        List<ProcedureCompleteDelegate> ProcedureCompleteDelegates
        {
            get;
        }

        void startProcedure();
        bool cancelProcedure();
    }
}
