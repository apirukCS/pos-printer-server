using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

public class PrinterManager
{
    private static ConcurrentDictionary<string, IntPtr> connectedPrinters = new ConcurrentDictionary<string, IntPtr>();

    public static IntPtr? GetPrinterConnection(string ipAddress)
    {
        IntPtr printer = ESCPOS.InitPrinter("");
        int s = ESCPOS.OpenPort(printer, $"NET,{ipAddress}");
        if (s != 0)
        {
            return null;
        }
        //connectedPrinters[ipAddress] = printer;
        return printer;

        //if (connectedPrinters.ContainsKey(ipAddress))
        //{
        //    return connectedPrinters[ipAddress];
        //}
        //elset
        //{
        //    IntPtr printer = ESCPOS.InitPrinter("");
        //    int s = ESCPOS.OpenPort(printer, $"NET,{ipAddress}");
        //    connectedPrinters[ipAddress] = printer;
        //    return printer;
        //}
    }

    public static void PrintSymbol(IntPtr printer, string data)
    {
        ESCPOS.PrintSymbol(printer, 49, data, 48, 10, 10, 1);
        //PrintSymbol(printer, 49, test, 48, 10, 10, 1);
        NewLine(printer);
    }

    public static IntPtr GetPrinterConnectionOnly(string ipAddress) {
        IntPtr printer = ESCPOS.InitPrinter("");
        int s = ESCPOS.OpenPort(printer, $"NET,{ipAddress}");
        return printer;
    }

    public static void ClosePort(IntPtr printer) {
        ESCPOS.ClosePort(printer);
    }

    public static string GetPrinterStatus(IntPtr printer,int status) {
        int ret = ESCPOS.GetPrinterState(printer,ref status);
        ESCPOS.ClosePort(printer);
        if (ret == 0)
        {
            if (0x12 == status)
            {
                return "Ready";
            }
            else if ((status & 0b100) > 0)
            {
                return "Cover opened";
            }
            else if ((status & 0b1000) > 0)
            {
                return "Feed button has been pressed";
            }
            else if ((status & 0b100000) > 0)
            {
                return "Printer is out of paper";
            }
            else if ((status & 0b1000000) > 0)
            {
                return "Error condition";
            }
            else
            {
                return "Other Error";
            }
        }
        else if (ret == -2)
        {
            return "Failed with invalid handle";
        }
        else if (ret == -1)
        {
            return "Invalid argument";
        }
        else if (ret == -4)
        {
            return "Failed, out of memory";
        }
        else if (ret == -9)
        {
            return "Failed to send data";
        }
        else if (ret == -10)
        {
            return "Write data timed out";
        }
        else {
            return "Failed to connection";
        }
        
    }

    // Method to close a printer connection
    //public static void ClosePrinterConnection(string ipAddress)
    //{
    //    if (connectedPrinters.ContainsKey(ipAddress))
    //    {
    //        IntPtr printer = connectedPrinters[ipAddress];
    //        ESCPOS.ClosePort(printer);
    //        connectedPrinters.Remove(ipAddress);
    //        Console.WriteLine($"Closed connection to printer at {ipAddress}");
    //    }
    //}

    // Close all printer connections
    //public static void CloseAllConnections()
    //{
    //    foreach (var printer in connectedPrinters.Values)
    //    {
    //        ESCPOS.ClosePort(printer);
    //    }
    //    connectedPrinters.Clear();
    //    Console.WriteLine("Closed all printer connections.");
    //}

    //-------------------------------ESC Command------------------------------------------
    public static void PrintText(IntPtr printer, string text, bool isFormatText = true, int? lineSpace = null)
    {
        //ESCPOS.SetCodePage(printer, 255);
        //if (isFormatText) {
        //    text = FormatTextNormal(text);
        //}
        SetTextSize(printer, 1);
        //MessageBox.Show($"PrintText {text}");
        byte[] textBytes = Encoding.GetEncoding("TIS-620").GetBytes(text);
        ESCPOS.WriteData(printer, textBytes, textBytes.Length);
        NewLine(printer, lineSpace);
    }

    public static void PrintTextOnly(IntPtr printer, string text)
    {
        SetTextSize(printer, 1);
        byte[] textBytes = Encoding.GetEncoding("TIS-620").GetBytes(text);
        ESCPOS.WriteData(printer, textBytes, textBytes.Length);
    }

    public static void PrintTextBold(IntPtr printer, string text,bool? isNewLine = true)
    {
        SetTextSize(printer, 2);
        //MessageBox.Show($"PrintTextBold {text}");
        byte[] textBytes = Encoding.GetEncoding("TIS-620").GetBytes(text);
        ESCPOS.WriteData(printer, textBytes, textBytes.Length);
        if (isNewLine == false) return;
        NewLine(printer);
    }

