using FeatureFlags.Core.Enums;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Text;

namespace FeatureFlags.Core.Helpers.TagHelpers
{
    public static class EnumCheckboxHelper
    {
        public static IHtmlContent EnumCheckboxesFor<TModel>(
            this IHtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, List<int>>> expression,
            List<int> modelValue,
            string? customCssClass = null,
            bool includeControlLabel = true)
        {
            var enumType = typeof(UserFlags);
            var values = Enum.GetValues(enumType);

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("<div class='row'>");

            foreach (var value in values)
            {
                int flagValue = (int)value;
                if (flagValue != 0)
                {
                    var displayName = GetEnumDisplayName(value);

                    stringBuilder.AppendLine($"<div class='col-md-6 {customCssClass}'>");
                    stringBuilder.AppendLine("<div class='form-group'>");

                    if (includeControlLabel)
                    {
                        stringBuilder.AppendLine($@"<label class='control-label'>{displayName}</label>");
                    }

                    var isChecked = modelValue.Contains(flagValue) ? "checked" : "";

                    stringBuilder.AppendLine(
                        $@"<div class='form-check form-switch'>
                            <input {isChecked} class='form-check-input' type='checkbox' id='{flagValue}' name='{htmlHelper.NameFor(expression)}' value='{flagValue}' />
                            <label class='form-check-label' for='{flagValue}'>{displayName}</label>
                        </div>");

                    stringBuilder.AppendLine("</div>");
                    stringBuilder.AppendLine("</div>");
                }
            }

            stringBuilder.AppendLine("</div>");

            return new HtmlString(stringBuilder.ToString());
        }

        private static string GetEnumDisplayName(object value)
        {
            var name = value?.ToString();

            if (!string.IsNullOrEmpty(name))
            {
                var fieldInfo = value!.GetType().GetField(name);

                if (fieldInfo != null)
                {
                    if (fieldInfo.GetCustomAttributes(typeof(DisplayAttribute), false)
                                                     .FirstOrDefault() is DisplayAttribute displayAttribute)
                    {
                        return displayAttribute.Name ?? name;
                    }
                }
            }

            return string.Empty;
        }
    }
}
