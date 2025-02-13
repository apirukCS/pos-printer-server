using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;
using PrintingModel;

using System.IO;
using System.Reflection;

public class PrinterManager
{
    private static ConcurrentDictionary<string, IntPtr> connectedPrinters = new ConcurrentDictionary<string, IntPtr>();

    public static IntPtr GetPrinterConnection(string ipAddress)
    {
        IntPtr printer = ESCPOS.InitPrinter("");
        int s = ESCPOS.OpenPort(printer, $"NET,{ipAddress}");
        return printer;
    }

    public static int PrintSymbol(IntPtr printer, string? data)
    {
        int s = ESCPOS.PrintSymbol(printer, 49, data ?? "", 48, 10, 10, 1);
        NewLine(printer);
        return s;
    }

    public static void Reset(IntPtr printer) {
        ESCPOS.WriteData(printer, new byte[] { 0x1B, 0x40 }, 2);
    }

    public static IntPtr GetPrinterConnectionOnly(string ipAddress) {
        IntPtr printer = ESCPOS.InitPrinter("");
        int s = ESCPOS.OpenPort(printer, $"NET,{ipAddress}");
        return printer;
    }

    public static int ClosePort(IntPtr printer) {
        int s = ESCPOS.ClosePort(printer);
        return s;
    }

    public static void ReleasePort(IntPtr printer) { 
        ESCPOS.ReleasePrinter(printer);
    }

    public static void PrinterInitialize(IntPtr printer) {
        ESCPOS.PrinterInitialize(printer);
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

    //-------------------------------ESC Command------------------------------------------
    public static int PrintText(IntPtr printer, string text, bool isFormatText = true, int? lineSpace = null)
    {
        text = ThaiText(text);
        SetTextSize(printer, 1);
        byte[] textBytes = Encoding.GetEncoding("TIS-620").GetBytes(text);
        int s = ESCPOS.WriteData(printer, textBytes, textBytes.Length);
        NewLine(printer, lineSpace);
        return s;
    }

    public static int PrintTextOnly(IntPtr printer, string text)
    {
        text = ThaiText(text);
        SetTextSize(printer, 1);
        byte[] textBytes = Encoding.GetEncoding("TIS-620").GetBytes(text);
        int s = ESCPOS.WriteData(printer, textBytes, textBytes.Length);
        return s;
    }

    public static int PrintTextBold(IntPtr printer, string text,bool? isNewLine = true)
    {
        text = ThaiText(text);
        SetTextSize(printer, 2);
        byte[] textBytes = Encoding.GetEncoding("TIS-620").GetBytes(text);
        int s = ESCPOS.WriteData(printer, textBytes, textBytes.Length);
        if (isNewLine == false) return s;
        NewLine(printer);
        return s;
    }

    public static int PrintTextTitleAndSubTitle(IntPtr printer, string title, string subTitle) {
        title = ThaiText(title);
        subTitle = ThaiText(subTitle);
        SetTextSize(printer, 1);
        byte[] textBytes = Encoding.GetEncoding("TIS-620").GetBytes(title);
        int s = ESCPOS.WriteData(printer, textBytes, textBytes.Length);
        SetTextSize(printer, 2);
        textBytes = Encoding.GetEncoding("TIS-620").GetBytes(subTitle);
        int s2 = ESCPOS.WriteData(printer, textBytes, textBytes.Length);
        NewLine(printer);
        return s + s2;
    }

    public static int PrintTextMediumBold(IntPtr printer, string text,bool isFormatText = true)
    {
        text = ThaiText(text);
        SetTextSize(printer, 3);
        if (isFormatText)
        {
            text = FormatTextNormal(text, 30);
        }
        byte[] textBytes = Encoding.GetEncoding("TIS-620").GetBytes(text);
        int s = ESCPOS.WriteData(printer, textBytes, textBytes.Length);
        NewLine(printer);
        return s;
    }

    public static int PrintTextLargeBold(IntPtr printer, string text)
    {
        text = ThaiText(text); ;
        SetTextSize(printer, 4);
        byte[] textBytes = Encoding.GetEncoding("TIS-620").GetBytes(text);
        int s = ESCPOS.WriteData(printer, textBytes, textBytes.Length);
        NewLine(printer);
        return s;
    }

    public static int PrintTextTwoColumn(IntPtr printer, string textLeft, string textRight, int size = 1)
    {
        textLeft = ThaiText(textLeft);
        textRight = ThaiText(textRight);
        SetTextSize(printer, size);
        int maxLineLength = 42;
        StringBuilder output = new StringBuilder();
        int countSpecialCharRight = SpecialCharacterCount(textRight);
        //textLeft =  textLeft + new string(' ', countSpecialCharRight);
        string textLeftFiltered = Regex.Replace(textLeft, "[\u0E31\u0E34-\u0E3A\u0E47-\u0E4D]", "");
        int effectiveLeftLength = textLeftFiltered.Length;
        int availableSpaceForLeft = maxLineLength - textRight.Length - 1 + countSpecialCharRight;

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
            int spaceBetween = maxLineLength - (effectiveLeftLength + textRight.Length) + countSpecialCharRight;
            string line = textLeft + new string(' ', spaceBetween) + textRight;
            output.Append(line + "\r\n");
        }
        byte[] textBytes = Encoding.GetEncoding("TIS-620").GetBytes(output.ToString());
        int s = ESCPOS.WriteData(printer, textBytes, textBytes.Length);
        return s;
    }