    public static void PrintTextTitleAndSubTitle(IntPtr printer, string title, string subTitle) {
        SetTextSize(printer, 1);
        byte[] textBytes = Encoding.GetEncoding("TIS-620").GetBytes(title);
        ESCPOS.WriteData(printer, textBytes, textBytes.Length);
        SetTextSize(printer, 2);
        //MessageBox.Show($"PrintTextTitleAndSub {title}{subTitle}");
        textBytes = Encoding.GetEncoding("TIS-620").GetBytes(subTitle);
        ESCPOS.WriteData(printer, textBytes, textBytes.Length);
        NewLine(printer);
    }

    public static void PrintTextMediumBold(IntPtr printer, string text,bool isFormatText = true)
    {
        SetTextSize(printer, 3);
        if (isFormatText)
        {
            text = FormatTextNormal(text, 30);
        }
        byte[] textBytes = Encoding.GetEncoding("TIS-620").GetBytes(text);
        ESCPOS.WriteData(printer, textBytes, textBytes.Length);
        NewLine(printer);
    }

    public static void PrintTextLargeBold(IntPtr printer, string text)
    {
        SetTextSize(printer, 4);
        byte[] textBytes = Encoding.GetEncoding("TIS-620").GetBytes(text);
        ESCPOS.WriteData(printer, textBytes, textBytes.Length);
        NewLine(printer);
    }

    public static void PrintText3(IntPtr printer, string text)
    {
        TextBold(printer);
        SetTextSize(printer, 3);
        byte[] textBytes = Encoding.GetEncoding("TIS-620").GetBytes(text);
        ESCPOS.WriteData(printer, textBytes, textBytes.Length);
        NewLine(printer);
    }

    public static void PrintText5(IntPtr printer, string text) {
        TextBold(printer);
        SetTextSize(printer, 5);
        byte[] textBytes = Encoding.GetEncoding("TIS-620").GetBytes(text);
        ESCPOS.WriteData(printer, textBytes, textBytes.Length);
        NewLine(printer);
    }

    public static void PrintText6(IntPtr printer, string text)
    {
        TextBold(printer);
        SetTextSize(printer, 6);
        byte[] textBytes = Encoding.GetEncoding("TIS-620").GetBytes(text);
        ESCPOS.WriteData(printer, textBytes, textBytes.Length);
        NewLine(printer);
    }

    //correct
    public static void PrintTextTwoColumn(IntPtr printer, string textLeft, string textRight, int size = 1)
    {
        SetTextSize(printer, size);
        int maxLineLength = 42;
        StringBuilder output = new StringBuilder();
        string textLeftFiltered = Regex.Replace(textLeft, "[\u0E31\u0E34-\u0E3A\u0E47-\u0E4D]", "");
        int effectiveLeftLength = textLeftFiltered.Length;
        int availableSpaceForLeft = maxLineLength - textRight.Length - 1;

        //effectiveLeftLength = ความยาวในแนวนอน
        if (effectiveLeftLength > availableSpaceForLeft)
        {
            string sub1 = textLeft.Substring(0, availableSpaceForLeft);
            char[] topDiacritics = { 'ิ', 'ี', 'ึ', 'ื', '่', '้', '๊', '๋', 'ั','์','ํ','็' };
            char[] bottomDiacritics = { 'ุ', 'ู' };

            int tp = sub1.Sum(c => topDiacritics.Contains(c) ? 1 : 0);
            int bc = sub1.Sum(c => bottomDiacritics.Contains(c) ? 1 : 0);

            string sub2 = textLeft.Substring(0, availableSpaceForLeft + tp + bc);
            int topCount = sub2.Sum(c => topDiacritics.Contains(c) ? 1 : 0);
            int bottomCount = sub2.Sum(c => bottomDiacritics.Contains(c) ? 1 : 0);
            availableSpaceForLeft = availableSpaceForLeft + topCount + bottomCount;

            output.Append($"{textLeft.Substring(0, availableSpaceForLeft)} " + textRight + "\r\n");
            
            string remainingLeftText = textLeft.Substring(availableSpaceForLeft);
            while (remainingLeftText.Length > maxLineLength)
            {
                string part = remainingLeftText.Substring(0, maxLineLength);
                output.Append(part + "\r\n");
                remainingLeftText = remainingLeftText.Substring(maxLineLength);
            }

            if (remainingLeftText.Length > 0)
            {
                output.Append(remainingLeftText + "\r\n");
            }
        }
        else
        {
            int spaceBetween = maxLineLength - (effectiveLeftLength + textRight.Length);
            string line = textLeft + new string(' ', spaceBetween) + textRight;
            output.Append(line + "\r\n");
        }
        //MessageBox.Show($"PrintTextTwoColumn\n {output.ToString()}");
        byte[] textBytes = Encoding.GetEncoding("TIS-620").GetBytes(output.ToString());
        ESCPOS.WriteData(printer, textBytes, textBytes.Length);
    }

