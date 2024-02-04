using System.Diagnostics.Metrics;

namespace ShadmanLegal.Models
{
    public class StampDutyModel
    {
        public string StateName { get; set; }
        public List<string> StampDutyDocs { get; set; }
    }

   public class StampDutyDataModel
   {
        public List<State> States { get; set; }
        public List<StampInstrument> Instruments { get; set; }
    }


    public class StampInstrument
    {
        public int InstrumentID { get; set; }
        public string InstrumentName { get; set; }
        public string Description { get; set; }
        public decimal? DefaultTaxValue { get; set; }
    }
    public class State
    {
        public int StateID { get; set; }
        public string StateName { get; set; }
        public int TaxPercentage { get; set; }
    }

}
