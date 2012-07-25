namespace otr_project.ViewModels
{
    public class RentalCartUpdateViewModel
    {
        //public string Message { get; set; }
        public decimal CartTotal { get; set; }
        public decimal UpdatedItemTotal { get; set; }
        public int UpdatedId { get; set; }
        public int NumberOfDays { get; set; }
        public string Message { get; set; }
        public bool Error { get; set; }
    }
}