    //ToDo 
    public static int PrintTextThreeColumn(IntPtr printer, string textLeft, string textMiddle, string textRight, int maxLineLength = 42, int leftColumnWidth = 7)
    {
        textLeft = ThaiText(textLeft);
        textMiddle = ThaiText(textMiddle);
        textRight = ThaiText(textRight);
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
        byte[] textBytes = Encoding.GetEncoding("TIS-620").GetBytes(output.ToString());
        int s = ESCPOS.WriteData(printer, textBytes, textBytes.Length);
        return s;
    }

    public static int OpenCashDrawer(IntPtr printer) {
        int s = ESCPOS.OpenCashDrawer(printer,0,30, 255);
        return s;
    }

    private static int SpecialCharacterCount(string text)
    {
        char[] topDiacritics = { 'ิ', 'ี', 'ึ', 'ื', '่', '้', '๊', '๋', 'ั', '์', 'ํ', '็' };
        char[] bottomDiacritics = { 'ุ', 'ู' };

        int specialCharacterCount = 0;

        foreach (char c in text)
        {
            if (topDiacritics.Contains(c))
            {
                specialCharacterCount++;
            }
            else if (bottomDiacritics.Contains(c))
            {
                specialCharacterCount++;
            }
        }

        return specialCharacterCount;
    }

    public static int SetTextFont(IntPtr printer, int fontType) { 
        int s = ESCPOS.SetTextFont(printer, fontType);
        return s;
    }

    public static int PrintQueueNumber(IntPtr printer, int number)
    {
        string basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if (string.IsNullOrEmpty(basePath))
        {
            basePath = Directory.GetCurrentDirectory();
        }
        string folderPath = Path.Combine(basePath, "images", "numbers");

        try
        {
            string path;
            if (number < 10)
            {
                path = Path.Combine(folderPath, $"{number}.jpeg");
            }
            else if (number < 100)
            {
                string pathTens = Path.Combine(folderPath, $"{number / 10}.jpeg");
                string pathOnes = Path.Combine(folderPath, $"{number % 10}.jpeg");
                string combinedPath = Path.Combine(folderPath, "combined.jpeg");
                ImageProcessor.MergeTwoImages(pathTens, pathOnes, combinedPath);
                path = combinedPath;
            }
            else
            {
                string pathHundreds = Path.Combine(folderPath, $"{number / 100}.jpeg");
                string pathTens = Path.Combine(folderPath, $"{(number % 100) / 10}.jpeg");
                string pathOnes = Path.Combine(folderPath, $"{number % 10}.jpeg");
                string combinedPath = Path.Combine(folderPath, "combined.jpeg");
                ImageProcessor.MergeThreeImages(pathHundreds, pathTens, pathOnes, combinedPath);
                path = combinedPath;
            }
            int s = ESCPOS.PrintImage(printer, path, 0);
            return s;
        }
        catch (Exception e) {
            //MessageBox.Show($"ee {e}");
            WriteLog.Write($"ee {e}");
            return -1;
        }
    }

