using FeatureFlags.Core.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace FeatureFlags.Core.Helper
{
    public static class UserFlagsHelper
    {
        public static IEnumerable<string> GetIndividualFlags(List<int> flags)
        {
            if (flags == null || flags.Count == 0)
            {
                return new List<string> { "None" };
            }

            Dictionary<UserFlags, string> displayAttributeCache = [];

            var result = flags.Select(flag =>
            {
                if (!Enum.IsDefined(typeof(UserFlags), flag))
                {
                    return flag.ToString();
                }

                var enumFlag = (UserFlags)flag;
                if (!displayAttributeCache.TryGetValue(enumFlag, out var displayName))
                {
                    var memInfo = typeof(UserFlags).GetMember(enumFlag.ToString());
                    var displayAttribute = memInfo[0].GetCustomAttributes(typeof(DisplayAttribute), false)
                                                    .OfType<DisplayAttribute>()
                                                    .FirstOrDefault();

                    displayName = displayAttribute?.Name ?? enumFlag.ToString();
                    displayAttributeCache[enumFlag] = displayName;
                }

                return displayName;
            }).ToList();

            return result;
        }

        public static IEnumerable<SelectListItem> GetFlagFilterItems()
        {
            var flags = Enum.GetValues(typeof(UserFlags)).Cast<UserFlags>();
            var items = flags.Select(flag => new SelectListItem
            {
                Text = GetDisplayName(flag),
                Value = ((int)flag).ToString()
            });

            return items;
        }

        private static string GetDisplayName(Enum value)
        {
            return value.GetType()
                .GetMember(value.ToString())
                .FirstOrDefault()?
                .GetCustomAttribute<DisplayAttribute>()?
                .GetName() ?? value.ToString();
        }
    }
}