    //public static void PrintTextTwoColumn(IntPtr printer, string textLeft, string textRight, int size = 1)
    //{
    //    SetTextSize(printer, size);
    //    int maxLineLength = 42;
    //    int spaceBetween = 5;

    //    int spaceForLeftText = maxLineLength - textRight.Length - spaceBetween;
    //    StringBuilder output = new StringBuilder();

    //    if (textLeft.Length > spaceForLeftText)
    //    {
    //        string leftPart1 = textLeft.Substring(0, spaceForLeftText);
    //        output.Append(leftPart1 + new string(' ', spaceBetween) + textRight + "\r\n");
    //        string remainingLeftText = textLeft.Substring(spaceForLeftText);
    //        while (remainingLeftText.Length > maxLineLength)
    //        {
    //            string part = remainingLeftText.Substring(0, maxLineLength);
    //            output.Append(part + "\r\n");
    //            remainingLeftText = remainingLeftText.Substring(maxLineLength);
    //        }

    //        if (remainingLeftText.Length > 0)
    //        {
    //            output.Append(remainingLeftText + "\r\n");
    //        }
    //    }
    //    else
    //    {
    //        spaceBetween = maxLineLength - (textLeft.Length + textRight.Length);
    //        MessageBox.Show($"spaceBetween {spaceBetween} ::: textLeft.Length {textLeft.Length} ::: textRight.Length {textRight.Length}");
    //        string line = textLeft + new string(' ', spaceBetween) + textRight;
    //        output.Append(line + "\r\n");
    //    }

    //    byte[] textBytes = Encoding.GetEncoding("TIS-620").GetBytes(output.ToString());
    //    ESCPOS.WriteData(printer, textBytes, textBytes.Length);
    //}

    //new string(' ', availableSpaceForMiddle)

    public static void PrintTextThreeColumn(IntPtr printer, string textLeft, string textMiddle, string textRight, int maxLineLength = 42, int leftColumnWidth = 7)
    {
        SetTextSize(printer, 1);
        int spaceBetween = 3;
        StringBuilder output = new StringBuilder();
        string leftTextTrimmed = textLeft.Length > leftColumnWidth ? textLeft.Substring(0, leftColumnWidth) : textLeft.PadRight(leftColumnWidth);

        string middleTextFiltered = Regex.Replace(textMiddle, "[\u0E31\u0E34-\u0E3A\u0E47-\u0E4D]", "");
        int effectiveMiddleLength = middleTextFiltered.Length;
        int availableSpaceForMiddle = maxLineLength - leftTextTrimmed.Length - textRight.Length - spaceBetween;

        if (effectiveMiddleLength > availableSpaceForMiddle)
        {
            string sub1 = textMiddle.Substring(0, availableSpaceForMiddle);
            string sub2 = textMiddle.Substring(0, availableSpaceForMiddle + SpecialCharacterCount(sub1));
            availableSpaceForMiddle = availableSpaceForMiddle + SpecialCharacterCount(sub2);
            string t = textMiddle.Substring(0, availableSpaceForMiddle);
            //MessageBox.Show($"kk leftTextTrimmed({leftTextTrimmed.Length}) \ntextMiddle({t.Length - SpecialCharacterCount(t)}) \nspaceBetween({spaceBetween}) \ntextRight({textRight.Length})");
            output.Append($"{leftTextTrimmed}{textMiddle.Substring(0, availableSpaceForMiddle)}{new string(' ', spaceBetween)}" + textRight + "\r\n");

            string remainingMiddleText = textMiddle.Substring(availableSpaceForMiddle); //
            while ((remainingMiddleText.Length + SpecialCharacterCount(remainingMiddleText)) > availableSpaceForMiddle) //
            {
                availableSpaceForMiddle = maxLineLength - leftTextTrimmed.Length - textRight.Length - spaceBetween;
                string part1 = remainingMiddleText.Substring(0, availableSpaceForMiddle);
                string part2 = remainingMiddleText.Substring(0, availableSpaceForMiddle + SpecialCharacterCount(part1));
                output.Append($"{new string(' ', leftTextTrimmed.Length)}{part2}{new string(' ', textRight.Length - 1)}" + "\r\n");
                remainingMiddleText = remainingMiddleText.Substring(availableSpaceForMiddle + SpecialCharacterCount(part1));
            }

            if (remainingMiddleText.Length > 0)
            {
                output.Append($"{new string(' ', leftTextTrimmed.Length)}{remainingMiddleText}" + "\r\n");
            }
        }
        else
        {
            // ถ้าไม่เกินความยาวที่กำหนด
            spaceBetween = maxLineLength - (leftTextTrimmed.Length + effectiveMiddleLength + textRight.Length);
            string line = leftTextTrimmed + textMiddle + new string(' ', spaceBetween) + textRight;
            output.Append(line + "\r\n");
        }
        //MessageBox.Show($"PrintTextThreeColumn \n{output.ToString()}");
        byte[] textBytes = Encoding.GetEncoding("TIS-620").GetBytes(output.ToString());
        ESCPOS.WriteData(printer, textBytes, textBytes.Length);
    }

