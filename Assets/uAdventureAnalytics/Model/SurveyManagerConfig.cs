

namespace uAdventure.Analytics
{
    public class SurveyManagerConfig
    {
        private bool showSurveyOnStart = true;
        private int startSurvey, endSurvey;

        //#################################################
        //################### PROPERTIES ##################
        //#################################################
        #region properties
        public bool ShowSurveyOnStart
        {
            get
            {
                return showSurveyOnStart;
            }

            set
            {
                showSurveyOnStart = value;
            }
        }

        public int StartSurvey
        {
            get
            {
                return startSurvey;
            }

            set
            {
                startSurvey = value;
            }
        }

        public int EndSurvey
        {
            get
            {
                return endSurvey;
            }

            set
            {
                endSurvey = value;
            }
        }
        #endregion properties
    }
}
