using System.Collections.Generic;


namespace CnAppForAzureDev.ViewModels
{
    public class TrolleyViewModel
    {
        public string Id { get; set; }

        public IList<TrolleyItemViewModel> Items { get; set; } = new List<TrolleyItemViewModel>();

        public string OwnerId { get; set; }
    }
}
