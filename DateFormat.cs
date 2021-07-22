using CitiesHarmony.API;
using ColossalFramework.UI;
using ICities;
using System;
using UnityEngine;

namespace DateFormat
{
    public class DateFormat : IUserMod
    {
        // required name and description of this mod
        public string Name => "Date Format";
        public string Description => "Specify the format for dates in the game";

        // whether or not to allow the NewDateFormat routine to run
        private static bool _runNewDateFormat = true;

        public void OnEnabled()
        {
            // check Harmony
            HarmonyHelper.EnsureHarmonyInstalled();
        }
        
        public void OnSettingsUI(UIHelperBase helper)
        {
            // get the date format from the config file
            DateFormatConfiguration config = Configuration<DateFormatConfiguration>.Load();

            // create a heading
            UIHelperBase group = helper.AddGroup("Date Format Options");

            // show current date in the selected format
            UITextField currentDate = (UITextField)group.AddTextfield("Today in specified format", "xxxx", (string sel) => { });
            currentDate.Disable();
            currentDate.Show();
            _runNewDateFormat = true;
            NewDateFormat(config, currentDate);
            group.AddSpace(30);

            // date part order
            string[] orders = new string[] { DateFormatConfiguration.OrderDMY, DateFormatConfiguration.OrderMDY, DateFormatConfiguration.OrderYMD };
            int defaultOrder = 0;
            for (int i = 0; i < orders.Length; i++)
            {
                if (orders[i] == config.Order)
                {
                    defaultOrder = i;
                    break;
                }
            }
            UIDropDown order = (UIDropDown)group.AddDropdown("Date part order", orders, defaultOrder, (int sel) =>
            {
                config.Order = orders[sel];
                NewDateFormat(config, currentDate);
            });
            order.textScale = 1.125f;
            order.verticalAlignment = UIVerticalAlignment.Middle;
            order.autoSize = false;
            order.size = new Vector2(order.size.x, order.size.y - 5f);
            order.builtinKeyNavigation = true;

            // date part separator
            string[] separators = new string[] { DateFormatConfiguration.SeparatorSlash, DateFormatConfiguration.SeparatorPeriod, DateFormatConfiguration.SeparatorDash, DateFormatConfiguration.SeparatorSpace };
            int defaultSeparator = 0;
            for (int i = 0; i < separators.Length; i++)
            {
                if (separators[i] == config.Separator)
                {
                    defaultSeparator = i;
                    break;
                }
            }
            UIDropDown separator = (UIDropDown)group.AddDropdown("Date part separator", separators, defaultSeparator, (int sel) =>
            {
                config.Separator = separators[sel];
                NewDateFormat(config, currentDate);
            });
            separator.textScale = 1.125f;
            separator.verticalAlignment = UIVerticalAlignment.Middle;
            separator.autoSize = false;
            separator.size = new Vector2(separator.size.x, separator.size.y - 5f);
            separator.builtinKeyNavigation = true;

            // month leading zero
            UICheckBox monthLeadginZero = (UICheckBox)group.AddCheckbox("Leading zero on months less than 10", config.MonthLeadingZero, (bool isChecked) =>
            {
                config.MonthLeadingZero = isChecked;
                NewDateFormat(config, currentDate);
            });

            // day leading zero
            UICheckBox dayLeadginZero = (UICheckBox)group.AddCheckbox("Leading zero on days less than 10", config.DayLeadingZero, (bool isChecked) =>
            {
                config.DayLeadingZero = isChecked;
                NewDateFormat(config, currentDate);
            });

            // reset to default
            group.AddSpace(10);
            UIButton resetToDefault = (UIButton)group.AddButton(" Reset To Default ", () =>
            {
                // each of the following will trigger the above Changed events if the default value is different than the current value
                // prevent NewDateFormat from running multiple times, once for each option setting that is changed
                _runNewDateFormat = false;

                // set default order
                defaultOrder = 0;
                for (int i = 0; i < orders.Length; i++)
                {
                    if (orders[i] == DateFormatConfiguration.DefaultOrder)
                    {
                        defaultOrder = i;
                        break;
                    }
                }
                order.selectedIndex = defaultOrder;

                // set default separator
                defaultSeparator = 0;
                for (int i = 0; i < separators.Length; i++)
                {
                    if (separators[i] == DateFormatConfiguration.DefaultSeparator)
                    {
                        defaultSeparator = i;
                        break;
                    }
                }
                separator.selectedIndex = defaultSeparator;

                // set default month and day leading zero
                monthLeadginZero.isChecked = DateFormatConfiguration.DefaultMonthLeadingZero;
                dayLeadginZero.isChecked = DateFormatConfiguration.DefaultDayLeadingZero;

                // now run new date format, even if nothing changed
                _runNewDateFormat = true;
                NewDateFormat(config, currentDate);
            });
            resetToDefault.textScale = 1f;
        }
        
        /// <summary>
        /// update things when the date format is changed
        /// </summary>
        public void NewDateFormat(DateFormatConfiguration config, UITextField currentDate)
        {
            try
            {
                if (_runNewDateFormat)
                {
                    // save the new format
                    Configuration<DateFormatConfiguration>.Save();

                    // on the options panel, display the current date in the new format
                    currentDate.text = DateTime.Now.ToString(config.BuildDateFormatString());

                    // reapply Harmony patches
                    HarmonyPatcher.ReapplyPatches();
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}