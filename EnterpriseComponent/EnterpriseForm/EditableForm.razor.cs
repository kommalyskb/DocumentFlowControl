using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using EnterpriseComponent.Model;
using Microsoft.AspNetCore.Components;
using EnterpriseComponent.Attributes;
using MudBlazor;
using System.Text.RegularExpressions;
using System.Globalization;
using EnterpriseComponent.Helper;
using HttpClientService;

namespace EnterpriseComponent.EnterpriseForm
{
    public partial class EditableForm
    {

        IDictionary<string, FormAttribute>? headerContent;
        int column = 1;
        protected override async Task OnInitializedAsync()
        {
            // Get Header
            var header = await httpService.Get<IDictionary<string, object>>(HeaderEndpoint!.Get.Url);
            string content = JsonSerializer.Serialize(header);

            Console.WriteLine(content);
            mapAttributes(header.Response);
            
        }
        

        //public override Task SetParametersAsync(ParameterView parameters)
        //{

        //    if (parameters.TryGetValue<APIEndpoint>(nameof(HeaderEndpoint), out var headerEndpoint))
        //    {
        //        if (headerEndpoint is not null)
        //        {
        //            Console.WriteLine("header is set");
        //        }
        //    }
        //    return base.SetParametersAsync(parameters);
        //}
        private void mapAttributes(IDictionary<string, object> content)
        {
            
            headerContent = new Dictionary<string, FormAttribute>();
            PropertyInfo[] listPI = HeaderType!.GetProperties();
            foreach (PropertyInfo pi in listPI)
            {

                FormAttribute dp = pi.GetCustomAttributes(typeof(FormAttribute), true).Cast<FormAttribute>().SingleOrDefault();
                if (dp != null)
                {
                    var isExist = content.TryGetValue(dp.Name, out object dictionary);
                    if (isExist)
                    {
                        if (dp.InputType == FieldInputType.Number)
                        {
                            dp.ValueNumber = Convert.ToDouble(dictionary);
                        }
                        else
                        {
                            dp.Value = dictionary.ToString();
                        }
                        
                    }
                    // Set column
                    if (dp.Column > column)
                    {
                        column = dp.Column;
                    }
                    headerContent.Add(dp.Name, dp);
                }
            }
        }


        bool success;
        string[] errors = { };
        MudForm form;

        private async Task submitForm(FormInputType inputType, AuthorizeHeader authorizeHeader)
        {


            var parameters = new DialogParameters();
            
            parameters.Add("Color", Color.Error);
            string url = "";
            
            switch (inputType)
            {
                case FormInputType.Create:
                    url = HeaderEndpoint!.Create!.Url!;
                    parameters.Add("ContentText", "Do you really want to create new record?");
                    parameters.Add("ButtonText", "Yes");
                    parameters.Add("Color", Color.Primary);
                    break;
                case FormInputType.Update:
                    url = HeaderEndpoint!.Update!.Url!;
                    parameters.Add("ContentText", "Do you really want to update the existing record?");
                    parameters.Add("ButtonText", "Yes");
                    parameters.Add("Color", Color.Primary);
                    break;
                case FormInputType.Delete:
                    url = HeaderEndpoint!.Delete!.Url!;
                    parameters.Add("ContentText", "Do you really want to delete these records?");
                    parameters.Add("ButtonText", "Yes");
                    parameters.Add("Color", Color.Error);
                    break;
                default:
                    break;
            }

            var options = new DialogOptions() { CloseButton = true };

            var dialog = DialogService.Show<ConfirmDialog>("Confirm", parameters, options);
            var dialogResult = await dialog.Result;
            if (!dialogResult.Cancelled)
            {
                var dialogLoading = DialogService.Show(typeof(LoadingDialog));
                try
                {
                    await Task.Delay(1000); // Delay for 1 second
                    IDictionary<string, object> content = new Dictionary<string, object>();
                    foreach (var item in form._formControls)
                    {
                        var type = item.GetType();
                        if (type.Name.Contains("MudTextField"))
                        {
                            var field = (MudTextField<string>)item;
                            content.Add(field.TagID, field.Value);
                        }
                        else if (type.Name.Contains("MudNumericField"))
                        {
                            var field = (MudNumericField<double?>)item;
                            content.Add(field.TagID, field.Value!);
                        }
                        else
                        {
                            var field = (MudTextField<string>)item;
                            content.Add(field.TagID, field.Value);
                        }

                    }

                    // Serialize to json string
                    var jsonStr = content.SerializeObject();

                    var obj = jsonStr.DeserializeObject();


                    APICallBack api = new APICallBack();

                    if (authorizeHeader is null)
                    {
                        var result = await httpService.Post<object>(url, obj);
                        api.Success = result.Success;
                        var response = await result.HttpResponseMessage.Content.ReadAsStringAsync();
                        api.Response = response;
                    }
                    else
                    {
                        var result = await httpService.Post<object>(url, obj, authorizeHeader);
                        api.Success = result.Success;
                        var response = await result.HttpResponseMessage.Content.ReadAsStringAsync();
                        api.Response = response;
                    }

                    // Call back
                    await OnSubmitFormCallBack.InvokeAsync(api);

                }
                catch (Exception)
                {
                    await OnSubmitFormCallBack.InvokeAsync(new APICallBack
                    {
                        Success = false
                    });
                }

                dialogLoading.Close();
            }

        }
    }

    
}