    //private static int SpecialCharacterCount(string text)
    //{
    //    char[] topDiacritics = { 'ิ', 'ี', 'ึ', 'ื', '่', '้', '๊', '๋', 'ั', '์', 'ํ', '็' };
    //    char[] bottomDiacritics = { 'ุ', 'ู' };

    //    int specialCharacterCount = 0;
    //    bool hasTopDiacritic = false;
    //    bool hasBottomDiacritic = false;

    //    foreach (char c in text)
    //    {
    //        // เช็คว่าตัวอักษรเป็นสระบน
    //        if (topDiacritics.Contains(c))
    //        {
    //            if (!hasTopDiacritic) // ถ้ายังไม่มีสระบน
    //            {
    //                specialCharacterCount++;
    //                hasTopDiacritic = true; // ตั้งค่าคำว่า "มีสระบน"
    //            }
    //        }
    //        // เช็คว่าตัวอักษรเป็นสระล่าง
    //        else if (bottomDiacritics.Contains(c))
    //        {
    //            if (!hasBottomDiacritic) // ถ้ายังไม่มีสระล่าง
    //            {
    //                specialCharacterCount++;
    //                hasBottomDiacritic = true; // ตั้งค่าคำว่า "มีสระล่าง"
    //            } 
    //        }
    //        // ถ้าตัวอักษรไม่ใช่สระหรือวรรณยุกต์
    //        else
    //        {
    //            // รีเซ็ตสถานะสระเมื่อเจอตัวอักษรใหม่ที่ไม่ใช่สระ
    //            hasTopDiacritic = false;
    //            hasBottomDiacritic = false;
    //        }
    //    }

    //    return specialCharacterCount;
    //}

    private static int SpecialCharacterCount(string text)
    {
        char[] topDiacritics = { 'ิ', 'ี', 'ึ', 'ื', '่', '้', '๊', '๋', 'ั', '์', 'ํ', '็' };
        char[] bottomDiacritics = { 'ุ', 'ู' };

        int specialCharacterCount = 0;
        bool hasTopDiacritic = false;
        bool hasBottomDiacritic = false;

        foreach (char c in text)
        {
            if (topDiacritics.Contains(c))
            {
                specialCharacterCount++;
                //hasTopDiacritic = true;
            }
            else if (bottomDiacritics.Contains(c))
            {
                specialCharacterCount++;
                //hasBottomDiacritic = true;
            }
            //else if (!topDiacritics.Contains(c) && !bottomDiacritics.Contains(c))
            //{
            //    hasTopDiacritic = false;
            //    hasBottomDiacritic = false;
            //}
            //if (topDiacritics.Contains(c) && !hasTopDiacritic)
            //{
            //    specialCharacterCount++;
            //    hasTopDiacritic = true;
            //}
            //else if (bottomDiacritics.Contains(c) && !hasBottomDiacritic)
            //{
            //    specialCharacterCount++;
            //    hasBottomDiacritic = true;
            //}
            //else if (!topDiacritics.Contains(c) && !bottomDiacritics.Contains(c))
            //{
            //    hasTopDiacritic = false;
            //    hasBottomDiacritic = false;
            //}
        }

        return specialCharacterCount;
    }

    //origi
    //private static int SpecialCharacterCount(string text)
    //{
    //    char[] topDiacritics = { 'ิ', 'ี', 'ึ', 'ื', '่', '้', '๊', '๋', 'ั', '์', 'ํ', '็' };
    //    char[] bottomDiacritics = { 'ุ', 'ู' };

    //    int specialCharacterCount = 0;
    //    bool hasTopDiacritic = false;
    //    bool hasBottomDiacritic = false;

    //    foreach (char c in text)
    //    {
    //        if (topDiacritics.Contains(c) && !hasTopDiacritic)
    //        {
    //            specialCharacterCount++;
    //            hasTopDiacritic = true;
    //        }
    //        else if (bottomDiacritics.Contains(c) && !hasBottomDiacritic)
    //        {
    //            specialCharacterCount++;
    //            hasBottomDiacritic = true;
    //        }
    //        else if (!topDiacritics.Contains(c) && !bottomDiacritics.Contains(c))
    //        {
    //            hasTopDiacritic = false;
    //            hasBottomDiacritic = false;
    //        }
    //    }

    //    return specialCharacterCount;
    //}


