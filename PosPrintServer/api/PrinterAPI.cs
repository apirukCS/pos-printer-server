using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;

public class PrinterAPI
{
    public static async Task<List<PrinterModel>?> GetPrinters(string url, string token)
    {
        try
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();
                //return ParsePrinterData(jsonResponse);
                return JsonConvert.DeserializeObject<List<PrinterModel>>(jsonResponse);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "HTTP Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return null;
        }
    }

}

