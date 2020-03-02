using System;
using System.Collections.Generic;
using uAdventure.Core;
using uAdventure.Editor;

namespace uAdventure.Analytics
{
    public class SurveyManagerConfigDataControl : DataControl
    {
        private readonly SurveyManagerConfig surveyManagerConfig;

        public SurveyManagerConfigDataControl(SurveyManagerConfig surveyManagerConfig)
        {
            this.surveyManagerConfig = surveyManagerConfig;
        }

        //#################################################
        //################### PROPERTIES ##################
        //#################################################
        #region properties
        public bool ShowSurveyOnStart
        {
            get
            {
                return surveyManagerConfig.ShowSurveyOnStart;
            }

            set
            {
                controller.AddTool(new ChangeBooleanValueTool(surveyManagerConfig, value, "ShowSurveyOnStart"));
            }
        }

        public int StartSurvey
        {
            get
            {
                return surveyManagerConfig.StartSurvey;
            }

            set
            {
                controller.AddTool(new ChangeIntegerValueTool(surveyManagerConfig, value, "StartSurvey"));
            }
        }

        public int EndSurvey
        {
            get
            {
                return surveyManagerConfig.EndSurvey;
            }

            set
            {
                controller.AddTool(new ChangeIntegerValueTool(surveyManagerConfig, value, "EndSurvey"));
            }
        }
        #endregion properties

        #region dataControl


        public override object getContent()
        {
            return surveyManagerConfig;
        }

        public override int[] getAddableElements()
        {
            return null;
        }

        public override bool canAddElement(int type)
        {
            return false;
        }

        public override bool canBeDeleted()
        {
            return false;
        }

        public override bool canBeDuplicated()
        {
            return false;
        }

        public override bool canBeMoved()
        {
            return false;
        }

        public override bool canBeRenamed()
        {
            return false;
        }

        public override bool addElement(int type, string id)
        {
            return false;
        }

        public override bool deleteElement(DataControl dataControl, bool askConfirmation)
        {
            return false;
        }

        public override bool moveElementUp(DataControl dataControl)
        {
            return false;
        }

        public override bool moveElementDown(DataControl dataControl)
        {
            return false;
        }

        public override string renameElement(string newName)
        {
            return null;
        }

        public override void updateVarFlagSummary(VarFlagSummary varFlagSummary)
        {
            throw new NotImplementedException();
        }

        public override bool isValid(string currentPath, List<string> incidences)
        {
            return true;
        }

        public override int countAssetReferences(string assetPath)
        {
            return 0;
        }

        public override void getAssetReferences(List<string> assetPaths, List<int> assetTypes)
        {
        }

        public override void deleteAssetReferences(string assetPath)
        {
        }

        public override int countIdentifierReferences(string id)
        {
            return 0;
        }

        public override void replaceIdentifierReferences(string oldId, string newId)
        {
        }

        public override void deleteIdentifierReferences(string id)
        {
        }

        public override List<Searchable> getPathToDataControl(Searchable dataControl)
        {
            return dataControl == this ? new List<Searchable> { this } : null;
        }

        public override void recursiveSearch()
        {

        }
        #endregion dataControl
    }
}