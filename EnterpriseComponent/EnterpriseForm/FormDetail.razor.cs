using EnterpriseComponent.Attributes;
using EnterpriseComponent.Helper;
using EnterpriseComponent.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EnterpriseComponent.EnterpriseForm
{
    public partial class FormDetail
    {

        private IEnumerable<object> Elements = new List<object>();
        private List<string> _events = new();
        private bool _editTriggerRowClick;

        IDictionary<string, FormAttribute>? headerContent;

        protected override async Task OnInitializedAsync()
        {
            // Get Header
            var header = await httpService.Get<List<IDictionary<string, object>>>(DetailEndpoint!.Get.Url);
            string content = JsonSerializer.Serialize(header.Response);

            Elements = (IEnumerable<object>)content.DeserializeObject();// Serialize for complex json

            Console.WriteLine(content);

            mapAttributes(header.Response);
        }

        private void mapAttributes(List<IDictionary<string, object>> contents)
        {

            headerContent = new Dictionary<string, FormAttribute>();
            PropertyInfo[] listPI = DetailType!.GetProperties();

            foreach (var content in contents)
            {
                foreach (PropertyInfo pi in listPI)
                {

                    FormAttribute dp = pi.GetCustomAttributes(typeof(FormAttribute), true).Cast<FormAttribute>().SingleOrDefault()!;
                    if (dp != null)
                    {
                        var isExist = content.TryGetValue(dp.Name!, out object? dictionary);
                        if (isExist)
                        {
                            if (dp.InputType == FieldInputType.Number)
                            {
                                dp.ValueNumber = Convert.ToDouble(dictionary);
                            }
                            else
                            {
                                dp.Value = dictionary!.ToString();
                            }

                        }

                        headerContent.Add(dp.Name!, dp);
                    }
                }
            }

            
        }
        // events
        void StartedEditingItem(object item)
        {
            _events.Insert(0, $"Event = StartedEditingItem, Data = {System.Text.Json.JsonSerializer.Serialize(item)}");
        }

        void CancelledEditingItem(object item)
        {
            _events.Insert(0, $"Event = CancelledEditingItem, Data = {System.Text.Json.JsonSerializer.Serialize(item)}");
        }

        void CommittedItemChanges(object item)
        {
            _events.Insert(0, $"Event = CommittedItemChanges, Data = {System.Text.Json.JsonSerializer.Serialize(item)}");
        }

    }
}
