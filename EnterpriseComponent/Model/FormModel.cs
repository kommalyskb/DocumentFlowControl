using MudBlazor;

namespace EnterpriseComponent.Model
{

    public class FormModel
    {
        public string? FormID { get; set; }
        public string? PreviousLink { get; set; }
        public Header Header { get; set; } = new();
        public List<Detail> Details { get; set; } = new();
    }

    public class Header
    {
        public string? Title { get; set; }
        public int TotalColumn { get; set; }
        public List<Column> Columns { get; set; } = new();
        public APIEndpoint APIEndpoint { get; set; } = new();
    }

    public class Column
    {
        public List<Field> Fields { get; set; } = new();
    }

    public class Field
    {
        public string? Id { get; set; }
        public FieldInputType? InputType { get; set; }
        public string? Label { get; set; }
        public string? CssClass { get; set; }
        public bool Required { get; set; }
        public bool Readonly { get; set; }
        public bool Visible { get; set; }
        public bool Lookup { get; set; }
        public string? RequireError { get; set; }
        public string? ErrorMessage { get; set; }
        public string? Description { get; set; }
        public string? DefaultText { get; set; }
        public bool Checked { get; set; }
        public Origin AnchorOrigin { get; set; }
        public List<DropdownItem> Items { get; set; } = new();
        public int Max { get; set; }
        public int Min { get; set; }
        public string? Step { get; set; }
        public int MaxLenght { get; set; }
        public Variant Variant { get; set; }
        public APIEndpoint LookupEndpoint { get; set; } = new();
        public int ColumnNo { get; set; } = 0;
    }

    public class DropdownItem
    {
        public string? Text { get; set; }
        public string? Value { get; set; }
    }

    public class Detail
    {
        public string? Title { get; set; }
        public List<Column> Columns { get; set; } = new();
        public APIEndpoint APIEndpoint { get; set; } = new();
    }

    public class APIEndpoint
    {
        public APIModel List { get; set; } = new();
        public APIModel Create { get; set; } = new();
        public APIModel Update { get; set; } = new();
        public APIModel Delete { get; set; } = new();
        public APIModel Get { get; set; } = new();
    }

    public class APIModel
    {
        public string? Url { get; set; }
        public IDictionary<string, string>? Header { get; set; }
    }
    public enum FieldInputType
    {
        Button,
        Checkbox,
        Color,
        Date,
        DateTimeLocal,
        Email,
        File,
        Hidden,
        Image,
        Month,
        Number,
        Password,
        Radio,
        Range,
        Reset,
        Search,
        Submit,
        Text,
        Time,
        Url,
        Week,
        Dropdown,
        Telephone
    }
}
