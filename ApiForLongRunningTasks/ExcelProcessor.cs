using OfficeOpenXml;

namespace ApiForLongRunningTasks
{
    public class ExcelProcessor
    {
        private readonly IServiceProvider _serviceProvider;

        public ExcelProcessor(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// From the given file path, opens excel file and inserts customers to database.
        /// </summary>
        /// <param name="filePath"></param>
        public void ProcessExcelFile(string filePath)
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using var package = new ExcelPackage(stream);

            var worksheet = package.Workbook.Worksheets[0];
            var rowCount = worksheet.Dimension.Rows;

            for (var i = 2; i <= rowCount; i++)
            {
                if (TryGetCustomer(worksheet, i, out var customer)) continue; // Ignore Empty rows.

                SaveCustomerToDatabase(customer); // Ignoring bulk insertion to get the feeling of big excel file.
            }
        }

        private static bool TryGetCustomer(ExcelWorksheet worksheet, int i, out Customer customer)
        {
            customer = null;
            if (worksheet.Cells[i, 1].Value is null || worksheet.Cells[i, 2].Value is null) return true;

            customer = new Customer(
                worksheet.Cells[i, 1].Value.ToString().Trim(),
                worksheet.Cells[i, 2].Value.ToString().Trim());
            return false;
        }

        private void SaveCustomerToDatabase(Customer customer)
        {
            var context = _serviceProvider.GetRequiredService<StoreDbContext>();
            context.Customers.Add(customer);
            context.SaveChanges();

            Thread.Sleep(1000); // Slowing down the process to get the feeling of big excel file.
        }
    }
}