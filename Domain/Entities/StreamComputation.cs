using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AggregationCRS.Domain
{
    public class StreamComputation
    {
        public string EntityKey { get; set; }
        public string ActivityReference { get; set; } = string.Empty;
        public DateTime? ActivityDate { get; set; }
        public string? Branch { set; get; } = string.Empty;
        public string? AccountIdentifier { set; get; } = string.Empty;
        public string? CustomerNumber { set; get; } = string.Empty;
        public string? AccountNumber { set; get; } = string.Empty;
        public string? ActivityId { get; set; }
        public string? ActivityCode { get; set; } = string.Empty;
        public string? GlobalActivityCode { get; set; } = string.Empty;
        public string? ActivityCurrency { get; set; } = string.Empty;
        public decimal? ActivityVolume { get; set; }
        public decimal? ExpectedFlow { get; set; }
        public bool ExpectedFlowFlag { get; set; } = false;
        public DateTime? ExpectedFlowRecordDate { get; set; }
        public string StreamId { set; get; }
        public string? GLAccount { set; get; } = string.Empty;
        public string? StreamReference { get; set; } = string.Empty;
        public DateTime? StreamDate { get; set; }
        public string? StreamCurrency { get; set; } = string.Empty;
        public decimal? ActualFlow { get; set; }
        public bool ActualFlowFlag { get; set; } = false;
        public DateTime? ActualFlowRecordDate { get; set; }
        public bool? HasVariance { get; set; }
        public decimal? Variance { get; set; }
        public bool? UtilizedConcession { get; set; }
        public decimal? ConceededFlowAmount { set; get; }
        public decimal? ConceededFlowAmountLCY { set; get; }
        public string? ConcessionId { set; get; }
        public DateTime LastUpdateTime { set; get; }

        [NotMapped]
        public Dictionary<string, string> Result { get; set; }

        public void MapEntity()
        {
            EntityKey = Result.GetValueOrDefault(nameof(EntityKey));
            ActivityReference = Result.GetValueOrDefault(nameof(ActivityReference));
            Branch = Result.GetValueOrDefault(nameof(Branch));
            AccountIdentifier = Result.GetValueOrDefault(nameof(AccountIdentifier));
            CustomerNumber = Result.GetValueOrDefault(nameof(CustomerNumber));
            AccountNumber = Result.GetValueOrDefault(nameof(AccountNumber));
            ActivityId = Result.GetValueOrDefault(nameof(ActivityId));
            ActivityCode = Result.GetValueOrDefault(nameof(ActivityCode));
            GlobalActivityCode = Result.GetValueOrDefault(nameof(GlobalActivityCode));
            ActivityCurrency = Result.GetValueOrDefault(nameof(ActivityCurrency));

            var activityVolS = Result.GetValueOrDefault(nameof(ActivityVolume));

            if (decimal.TryParse(activityVolS, out decimal activityVol))
            {
                ActivityVolume = activityVol;
            }



            var expectedFlowS = Result.GetValueOrDefault(nameof(ExpectedFlow));
            if (!string.IsNullOrEmpty(expectedFlowS)) { ExpectedFlow = decimal.Parse(expectedFlowS); }

            var expectedFlowFlagS = Result.GetValueOrDefault(nameof(ExpectedFlowFlag));
            if (!string.IsNullOrEmpty(expectedFlowFlagS)) { ExpectedFlowFlag = bool.Parse(expectedFlowFlagS); }

            var activityDateField = Result.GetValueOrDefault(nameof(ActivityDate));
            if (!string.IsNullOrEmpty(activityDateField)) { ActivityDate = DateTime.Parse(activityDateField); }

            StreamId = Result.GetValueOrDefault(nameof(StreamId));
            ActivityId = Result.GetValueOrDefault(nameof(ActivityId));

            GLAccount = Result.GetValueOrDefault(nameof(GLAccount));
            StreamReference = Result.GetValueOrDefault(nameof(StreamReference));
            StreamCurrency = Result.GetValueOrDefault(nameof(StreamCurrency));

            var streamDateField = Result.GetValueOrDefault(nameof(StreamDate));
            if (!string.IsNullOrEmpty(streamDateField)) { StreamDate = DateTime.Parse(streamDateField); }

            var actualFlowS = Result.GetValueOrDefault(nameof(ActualFlow));
            if (!string.IsNullOrEmpty(actualFlowS)) { ActualFlow = decimal.Parse(actualFlowS); }

            var actualFlowFlagS = Result.GetValueOrDefault(nameof(ActualFlowFlag));
            if (!string.IsNullOrEmpty(actualFlowFlagS)) { ActualFlowFlag = bool.Parse(actualFlowFlagS); }

            var hasVarianceS = Result.GetValueOrDefault(nameof(HasVariance));
            if (!string.IsNullOrEmpty(hasVarianceS)) { HasVariance = bool.Parse(hasVarianceS); }

            var varianceS = Result.GetValueOrDefault(nameof(Variance));
            if (!string.IsNullOrEmpty(varianceS)) { Variance = decimal.Parse(varianceS); }

            var utilizedConcessionS = Result.GetValueOrDefault(nameof(UtilizedConcession));
            if (!string.IsNullOrEmpty(utilizedConcessionS)) { UtilizedConcession = bool.Parse(utilizedConcessionS); }

            var conceededFlowAmountS = Result.GetValueOrDefault(nameof(ConceededFlowAmount));
            if (!string.IsNullOrEmpty(conceededFlowAmountS)) { ConceededFlowAmount = decimal.Parse(conceededFlowAmountS); }

            var conceededFlowAmountLCYS = Result.GetValueOrDefault(nameof(ConceededFlowAmountLCY));
            if (!string.IsNullOrEmpty(conceededFlowAmountLCYS)) { ConceededFlowAmountLCY = decimal.Parse(conceededFlowAmountLCYS); }

            var concessionIdS = Result.GetValueOrDefault(nameof(ConcessionId));
            if (!string.IsNullOrEmpty(concessionIdS)) { ConcessionId = (concessionIdS); }

            var lastUpdateTimeS = Result.GetValueOrDefault(nameof(LastUpdateTime));
            if (!string.IsNullOrEmpty(lastUpdateTimeS)) { LastUpdateTime = DateTime.Parse(lastUpdateTimeS); }
        }
    }
}