    //private static int SpecialCharacterCount(string text) {
    //    char[] topDiacritics = { 'ิ', 'ี', 'ึ', 'ื', '่', '้', '๊', '๋', 'ั', '์', 'ํ', '็' };
    //    char[] bottomDiacritics = { 'ุ', 'ู' };

    //    int top = text.Sum(c => topDiacritics.Contains(c) ? 1 : 0);
    //    int bottom = text.Sum(c => bottomDiacritics.Contains(c) ? 1 : 0);
    //    return top + bottom;
    //}

    //pop
    //correct
    //public static void PrintTextThreeColumn(IntPtr printer, string textLeft, string textMiddle, string textRight, int maxLineLength = 42, int leftColumnWidth = 7)
    //{
    //    SetTextSize(printer, 1);
    //    StringBuilder output = new StringBuilder();
    //    string leftTextTrimmed = textLeft.Length > leftColumnWidth ? textLeft.Substring(0, leftColumnWidth) : textLeft.PadRight(leftColumnWidth);

    //    string middleTextFiltered = Regex.Replace(textMiddle, "[\u0E31\u0E34-\u0E3A\u0E47-\u0E4D]", "");
    //    int effectiveMiddleLength = middleTextFiltered.Length;
    //    int availableSpaceForMiddle = maxLineLength - leftTextTrimmed.Length - textRight.Length - 1;

    //    if (effectiveMiddleLength > availableSpaceForMiddle)
    //    {
    //        string sub1 = textMiddle.Substring(0, availableSpaceForMiddle);
    //        char[] topDiacritics = { 'ิ', 'ี', 'ึ', 'ื', '่', '้', '๊', '๋', 'ั', '์', 'ํ', '็' };
    //        char[] bottomDiacritics = { 'ุ', 'ู' };

    //        int tp = sub1.Sum(c => topDiacritics.Contains(c) ? 1 : 0);
    //        int bc = sub1.Sum(c => bottomDiacritics.Contains(c) ? 1 : 0);

    //        string sub2 = textMiddle.Substring(0, availableSpaceForMiddle + tp + bc);
    //        int topCount = sub2.Sum(c => topDiacritics.Contains(c) ? 1 : 0);
    //        int bottomCount = sub2.Sum(c => bottomDiacritics.Contains(c) ? 1 : 0);
    //        availableSpaceForMiddle = availableSpaceForMiddle + topCount + bottomCount;

    //        output.Append($"{leftTextTrimmed}{textMiddle.Substring(0, availableSpaceForMiddle)} " + textRight + "\r\n");

    //        string remainingMiddleText = textMiddle.Substring(availableSpaceForMiddle);
    //        while (remainingMiddleText.Length > maxLineLength)
    //        {
    //            string part = remainingMiddleText.Substring(0, maxLineLength);
    //            output.Append(part + "\r\n");
    //            remainingMiddleText = remainingMiddleText.Substring(maxLineLength);
    //        }

    //        if (remainingMiddleText.Length > 0)
    //        {
    //            output.Append(remainingMiddleText + "\r\n");
    //        }
    //    }
    //    else
    //    {
    //        // ถ้าไม่เกินความยาวที่กำหนด
    //        int spaceBetween = maxLineLength - (leftTextTrimmed.Length + effectiveMiddleLength + textRight.Length);
    //        string line = leftTextTrimmed + textMiddle + new string(' ', spaceBetween) + textRight;
    //        output.Append(line + "\r\n");
    //    }
    //    MessageBox.Show($"PrintTextThreeColumn {output.ToString()}");
    //    byte[] textBytes = Encoding.GetEncoding("TIS-620").GetBytes(output.ToString());
    //    ESCPOS.WriteData(printer, textBytes, textBytes.Length);
    //}

    //public static void PrintTextThreeColumn(IntPtr printer, string column1, string column2, string column3, int maxLineLength = 42, int col1Width = 7)
    //{
    //    // กรองสระและวรรณยุกต์สำหรับการคำนวณพื้นที่
    //    string column2Filtered = Regex.Replace(column2, "[\u0E31\u0E34-\u0E3A\u0E47-\u0E4D]", "");

    //    // คำนวณพื้นที่ที่เหลือสำหรับคอลัมน์ที่สองในบรรทัดแรก
    //    int spaceForCol2 = maxLineLength - col1Width - column3.Length;
    //    StringBuilder output = new StringBuilder();

    //    // แยกข้อความในคอลัมน์ที่หนึ่งให้อยู่ภายใน col1Width
    //    string col1Text = column1.PadRight(col1Width);

    //    // ตรวจสอบว่าคอลัมน์ที่สองเกินพื้นที่ที่เหลือหรือไม่
    //    if (column2Filtered.Length > spaceForCol2)
    //    {
    //        // กรณีที่คอลัมน์ที่สองเกินพื้นที่ ให้แบ่งเป็นสองส่วน
    //        string col2Part1 = column2.Substring(0, spaceForCol2);
    //        string col2Remaining = column2.Substring(spaceForCol2);

