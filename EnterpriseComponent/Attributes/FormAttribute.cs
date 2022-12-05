using EnterpriseComponent.Model;
using MudBlazor;
    

namespace EnterpriseComponent.Attributes
{
    public class FormAttribute : Attribute
    {
        public string? Name { get; set; }
        public string? DisplayName { get; set; }
        public string? HelperText { get; set; }
        public int Column { get; set; }
        public int Order { get; set; }
        public bool Readonly { get; set; }
        public bool Visible { get; set; }
        public bool Required { get; set; }
        public int MaxLenght { get; set; }
        public bool Ignore { get; set; }
        public string? Value { get; set; }
        public double? ValueNumber { get; set; }
        public FieldInputType InputType { get; set; }
        public string? RequiredError { get; set; }
    }
}