    public static int PrintImage(IntPtr printer, string path)
    {
        int s = ESCPOS.PrintImage(printer, path, 0);
        return s;
    }

    public static async Task<int> PrintImageUrl(IntPtr printer, string url, string path, uint width = 200)
    {
        if (string.IsNullOrEmpty(url)) return 0;
        await ImageProcessor.ProcessImageFromUrlAsync(url, path, width);
        int s = ESCPOS.PrintImage(printer, path, 0);
        return s;
    }

    public static void NewLine(IntPtr printer,int? lineSpace = null)
    {
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
        var res = ESCPOS.WriteData(printer, centerAlignCommand, centerAlignCommand.Length);
    }

    public static void TextAlignLeft(IntPtr printer)
    {
        byte[] centerAlignCommand = new byte[] { 0x1B, 0x61, 0x00 };
        ESCPOS.WriteData(printer, centerAlignCommand, centerAlignCommand.Length);
    }

    public static void PrintBarcode(IntPtr printer, string barcode) {
        var centerAlign = new byte[] { 0x1B, 0x61, 0x01 };
        ESCPOS.WriteData(printer, centerAlign, centerAlign.Length);

        // ตั้งความสูงบาร์โค้ด
        var heightCommand = new byte[] { 0x1D, 0x68, 0x48 };
        ESCPOS.WriteData(printer, heightCommand, heightCommand.Length);

        // ตั้งความกว้างบาร์โค้ด
        var widthCommand = new byte[] { 0x1D, 0x77, 0x03 };
        ESCPOS.WriteData(printer, widthCommand, widthCommand.Length);

        // ไม่แสดงข้อความ HRI (0 = ไม่แสดง)
        var hriCommand = new byte[] { 0x1D, 0x48, 0x00 };
        ESCPOS.WriteData(printer, hriCommand, hriCommand.Length);

        // พิมพ์บาร์โค้ด
        string barcodeData = barcode;
        var barcodeCommand = new byte[]
        {
            0x1D, 0x6B,       // GS k
            0x49,             // m=73 (Code 128)
            (byte)barcodeData.Length  // ความยาวข้อมูล
        }.Concat(Encoding.ASCII.GetBytes(barcodeData)).ToArray();

        ESCPOS.WriteData(printer, barcodeCommand, barcodeCommand.Length);
        TextAlignLeft(printer);
    }

    public static int TextBold(IntPtr printer)
    {
        byte[] fBold = new byte[] { 0x1B, 0x45, 0x01 };
        int s = ESCPOS.WriteData(printer, fBold, fBold.Length);
        return s;
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


    public static string ThaiText(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;

        StringBuilder result = new StringBuilder();

        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == 'ำ')
            {
                bool hasTonetMark = (i > 0 && IsThaiToneMark(text[i - 1]));

                if (!hasTonetMark)
                {
                    result.Append("ํา");
                }
                else
                {
                    result.Append("ำ");
                }
            }
            else
            {
                result.Append(text[i]);
            }
        }

        return result.ToString();
    }

    private static bool IsThaiToneMark(char c)
    {
        // ่  ้  ๊  ๋
        return c == '\u0E48' || c == '\u0E49' || c == '\u0E4A' || c == '\u0E4B';
    }

    private static bool IsThaiUpperVowel(char c)
    {
        return c == '่' || // เอก
               c == '้' || // โท
               c == '๊' || // ตรี
               c == '๋' || // จัตวา
               c == '็' || // ไม้ไต่คู้
               c == '์' || // การันต์
               c == 'ิ' || // สระอิ
               c == 'ี' || // สระอี
               c == 'ึ' || // สระอึ
               c == 'ื' || // สระอื
               c == 'ั' || // ไม้หันอากาศ
               c == 'ํ';    // นิคหิต
    }
}