    //        // แสดงบรรทัดแรก (คอลัมน์ที่หนึ่งและคอลัมน์ที่สาม)
    //        output.Append(col1Text + col2Part1 + new string(' ', maxLineLength - col1Width - col2Part1.Length - column3.Length) + column3 + "\r\n");

    //        // จัดการบรรทัดถัดไปโดยให้คอลัมน์ที่สองอยู่ภายใต้พื้นที่สูงสุด
    //        while (col2Remaining.Length > 0)
    //        {
    //            // คำนวณจำนวนตัวอักษรที่เหลือได้ในบรรทัดนี้
    //            int remainingSpace = maxLineLength - col1Width;
    //            string linePart;

    //            if (col2Remaining.Length > remainingSpace)
    //            {
    //                linePart = col2Remaining.Substring(0, remainingSpace);
    //                col2Remaining = col2Remaining.Substring(remainingSpace);
    //            }
    //            else
    //            {
    //                linePart = col2Remaining;
    //                col2Remaining = string.Empty;
    //            }

    //            // แสดงบรรทัดต่อไป (คอลัมน์ที่สอง)
    //            output.Append(new string(' ', col1Width) + linePart + "\r\n");
    //        }
    //    }
    //    else
    //    {
    //        // กรณีที่คอลัมน์ที่สองไม่เกินพื้นที่
    //        string line = col1Text + column2 + new string(' ', maxLineLength - col1Width - column2.Length - column3.Length) + column3;
    //        output.Append(line + "\r\n");
    //    }

    //    // ส่งข้อมูลไปที่เครื่องพิมพ์
    //    byte[] textBytes = Encoding.GetEncoding("TIS-620").GetBytes(output.ToString());
    //    ESCPOS.WriteData(printer, textBytes, textBytes.Length);
    //}


    //it correct
    //public static void PrintTextThreeColumn(IntPtr printer, string column1, string column2, string column3, int maxLineLength = 42)
    //{
    //    int col1Width = 7; // ความกว้างคงที่ของคอลัมน์แรก
    //    int spaceForCol2 = maxLineLength - col1Width - column3.Length; // พื้นที่ที่เหลือสำหรับคอลัมน์ที่สอง

    //    // ลบสระและวรรณยุกต์ในคอลัมน์ที่สองเพื่อการคำนวณความยาว
    //    string column2Filtered = Regex.Replace(column2, "[\u0E31\u0E34-\u0E3A\u0E47-\u0E4D]", "");
    //    int effectiveCol2Length = column2Filtered.Length;

    //    StringBuilder output = new StringBuilder();

    //    // สร้างส่วนของคอลัมน์ที่หนึ่งด้วยความกว้างคงที่
    //    string col1Text = column1.PadRight(col1Width);

    //    // ตรวจสอบว่าคอลัมน์ที่สองมีความยาวเกินหรือไม่
    //    if (effectiveCol2Length > spaceForCol2)
    //    {
    //        // แยกข้อความส่วนแรกของคอลัมน์ที่สอง
    //        string col2Part1 = column2.Substring(0, spaceForCol2);
    //        output.Append(col1Text + col2Part1 + new string(' ', maxLineLength - col1Width - col2Part1.Length - column3.Length) + column3 + "\r\n");

    //        // แยกข้อความที่เหลือของคอลัมน์ที่สองและพิมพ์บรรทัดใหม่
    //        string remainingCol2Text = column2.Substring(spaceForCol2);
    //        while (remainingCol2Text.Length > 0)
    //        {
    //            // แยกบรรทัดถัดไปโดยมีพื้นที่สำหรับคอลัมน์ที่สองทั้งหมด
    //            string col2Line = remainingCol2Text.Length > maxLineLength - col1Width
    //                ? remainingCol2Text.Substring(0, maxLineLength - col1Width)
    //                : remainingCol2Text;

    //            output.Append(new string(' ', col1Width) + col2Line + "\r\n");
    //            remainingCol2Text = remainingCol2Text.Substring(col2Line.Length);
    //        }
    //    }
    //    else
    //    {
    //        // ถ้าความยาวคอลัมน์ที่สองไม่เกินพื้นที่ที่กำหนด
    //        string line = col1Text + column2 + new string(' ', spaceForCol2 - effectiveCol2Length) + column3;
    //        output.Append(line + "\r\n");
    //    }

    //    // ส่งข้อความไปยังเครื่องพิมพ์
    //    byte[] textBytes = Encoding.GetEncoding("TIS-620").GetBytes(output.ToString());
    //    ESCPOS.WriteData(printer, textBytes, textBytes.Length);
    //}


