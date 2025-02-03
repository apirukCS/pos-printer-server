using PM = PrinterManager;

public class CashDrawer {
    public static void Kick(string ip) {
        IntPtr printer = ESCPOS.InitPrinter("");
        int s = ESCPOS.OpenPort(printer, $"NET,{ip}");
        PM.OpenCashDrawer(printer);
        PM.ClosePort(printer);
    }
}