using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CPSC.Models
{
    public class ActionPlanModel
    {
        public string AID { get; set; }
        public string DealerCode { get; set; }
        public string Parameter { get; set; }
        public string MetricName { get; set; }
        public string MaxPoint { get; set; }
        public string ParaScoreRYG { get; set; }
        public string CurrentStatus { get; set; }
        public string Observation { get; set; }
        public string ActionPlan { get; set; }
        public string Responsibility { get; set; }
        public string Timeline { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedDateTime { get; set; }
        public string UpdatedBy { get; set; }
        public string UpdatedDateTime { get; set; }
        public string DataType { get; set; }
        public string Submited { get; set; }
        public string Score_Dealer { get; set; }
        public string Remarks { get; set; }
    }
}