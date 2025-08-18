using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using Avalonia.Controls;

namespace PayApp;

public partial class MainWindow : Window, INotifyPropertyChanged
{
	public ObservableCollection<EmployeeViewModel> Employees { get; } = new();
	public ObservableCollection<MonthlyTotalViewModel> MonthlyTotals { get; } = new();

	private string _sourcePath = Path.Combine(AppContext.BaseDirectory, "Data1.xml");
	public string SourcePath
	{
		get => _sourcePath;
		set
		{
			if (_sourcePath != value)
			{
				_sourcePath = value;
				OnPropertyChanged();
			}
		}
	}

	public MainWindow()
	{
		InitializeComponent();
		DataContext = this;
	}

	public event PropertyChangedEventHandler? PropertyChanged;
	private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	private async void OnBrowseSourceClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		var ofd = new OpenFileDialog
		{
			AllowMultiple = false,
			Title = "Select source XML (Data1.xml or Data2.xml)",
			Filters = new List<FileDialogFilter>
			{
				new FileDialogFilter { Name = "XML", Extensions = { "xml" } }
			}
		};
		var res = await ofd.ShowAsync(this);
		if (res != null && res.Length == 1)
		{
			SourcePath = res[0];
		}
	}

	private void OnRunPipelineClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		RunPipeline();
	}

	private void RunPipeline()
	{
		Employees.Clear();
		MonthlyTotals.Clear();

		string xsltPath = Path.Combine(AppContext.BaseDirectory, "TransformToEmployees.xslt");
		string employeesOutPath = Path.Combine(AppContext.BaseDirectory, "Employees.xml");

		// 1) Run XSLT transformation (handles both Data1.xml and Data2.xml)
		var transform = new XslCompiledTransform();
		using (var reader = XmlReader.Create(xsltPath))
		{
			transform.Load(reader);
		}
		using (var inputReader = XmlReader.Create(SourcePath))
		using (var writer = XmlWriter.Create(employeesOutPath, new XmlWriterSettings { Indent = true, Encoding = System.Text.Encoding.UTF8 }))
		{
			transform.Transform(inputReader, writer);
		}

		// 2) After transform, compute total per Employee and add attribute totalSalary
		var employeesDoc = XDocument.Load(employeesOutPath);
		foreach (var employee in employeesDoc.Root!.Elements("Employee"))
		{
			double sum = 0.0;
			foreach (var sal in employee.Elements("salary"))
			{
				var amountStr = (string?)sal.Attribute("amount") ?? "0";
				sum += ParseAmount(amountStr);
			}
			employee.SetAttributeValue("totalSalary", sum.ToString("0.00", CultureInfo.InvariantCulture));
		}
		employeesDoc.Save(employeesOutPath);

		// 3) In the source file Data1.xml, add attribute to <Pay> that is sum of all amount
		// Only if the source is Data1-like (flat <item> children directly under <Pay>)
		var sourceDoc = XDocument.Load(SourcePath);
		var payElement = sourceDoc.Root;
		if (payElement != null)
		{
			var flatItems = payElement.Elements("item").ToList();
			if (flatItems.Any())
			{
				double totalAll = 0.0;
				foreach (var it in flatItems)
				{
					var amountStr = (string?)it.Attribute("amount") ?? "0";
					totalAll += ParseAmount(amountStr);
				}
				payElement.SetAttributeValue("totalAmount", totalAll.ToString("0.00", CultureInfo.InvariantCulture));
				sourceDoc.Save(SourcePath);
			}
		}

		// 4) Bind to UI: list all Employee and totals by month
		var inMemoryEmployees = employeesDoc.Root!.Elements("Employee").Select(emp => new EmployeeViewModel
		{
			Name = (string?)emp.Attribute("name") ?? string.Empty,
			Surname = (string?)emp.Attribute("surname") ?? string.Empty,
			TotalSalary = (string?)emp.Attribute("totalSalary") ?? "0"
		}).ToList();
		foreach (var vm in inMemoryEmployees)
		{
			Employees.Add(vm);
		}

		var monthTotals = employeesDoc.Root!.Elements("Employee")
			.SelectMany(emp => emp.Elements("salary"))
			.GroupBy(s => (string?)s.Attribute("mount") ?? string.Empty)
			.Select(g => new MonthlyTotalViewModel
			{
				Month = g.Key,
				Total = g.Sum(s => ParseAmount((string?)s.Attribute("amount") ?? "0")).ToString("0.00", CultureInfo.InvariantCulture)
			})
			.OrderBy(m => m.Month)
			.ToList();
		foreach (var m in monthTotals)
		{
			MonthlyTotals.Add(m);
		}
    }

	private static double ParseAmount(string value)
	{
		// Handle both dot and comma decimal separators
		value = value.Trim();
		if (value.Contains(',') && value.Contains('.'))
		{
			// If both are present, assume comma is decimal if dot is thousands separator, remove dots
			var noThousands = value.Replace(".", string.Empty);
			return double.Parse(noThousands.Replace(',', '.'), CultureInfo.InvariantCulture);
		}
		if (value.Contains(','))
		{
			return double.Parse(value.Replace(',', '.'), CultureInfo.InvariantCulture);
		}
		return double.Parse(value, CultureInfo.InvariantCulture);
	}
}

public class EmployeeViewModel
{
	public string Name { get; set; } = string.Empty;
	public string Surname { get; set; } = string.Empty;
	public string TotalSalary { get; set; } = string.Empty;
}

public class MonthlyTotalViewModel
{
	public string Month { get; set; } = string.Empty;
	public string Total { get; set; } = string.Empty;
}
