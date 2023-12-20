using FeatureFlags.Core.Enums;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;

namespace FeatureFlags.Core.Helpers.TagHelpers
{
    [HtmlTargetElement("enum-checkboxes", Attributes = "for")]
    public class EnumCheckboxesTagHelper : TagHelper
    {
        public required string For { get; set; }
        public int Col { get; set; } = 3;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var enumType = typeof(UserFlags);
            var values = Enum.GetValues(enumType).Cast<UserFlags>().Where(e => e != UserFlags.None).ToList();

            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("<div id='checkboxContainer'>");

            for (int i = 0; i < values.Count; i++)
            {
                if (i % Col == 0)
                {
                    if (i != 0)
                    {
                        stringBuilder.AppendLine("</div>");
                    }
                    stringBuilder.AppendLine("<div class='row mt-2'>");
                }

                string displayName = GetDisplayName(values[i]);
                stringBuilder.AppendLine(
                    $@"<div class='col-md-{12 / Col}'>
                        <div class='form-group mb-2'>
                            <div class='form-check form-switch'>
                                <input class='form-check-input' type='checkbox' id='{values[i]}' name='{For}' value='{(int)values[i]}' />
                                <label class='form-check-label' for='{values[i]}'>{displayName}</label>
                            </div>
                        </div>
                    </div>");
            }

            stringBuilder.AppendLine("</div></div>");

            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Content.SetHtmlContent(stringBuilder.ToString());
        }

        private static string GetDisplayName(UserFlags enumValue)
        {
            return enumValue.GetType()
                .GetMember(enumValue.ToString())
                .FirstOrDefault()?
                .GetCustomAttribute<DisplayAttribute>()?
                .Name ?? enumValue.ToString();
        }
    }
}
