using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using uAdventure.Editor;

namespace uAdventure.Analytics
{
    [DOMWriter(typeof(SurveyManagerConfig))]
    public class SurveyManagerConfigWriter : ParametrizedDOMWriter
    {
        public SurveyManagerConfigWriter() {}

        protected override void FillNode(XmlNode node, object target, params IDOMWriterParam[] options)
        {
            var element = node as XmlElement;
            var config = target as SurveyManagerConfig;

            element.SetAttribute("show-survey", config.ShowSurveyOnStart ? "yes" : "no");
            element.SetAttribute("start-survey", config.StartSurvey.ToString());
            element.SetAttribute("end-survey", config.EndSurvey.ToString());
        }

        protected override string GetElementNameFor(object target)
        {
            return "survey-manager-config";
        }
    }
}
