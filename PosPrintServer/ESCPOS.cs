using System;
using System.Runtime.InteropServices;

public class ESCPOS
{
    [DllImport("printer.sdk.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
    public static extern IntPtr InitPrinter(string model);

    [DllImport("printer.sdk.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
    public static extern int ReleasePrinter(IntPtr intPtr);

    [DllImport("printer.sdk.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
    public static extern int OpenPort(IntPtr intPtr, string port);
    [DllImport("printer.sdk.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
    public static extern int ClosePort(IntPtr intPtr);
    [DllImport("printer.sdk.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
    public static extern int WriteData(IntPtr intPtr, byte[] buffer, int size);

    [DllImport("printer.sdk.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
    public static extern int ReadData(IntPtr intPtr, byte[] buffer, int size);
    [DllImport("printer.sdk.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
    public static extern int PrinterInitialize(IntPtr intPtr);
    [DllImport("printer.sdk.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern int SetTextLineSpace(IntPtr intPtr, int lineSpace);
    [DllImport("printer.sdk.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern int CancelPrintDataInPageMode(IntPtr intPtr);
    [DllImport("printer.sdk.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern int GetPrinterState(IntPtr intPtr, ref int printerStatus);

    [DllImport("printer.sdk.dll", CallingConvention = CallingConvention.StdCall)]
    public static extern int SetCodePage(IntPtr intPtr, int characterSet);

    [DllImport("printer.sdk.dll", CallingConvention = CallingConvention.StdCall)]
    public static extern int SetTextFont(IntPtr intPtr, int font);

    [DllImport("printer.sdk.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern int SetInternationalCharacter(IntPtr intPtr, int characterSet);
    [DllImport("printer.sdk.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern int CutPaper(IntPtr intPtr, int cutMode);
    [DllImport("printer.sdk.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern int CutPaperWithDistance(IntPtr intPtr, int distance);
    [DllImport("printer.sdk.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern int FeedLine(IntPtr intPtr, int lines);
    [DllImport("printer.sdk.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern int OpenCashDrawer(IntPtr intPtr, int pinMode, int onTime, int ofTime);
    [DllImport("printer.sdk.dll", CallingConvention = CallingConvention.StdCall)]
    public static extern int PrintText(IntPtr intPtr, string data, int alignment, int textSize);
    [DllImport("printer.sdk.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern int PrintTextS(IntPtr intPtr, string data);
    [DllImport("printer.sdk.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern int PrintBarCode(IntPtr intPtr, int bcType, string bcData, int width, int height, int alignment, int hriPosition);
    [DllImport("printer.sdk.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern int PrintSymbol(IntPtr intPtr, int type, string data, int errLevel, int width, int height, int alignment);
    [DllImport("printer.sdk.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern int PrintImage(IntPtr intPtr, string filePath, int scaleMode);

    [DllImport("printer.sdk.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern int SelectPageMode(IntPtr hPrinter);
}
