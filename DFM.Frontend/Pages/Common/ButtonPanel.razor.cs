using DFM.Shared.Common;

namespace DFM.Frontend.Pages.Common
{
    public partial class ButtonPanel
    {
        bool createBtn = true;
        bool deleteBtn = true;
        bool saveBtn = true;
        bool sendBtn = true;
        bool editBtn = true;
        bool restoreBtn = true;
        bool terminateBtn = true;
        bool backButton = false;
        bool historyBtn = true;
        FormMode oldMode;
        string previousBtn = "";
        string current = "";
        void disableButton()
        {
            oldMode = FormMode;
            switch (FormMode)
            {
                case FormMode.View:
                    if (Behavior == BehaviorStatus.ReadOnly)
                    {
                        createBtn = true;
                        deleteBtn = true;
                        saveBtn = true;
                        sendBtn = true;
                        editBtn = true;
                        restoreBtn = true;
                        terminateBtn = true;
                        historyBtn = false;
                    }
                    else if(Behavior == BehaviorStatus.ReadWrite) 
                    {
                        createBtn = false;
                        deleteBtn = false;
                        saveBtn = true;
                        //sendBtn = false;
                        editBtn = false;
                        terminateBtn = false;
                        restoreBtn = true;
                        historyBtn = false;

                        if (TraceStatus == TraceStatus.CoProccess)
                        {
                            sendBtn = true;
                        }
                        else
                        {
                            sendBtn = false;
                        }
                    }
                    else
                    {
                        createBtn = true;
                        deleteBtn = true;
                        saveBtn = true;
                        //sendBtn = false;
                        editBtn = true;
                        restoreBtn = true;
                        terminateBtn = false;
                        historyBtn = false;
                        if (TraceStatus == TraceStatus.CoProccess)
                        {
                            sendBtn = true;
                        }
                        else
                        {
                            sendBtn = false;
                        }
                    }

                    
                    break;
                case FormMode.Trash:
                    createBtn = true;
                    deleteBtn = true;
                    saveBtn = true;
                    sendBtn = true;
                    editBtn = true;
                    terminateBtn = true;
                    restoreBtn = false;
                    historyBtn = false;
                    break;
                case FormMode.Terminated:
                    createBtn = true;
                    deleteBtn = true;
                    saveBtn = true;
                    sendBtn = true;
                    editBtn = true;
                    terminateBtn = true;
                    restoreBtn = true;
                    historyBtn = false;
                    break;
                case FormMode.Create:
                    createBtn = true;
                    deleteBtn = true;
                    saveBtn = false;
                    sendBtn = false;
                    editBtn = true;
                    terminateBtn = true;
                    restoreBtn = true;
                    historyBtn = true;
                    break;
                case FormMode.Edit:
                    createBtn = true;
                    deleteBtn = false;
                    saveBtn = false;
                    editBtn = true;
                    terminateBtn = false;
                    restoreBtn = true;
                    historyBtn = false;
                    sendBtn = false;
                    break;
                case FormMode.List:
                    createBtn = false;
                    deleteBtn = true;
                    saveBtn = true;
                    sendBtn = true;
                    editBtn = true;
                    terminateBtn = true;
                    restoreBtn = true;
                    historyBtn = true;
                    break;
                default:
                    break;
            }

           
        }

        protected override void OnInitialized()
        {
            disableButton();

            base.OnInitialized();
        }
        protected override Task OnParametersSetAsync()
        {
            //if (oldMode != FormMode)
            //{
            //    disableButton();
            //}
            disableButton();
            switch (FormMode)
            {
                case FormMode.View:
                    previousBtn = RootBreadcrumb!;
                    current = ViewBreadcrumb!;
                    
                    break;
                case FormMode.Trash:
                    previousBtn = RootBreadcrumb!;
                    current = ViewBreadcrumb!;

                    break;
                case FormMode.Create:

                    previousBtn = RootBreadcrumb!;
                    current = CreateBreadcrumb!;
                    break;
                case FormMode.Edit:
                    previousBtn = RootBreadcrumb!;
                    current = EditBreadcrumb!;

                    break;
                case FormMode.List:
                    previousBtn = RootBreadcrumb!;
                    current = "";
                    break;
                default:
                    break;
            }

            if (OnProcessing)
            {
                createBtn = true;
                deleteBtn = true;
                saveBtn = true;
                sendBtn = true;
                editBtn = true;
                backButton = true;
                historyBtn = true;
                restoreBtn = true;
                terminateBtn = true;
            }
            else
            {
                if (FormMode == FormMode.List)
                {
                    createBtn = false;
                }
                backButton = false;
            }
            
            return base.OnParametersSetAsync();
        }
    }
}