    //public static void PrintTextThreeColumn(IntPtr printer, string textLeft,string textMiddle, string textRight, int size = 1) {
    //    SetTextSize(printer, 1);
    //    int maxLineLength = 48;
    //    int leftWidth = 8;
    //    int spaceBetweenMiddleAndRight = 5;
    //    int rightWidth = textRight.Length;
    //    int spaceForMiddleText = maxLineLength - leftWidth - spaceBetweenMiddleAndRight - rightWidth;
    //    StringBuilder output = new StringBuilder();
    //    if (textMiddle.Length > spaceForMiddleText)
    //    {
    //        string middlePart1 = textMiddle.Substring(0, spaceForMiddleText);
    //        output.Append(textLeft.PadRight(leftWidth) + middlePart1 + new string(' ', spaceBetweenMiddleAndRight) + textRight + "\r\n");
    //        string remainingMiddleText = textMiddle.Substring(spaceForMiddleText);
    //        while (remainingMiddleText.Length > maxLineLength - leftWidth)
    //        {
    //            string part = remainingMiddleText.Substring(0, maxLineLength - leftWidth);
    //            output.Append(new string(' ', leftWidth) + part + "\r\n");
    //            remainingMiddleText = remainingMiddleText.Substring(maxLineLength - leftWidth);
    //        }

    //        if (remainingMiddleText.Length > 0)
    //        {
    //            output.Append(new string(' ', leftWidth) + remainingMiddleText + "\r\n");
    //        }
    //    }
    //    else
    //    {
    //        string line = textLeft.PadRight(leftWidth) + textMiddle + new string(' ', spaceBetweenMiddleAndRight) + textRight;
    //        output.Append(line + "\r\n");
    //    }

    //    byte[] textBytes = Encoding.GetEncoding("TIS-620").GetBytes(output.ToString());
    //    ESCPOS.WriteData(printer, textBytes, textBytes.Length);
    //}

    public static void PrintQueueNumber(IntPtr printer, int number)
    {
        string path;
        if (number < 10)
        {
            // หากตัวเลขมี 1 หลัก
            path = $@"C:\dotnet\PosPrintServer\PosPrintServer\images\numbers\{number}.jpeg";
        }
        else if (number < 100)
        {
            string pathTens = $@"C:\dotnet\PosPrintServer\PosPrintServer\images\numbers\{number / 10}.jpeg";
            string pathOnes = $@"C:\dotnet\PosPrintServer\PosPrintServer\images\numbers\{number % 10}.jpeg";
            string combinedPath = @"C:\dotnet\PosPrintServer\PosPrintServer\images\numbers\combined.jpeg";
            ImageProcessor.MergeTwoImages(pathTens, pathOnes, combinedPath);
            path = combinedPath;
        }
        else
        {
            string pathHundreds = $@"C:\dotnet\PosPrintServer\PosPrintServer\images\numbers\{number / 100}.jpeg";
            string pathTens = $@"C:\dotnet\PosPrintServer\PosPrintServer\images\numbers\{(number % 100) / 10}.jpeg";
            string pathOnes = $@"C:\dotnet\PosPrintServer\PosPrintServer\images\numbers\{number % 10}.jpeg";
            string combinedPath = @"C:\dotnet\PosPrintServer\PosPrintServer\images\numbers\combined.jpeg";
            ImageProcessor.MergeThreeImages(pathHundreds, pathTens, pathOnes, combinedPath);
            path = combinedPath;
        }
        int s = ESCPOS.PrintImage(printer, path, 0);
    }

    public static void PrintImage(IntPtr printer, string path)
    {
        ESCPOS.PrintImage(printer, path, 0);
    }

    public static async Task PrintImageUrl(IntPtr printer, string url, string path, uint width = 200)
    {
        if (string.IsNullOrEmpty(url)) return;
        await ImageProcessor.ProcessImageFromUrlAsync(url, path, width);
        int s = ESCPOS.PrintImage(printer, path, 0);
    }

    public static void NewLine(IntPtr printer,int? lineSpace = null)
    {
        //MessageBox.Show($"lineSpace {lineSpace}");
        if (lineSpace != null)
        {
            LineSpace(printer, lineSpace);
        }
        ESCPOS.WriteData(printer, new byte[] { 0x0A }, 1);
        if (lineSpace != null)
        {
            LineSpaceDefault(printer);
        }
    }

    public static void CutPaper(IntPtr printer)
    {
        ESCPOS.CutPaperWithDistance(printer, 40);
    }

    public static void AlignCenter(IntPtr printer)
    {
        byte[] centerAlignCommand = new byte[] { 0x1B, 0x61, 0x01 };
        //MessageBox.Show($"printer {printer}");
        var res = ESCPOS.WriteData(printer, centerAlignCommand, centerAlignCommand.Length);
        //MessageBox.Show($"res {res}");
    }

    public static void TextAlignLeft(IntPtr printer)
    {
        byte[] centerAlignCommand = new byte[] { 0x1B, 0x61, 0x00 };
        ESCPOS.WriteData(printer, centerAlignCommand, centerAlignCommand.Length);
    }

