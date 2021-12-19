namespace DateFormat
{
    /// <summary>
    /// define global (i.e. for this mod but not game specific) configuration properties
    /// </summary>
    /// <remarks>convention for the config file name seems to be the mod name + "Config.xml"</remarks>
    [ConfigurationFileName("DateFormatConfig.xml")]
    public class DateFormatConfiguration
    {
        // define constants for configuration options
        public const string OrderDMY = "DMY";
        public const string OrderMDY = "MDY";
        public const string OrderYMD = "YMD";

        public const string SeparatorSlash  = "Slash";
        public const string SeparatorPeriod = "Period";
        public const string SeparatorDash   = "Dash";
        public const string SeparatorSpace  = "Space";

        // it is important to set default config values in case there is no config file
        public const string DefaultOrder = OrderDMY;
        public const string DefaultSeparator = SeparatorSlash;
        public const bool DefaultMonthLeadingZero = true;
        public const bool DefaultDayLeadingZero = true;

        // date format options
        public string Order = DefaultOrder;
        public string Separator = DefaultSeparator;
        public bool MonthLeadingZero = DefaultMonthLeadingZero;
        public bool DayLeadingZero = DefaultDayLeadingZero;

        /// <summary>
        /// build a date format string based on the configured options
        /// </summary>
        public string BuildDateFormatString()
        {
            // get date part separator
            string separator = "/";
            switch (Separator)
            {
                case SeparatorSlash:  separator = "/"; break;
                case SeparatorPeriod: separator = "."; break;
                case SeparatorDash:   separator = "-"; break;
                case SeparatorSpace:  separator = " "; break;
            }

            // get year, month, and day formats
            const string yearFormat = "yyyy";
            string monthFormat = (MonthLeadingZero ? "MM" : "M");
            string dayFormat = (DayLeadingZero ? "dd" : "d");

            // put it all together in the configured order
            string dateFormat = "dd/MM/yyyy";
            switch (Order)
            {
                case OrderDMY: dateFormat = dayFormat + separator + monthFormat + separator + yearFormat; break;
                case OrderMDY: dateFormat = monthFormat + separator + dayFormat + separator + yearFormat; break;
                case OrderYMD: dateFormat = yearFormat + separator + monthFormat + separator + dayFormat; break;
            }

            // return the date format
            return dateFormat;
        }
    }
}