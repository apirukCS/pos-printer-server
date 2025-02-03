using System;
using System.Globalization;

public class DateTimeHelper
{
    private readonly CultureInfo thaiCulture;
    private readonly CultureInfo englishCulture;

    public DateTimeHelper()
    {
        thaiCulture = new CultureInfo("th-TH");
        thaiCulture.DateTimeFormat.Calendar = new ThaiBuddhistCalendar(); // ใช้ปฏิทินไทย

        englishCulture = new CultureInfo("en-US");
    }

    public string GetCurrentDate(string language,bool isNumber = false)
    {
        DateTime currentDate = DateTime.Now;
        return FormatDate(currentDate, language, isNumber);
    }

    public string GetFormattedDate(DateTime date, string language)
    {
        return FormatDate(date, language);
    }

    public string GetCurrentDateTime(string language)
    {
        DateTime currentDate = DateTime.Now;
        return $"{FormatDate(currentDate, language)} {FormatTime(currentDate, language)}";
    }

    private string FormatDate(DateTime date, string language, bool isNumber = false)
    {
        if (isNumber) {
            CultureInfo thaiCulture = new CultureInfo(language == "en" ? "en-US" : "th-TH");
            var d = date.ToString("d/M/yyyy", thaiCulture);
            return d;
        }
        
        if (language == "en")
        {
            return date.ToString("dd MMMM yyyy", englishCulture);
        }
        else
        {
            return date.ToString("dd MMMM yyyy", thaiCulture);
        }
    }

    private string FormatTime(DateTime dateTime, string language)
    {
        
        if (language == "en")
        {
            return "at " + dateTime.ToString("hh:mm tt", englishCulture);
        }
        else
        {
            return "เวลา " + dateTime.ToString("HH:mm", thaiCulture) + " น.";
        }
    }

    public string GetMonthName(int month, string language)
    {
        var date = new DateTime(DateTime.Now.Year, month, 1);
        if (language == "en")
        {
            return date.ToString("MMMM", englishCulture);
        }
        else
        {
            return date.ToString("MMMM", thaiCulture);
        }
    }
}
