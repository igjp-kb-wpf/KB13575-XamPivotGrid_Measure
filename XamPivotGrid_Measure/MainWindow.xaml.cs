using Infragistics.Controls.Grids;
using Infragistics.Olap;
using Infragistics.Olap.Data;
using Infragistics.Olap.FlatData;
using Infragistics.Olap.Xmla;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace XamPivotGrid_Measure
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            FlatDataSource flatDataSource = new SampleFlatDataSource();
            this.dataSelector.DataSource = flatDataSource;
            this.pivotGrid.DataSource = flatDataSource;


            Dispatcher.BeginInvoke(DispatcherPriority.Background,
                new Action(
                    () =>
                    {
                        var filterViewModelsColumns = this.pivotGrid.DataSource.Columns.Where(fvm => fvm is IFilterViewModel).ToList();
                        var filterViewModelsRows = this.pivotGrid.DataSource.Rows.Where(fvm => fvm is IFilterViewModel).ToList();
                        FlatDataSource dataSource = this.pivotGrid.DataSource as FlatDataSource;
                        for (int i = 0; i < filterViewModelsColumns.Count; i++)
                        {
                            var filterViewModel = filterViewModelsColumns[i] as IFilterViewModel;
                            dataSource.ExpandAllLevelsAsync(filterViewModel);
                        }
                        for (int i = 0; i < filterViewModelsRows.Count; i++)
                        {
                            var filterViewModel = filterViewModelsRows[i] as IFilterViewModel;
                            dataSource.ExpandAllLevelsAsync(filterViewModel);
                        }
                        dataSource.RefreshGrid();
                    }
             ));
        }
    }

    public class SampleFlatDataSource : FlatDataSource
    {
        public SampleFlatDataSource()
        {
            this.ItemsSource = SampleDataGenerator.GenerateSales(100000);
            this.Cube = DataSourceBase.GenerateInitialCube("Sale");
            this.Rows = DataSourceBase.GenerateInitialItems("[Seller].[Seller]");
            //this.Columns = DataSourceBase.GenerateInitialItems("[Product].[Product]");
            this.Measures = DataSourceBase.GenerateInitialItems("AmountOfSale");
            this.DimensionsGenerationMode = Infragistics.Olap.FlatData.DimensionsGenerationMode.Mixed;
            this.PreserveMembersOrder = false;

            CubeMetadata cubeMetadata = new CubeMetadata();
            cubeMetadata.DisplayName = "Sales Data";
            cubeMetadata.DataTypeFullName = typeof(Sale).FullName;
            cubeMetadata.DimensionSettings.Add(new DimensionMetadata()
            {
                SourcePropertyName = "AmountOfSale",
                DisplayName = "Amount of sale",
                DisplayFormat = "{0:C2}"
            });
            cubeMetadata.DimensionSettings.Add(new DimensionMetadata()
            {
                SourcePropertyName = "NumberOfUnits",
                DisplayName = "Units sold"
            });
            cubeMetadata.DimensionSettings.Add(new DimensionMetadata()
            {
                SourcePropertyName = "UnitPrice",
                DisplayName = "Unit Price",
                DisplayFormat = "{0:C2}"
            });
            cubeMetadata.DimensionSettings.Add(new DimensionMetadata
            {
                SourcePropertyName = "UnitPrice",
                DimensionType = DimensionType.Measure,
                DisplayName = "Final Price",
            });

            cubeMetadata.DimensionSettings.Add(new DimensionMetadata()
            {
                SourcePropertyName = "Product",
                DisplayName = "Product"
            });
            cubeMetadata.DimensionSettings.Add(new DimensionMetadata()
            {
                SourcePropertyName = "City",
                DisplayName = "City"
            });
            cubeMetadata.DimensionSettings.Add(new DimensionMetadata()
            {
                SourcePropertyName = "Seller",
                DisplayName = "Seller",
            });
            cubeMetadata.DimensionSettings.Add(new DimensionMetadata()
            {
                SourcePropertyName = "Date",
                DisplayName = "Date"
            });

            this.CubesSettings.Add(cubeMetadata);

            HierarchyDescriptor<Sale> sellerHierarchy = new HierarchyDescriptor<Sale>(p => p.Seller);
            sellerHierarchy.AddLevel(p =>
                "All Sellers",
                "All Sellers"
                );
            sellerHierarchy.AddLevel(p =>
                p.Seller.Name,
               "Seller name"
                );
            this.AddHierarchyDescriptor(sellerHierarchy);

            HierarchyDescriptor<Sale> productHierarchy = new HierarchyDescriptor<Sale>(p => p.Product);
            productHierarchy.AddLevel(p =>
                "All products",
                "All products"
                );
            productHierarchy.AddLevel(p =>
                p.Product.Name,
                "Product name"
                );
            this.AddHierarchyDescriptor(productHierarchy);

            HierarchyDescriptor<Sale> locationHierarchy = new HierarchyDescriptor<Sale>(p => p.City);
            locationHierarchy.AddLevel(p =>
                "All locations",
                "All locations"
                );
            locationHierarchy.AddLevel(p =>
                p.City,
                "City"
                );
            this.AddHierarchyDescriptor(locationHierarchy);

            HierarchyDescriptor dateTimeDescriptor = new HierarchyDescriptor { AppliesToPropertiesOfType = typeof(DateTime) };
            dateTimeDescriptor.AddLevel<DateTime>(dt =>
                "AllPeriods",
                "All Periods"
            );
            dateTimeDescriptor.AddLevel<DateTime>(dt =>
                dt.Year,
                "Years"
            );
            dateTimeDescriptor.AddLevel<DateTime>(dt =>
                dt.QuarterShort(),
                "Quarters"
            );
            dateTimeDescriptor.AddLevel<DateTime>(dt =>
                dt.MonthShort(),
                "Months"
            );
            dateTimeDescriptor.AddLevel<DateTime>(dt =>
                dt.Date.ToString("yyyy/MM/dd"),
                "Date"
            );
            this.HierarchyDescriptors.Add(dateTimeDescriptor);
        }
    }

    public class SampleDataGenerator
    {
        private static String[] Products;
        private static String[] SellerNames;
        private static String[] Cities;

        private static Random random = new Random();

        public static ObservableCollection<Sale> GenerateSales(int numberOfSales)
        {
            SampleDataGenerator.Products = "Accessories;Bikes;Clothing;Components".Split(';');
            SampleDataGenerator.SellerNames = "John Smith;Alfredo Fetuchini;Howard Sprouse;Russell Shorter;Nicholas Carmona;Larry Lieb;Carl Costello;Benjamin Dupree;Bryan Culver;Monica Freitag;Lydia Burson;Harold Garvin;Walter Pang;David Haley;Glenn Landeros;Harry Tyler;Stanley Brooker;Antonio Charbonneau;Brandon Mckim;Claudia Kobayashi;Benjamin Meekins;Mark Slater;Kathe Pettel;Elisa Longbottom".Split(';');
            SampleDataGenerator.Cities = "Sofia;London;New York;Berlin;Seattle;Mellvile;Tokyo".Split(';');

            ObservableCollection<Sale> Sales = new ObservableCollection<Sale>();
            for (double i = 0; i < numberOfSales; i++)
            {
                Sale sale = new Sale();
                sale.Date = GetRandomDate();
                sale.Product = GerRandomProduct();
                sale.NumberOfUnits = GetRandomNumUnits();
                sale.Seller = GetRandomSeller();
                Sales.Add(sale);
            }
            return Sales;
        }

        private static Seller GetRandomSeller()
        {
            return new Seller()
            {
                City = GetRandomCity(),
                Name = GetRandomSellerName()
            };
        }

        private static string GetRandomSellerName()
        {
            Random a = new Random(SampleDataGenerator.random.Next());
            return SampleDataGenerator.SellerNames[a.Next(SampleDataGenerator.SellerNames.Length)];
        }

        private static string GetRandomCity()
        {
            Random a = new Random(SampleDataGenerator.random.Next());
            return SampleDataGenerator.Cities[a.Next(SampleDataGenerator.Cities.Length)];
        }

        private static int GetRandomNumUnits()
        {
            Random a = new Random(SampleDataGenerator.random.Next());
            return a.Next(1, 500);
        }

        private static Product GerRandomProduct()
        {
            return new Product()
            {
                Name = GetRandomProductName(),
                Price = GetRandomPrice()
            };
        }

        private static double GetRandomPrice()
        {
            Random a = new Random(SampleDataGenerator.random.Next());
            return a.NextDouble() * 100;
        }

        private static string GetRandomProductName()
        {
            Random a = new Random(SampleDataGenerator.random.Next());
            return SampleDataGenerator.Products[a.Next(SampleDataGenerator.Products.Length)];
        }

        private static DateTime GetRandomDate()
        {
            Random a = new Random(SampleDataGenerator.random.Next());
            int day = a.Next(1, 28);
            int month = a.Next(1, 12);
            int year = a.Next(DateTime.Today.Year, DateTime.Today.Year);
            return new DateTime(year, month, day);
        }
    }

    public class Sale
    {
        public Product Product { get; set; }
        public Seller Seller { get; set; }
        public DateTime Date { get; set; }
        public string City
        {
            get { return Seller.City; }
            set { Seller.City = value; }
        }

        public int NumberOfUnits { get; set; }
        public double AmountOfSale
        {
            get { return UnitPrice * NumberOfUnits; }
            set { Product.Price = value / NumberOfUnits; }
        }
        public double UnitPrice
        {
            get { return Product.Price; }
            set { Product.Price = value; }
        }
    }

    public class Product
    {
        public string Name { get; set; }
        public double Price { get; set; }
    }

    public class Seller
    {
        public string Name { get; set; }
        public string City { get; set; }
    }

    public class MyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            PivotHeaderCell headerCell = value as PivotHeaderCell;
            if (headerCell != null)
            {
                FlatDataSource data = (headerCell.Grid as XamPivotGrid).DataSource as FlatDataSource;
                if (data.Measures.Count == 1 && headerCell.Member.Caption == "")
                {
                    return data.Measures[0].Caption;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
