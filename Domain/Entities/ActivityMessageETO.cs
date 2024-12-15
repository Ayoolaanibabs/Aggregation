namespace AggregationCRS.Domain.Entities
{
    public class ActivityMessageETO
    {
        public string EventId { get; set; }
        public string EntityKey { get => $"{CustomerNumber}.{AccountNumber}.{ActivityCode}.{ActivityCurrency}"; }
        public DateTime ActivityDate { get; set; }
        public string ActivityReference { get; set; } = string.Empty;
        public DateTime StreamDate { get; set; }
        public string ActivityId { get; set; } = string.Empty;
        public string StreamTypeId { get; set; } = string.Empty;
        public string AccountNumber { set; get; }   = string.Empty;
        public string GLAccount { set; get; }   = string.Empty;
        public string Branch { set; get; }   = string.Empty;
        public string StreamId { set; get; }   = string.Empty;
        public string StreamReference { get; set; } = string.Empty;
        public string StreamCode { set; get; }   = string.Empty;
        public string ActivityCode { set; get; }   = string.Empty;
        public string GlobalActivityCode { set; get; }   = string.Empty;
        public string StreamCurrency { set; get; }   = string.Empty;
        public string CustomerNumber { set; get; }   = string.Empty;
        public string ActivityCurrency {  set; get; } = string.Empty;
        public double ActivityVolume {  set; get; } 
        public double ExpectedFlowAmount {  set; get; } 
        public double ActualFlow {  set; get; } 
        public double ConceededFlowAmount {  set; get; } 
        public double UtilizedConcessionFlowAmount {  set; get; } 
        public double ConceededFlowAmountACY {  set; get; } 
        public double VarianceFlowAmount { set; get; }
        public int? ConcessionId { set; get; }
        public string ActivityName { get; set; }
        public string StreamName { get; set; }
        public int? StreamType { get; set; }
    }
}
