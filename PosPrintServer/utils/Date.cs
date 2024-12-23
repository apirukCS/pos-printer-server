using System;
using System.Globalization;

public class DateTimeHelper
{
    private readonly CultureInfo thaiCulture;
    private readonly CultureInfo englishCulture;

    public DateTimeHelper()
    {
        // กำหนด CultureInfo สำหรับไทยและภาษาอังกฤษ
        thaiCulture = new CultureInfo("th-TH");
        thaiCulture.DateTimeFormat.Calendar = new ThaiBuddhistCalendar(); // ใช้ปฏิทินไทย

        englishCulture = new CultureInfo("en-US");
    }

    // ฟังก์ชันที่คืนวันที่ปัจจุบันในรูปแบบ "dd MMMM yyyy" ตามภาษาที่ระบุ
    public string GetCurrentDate(string language)
    {
        DateTime currentDate = DateTime.Now;
        return FormatDate(currentDate, language);
    }

    // ฟังก์ชันที่คืนวันที่ที่กำหนดในรูปแบบ "dd MMMM yyyy" ตามภาษาที่ระบุ
    public string GetFormattedDate(DateTime date, string language)
    {
        return FormatDate(date, language);
    }

    // ฟังก์ชันที่คืนวันที่และเวลาในรูปแบบ "dd MMMM yyyy HH:mm:ss" ตามภาษาที่ระบุ
    public string GetCurrentDateTime(string language)
    {
        DateTime currentDate = DateTime.Now;
        return $"{FormatDate(currentDate, language)} {FormatTime(currentDate, language)}";
    }

    // ฟังก์ชันภายในเพื่อจัดรูปแบบวันที่ตามภาษา
    private string FormatDate(DateTime date, string language)
    {
        if (language == "th")
        {
            return date.ToString("dd MMMM yyyy", thaiCulture);
        }
        else if (language == "en")
        {
            return date.ToString("dd MMMM yyyy", englishCulture);
        }
        else
        {
            throw new ArgumentException("Invalid language specified. Use 'th' or 'en'.");
        }
    }

    private string FormatTime(DateTime dateTime, string language)
    {
        if (language == "th")
        {
            // เพิ่มคำว่า "เวลา" ก่อนเวลาในภาษาไทย
            return "เวลา " + dateTime.ToString("HH:mm", thaiCulture) + " น.";
        }
        else if (language == "en")
        {
            // เพิ่มคำว่า "Time" ก่อนเวลาในภาษาอังกฤษ
            return "Time " + dateTime.ToString("HH:mm", englishCulture);
        }
        else
        {
            throw new ArgumentException("Invalid language specified. Use 'th' or 'en'.");
        }
    }
}