    public static void PrintBarcode(IntPtr printer, string barcode) {
        ESCPOS.PrintBarCode(printer,1,barcode,320,70,1,1);
    }

    public static void TextBold(IntPtr printer)
    {
        byte[] fBold = new byte[] { 0x1B, 0x45, 0x01 };
        ESCPOS.WriteData(printer, fBold, fBold.Length);
    }

    public static void SetTextSize(IntPtr printer, int size)
    {
        byte[] sizeCommand;
        switch (size)
        {
            case 1: // ขนาดปกติ //
                sizeCommand = new byte[] { 0x1B, 0x21, 0x00 };
                break;
            case 2: //bold //
                sizeCommand = new byte[] { 0x1B, 0x21, 0x08 };
                break;
            case 3: // boldMedium //
                sizeCommand = new byte[] { 0x1B, 0x21, 0x20 };
                break;
            case 4: // boldLarge //
                sizeCommand = new byte[] { 0x1B, 0x21, 0x10 };
                break;
            case 5: // ขนาดใหญ่ (2x ในแนวนอน)
                sizeCommand = new byte[] { 0x1D, 0x21, 0x01 };
                break;
            case 6: // ขนาดใหญ่ (2x ในแนวนอนและแนวตั้ง)
                sizeCommand = new byte[] { 0x1D, 0x21, 0x11 };
                break;
            case 7: // ขนาดใหญ่ขึ้น (3x ในแนวนอนและแนวตั้ง)
                sizeCommand = new byte[] { 0x1D, 0x21, 0x22 }; // ขนาดใหญ่ขึ้น
                break;
            case 8: // ขนาดใหญ่ขึ้น (4x ในแนวนอนและแนวตั้ง)
                sizeCommand = new byte[] { 0x1D, 0x21, 0x33 };
                break;
            case 9: // ขนาดใหญ่ขึ้น (7x ในแนวนอน)
                sizeCommand = new byte[] { 0x1D, 0x21, 0x63 };
                break;
            case 10: // ขนาดขยายกว้าง x2 (เฉพาะแนวนอน)
                sizeCommand = new byte[] { 0x1D, 0x21, 0x02 }; // ขยายขนาด x2 ในแนวนอน
                break;
            default: // ขนาดปกติ
                sizeCommand = new byte[] { 0x1B, 0x21, 0x00 };
                break;
        }

        //normalSize = { 0x1B, 0x21, 0x03}

        //bold = { 0x1B, 0x21, 0x08}

        //boldMedium = { 0x1B, 0x21, 0x20}

        //boldLarge = { 0x1B, 0x21, 0x10}

        ESCPOS.WriteData(printer, sizeCommand, sizeCommand.Length);
    }

    public static void LineSpace(IntPtr printer, int? space)
    {
        if (space == null) return;
        byte[] lineSpacingCommand = new byte[] { 0x1B, 0x33, (byte)space };
        ESCPOS.WriteData(printer, lineSpacingCommand, lineSpacingCommand.Length);
    }

    public static void LineSpaceDefault(IntPtr printer)
    {
        byte[] resetLineSpacingCommand = new byte[] { 0x1B, 0x32 };
        ESCPOS.WriteData(printer, resetLineSpacingCommand, resetLineSpacingCommand.Length);
    }

    public static void DrawLine(IntPtr printer, bool isBold = false)
    {
        if (isBold)
        {
            SetTextSize(printer, 3);
        }
        else {
            SetTextSize(printer, 1);
        }
        
        string line = new string('-', 42);
        byte[] textBytes = Encoding.GetEncoding("TIS-620").GetBytes(line);
        ESCPOS.WriteData(printer, textBytes, textBytes.Length);
        NewLine(printer);
    }

    public static void SelectFontA(IntPtr printer) {
        byte[] fontA = new byte[] { 0x1B, 0x21, 0x00 };
        ESCPOS.WriteData(printer, fontA, fontA.Length);
    }

    public static string FormatTextNormal(string text, int maxCharsPerLine = 42)
    {
        StringBuilder formattedText = new StringBuilder();
        int currentIndex = 0;

        while (currentIndex < text.Length)
        {
            int length = Math.Min(maxCharsPerLine, text.Length - currentIndex);
            string line = text.Substring(currentIndex, length);

            formattedText.Append(line);

            if (currentIndex + length < text.Length)
            {
                formattedText.Append("\n\r");
            }

            currentIndex += length;
        }
        return formattedText.ToString();
    }

    //test
    public static void TextPrintText(IntPtr printer) {
        
        int s = ESCPOS.PrintTextS(printer, "mtest ทกสอบ \n\rทกสอบ\n\r");
        //MessageBox.Show($"sss {s}");
    }
}
