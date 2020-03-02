using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using uAdventure.Core;

namespace uAdventure.Analytics
{
    [DOMParser("survey-manager-config")]
    public class SurveyManagerConfigParser : IDOMParser
    {
        public object DOMParse(XmlElement element, params object[] parameters)
        {
            return new SurveyManagerConfig
            {
                ShowSurveyOnStart = ExParsers.ParseDefault(element.GetAttribute("show-survey"), "no") == "yes",
                StartSurvey = ExParsers.ParseDefault(element.GetAttribute("start-survey"), 0),
                EndSurvey = ExParsers.ParseDefault(element.GetAttribute("end-survey"), 0)
            };
        }
    }
}
