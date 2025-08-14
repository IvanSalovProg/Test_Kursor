# Pay Processing Assignment

This workspace contains:

- `Data1.xml`, `Data2.xml`: example source data
- `TransformToEmployees.xslt`: XSLT that converts either source format to `Employees.xml`
- `Employees.xml`: sample output
- `PayApp/`: Avalonia .NET 8 desktop GUI app that runs the pipeline and displays results

## XSLT usage

You can run the XSLT with .NET's `xsltc` or any XSLT 1.0 processor. Example with `xmllint` + `xsltproc` (if available):

```bash
xsltproc TransformToEmployees.xslt Data1.xml > Employees.xml
```

or for `Data2.xml`:

```bash
xsltproc TransformToEmployees.xslt Data2.xml > Employees.xml
```

## GUI app

The GUI app requires .NET SDK 8.0+.

- Install .NET SDK: see `https://dotnet.microsoft.com/en-us/download`
- Build and run:

```bash
cd PayApp
 dotnet restore
 dotnet run
```

In the app:
- Select `Data1.xml` or `Data2.xml`
- Click "Run Pipeline"
- The app will:
  1. Run the XSLT to produce `Employees.xml`
  2. Append `totalSalary` attribute per `Employee` in `Employees.xml`
  3. Add `totalAmount` attribute to the `<Pay>` element in the source file (sum of all `amount`)
  4. Show the employees list and monthly totals

### Notes
- Amount parsing supports both comma and dot decimal separators (e.g., `3001,10` and `2000.0`).
- The XSLT uses Muenchian grouping to group `item` elements by `name|surname` across the document (works for both `Data1.xml` and `Data2.xml`).