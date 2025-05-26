# QSIT Type Optimizer for Revit

A lightweight Revit add-in to streamline your type-management workflow: batch-delete unused types, duplicate & rename families with a `QSIT_` prefix, push type changes to instances, assign or randomize instance comments, and quickly place elements from selected types—all from a single WinForms panel in Revit.

## 📦 Features

* **Bulk Delete**: Remove all unused family types in the active document by category.
* **Duplicate & Rename**: Duplicate selected types and prefix their names with `QSIT_`, skipping any that already exist.
* **Update Instances**: Swap every instance of an original type to its corresponding `QSIT_` version.
* **Assign Manual Comment**: Write a custom comment to the Comments parameter on all instances of the selected types.
* **Randomize Comment**: Generate semi-unique comments (e.g., timestamps, random strings) and push them to the Comments parameter.
* **Place Element**: Quickly pick a point in the model to place an instance of the selected family type.

## ⚙️ Requirements

* Autodesk Revit 2023 or 2024
* .NET Framework 4.8
* Visual Studio 2022 (or later) with Revit API SDK installed
* Windows 10 / 11

## 🚀 Installation

1.  **Clone or download this repo:**
    ```bash
    git clone [https://github.com/ahmed3bdelaziz/QSIT_TypeOptimizer.git](https://github.com/ahmed3bdelaziz/QSIT_TypeOptimizer.git)
    ```
2.  Open `QSIT_TypeOptimizer.sln` in Visual Studio.
3.  Build the solution in `Debug` or `Release` mode.
4.  Copy the following files into your Revit AddIns folder (e.g., `%AppData%\Autodesk\Revit\Addins\2023\`):
    * `QSIT_TypeOptimizer.dll`
    * `QSIT_TypeOptimizer.addin`
    * `Resources` folder (embedded icons are compiled, but keep PNGs alongside if you adjust icons)
5.  Launch Revit and look for the "QSIT Tools" panel on the Add-Ins ribbon.

## 🎬 Usage

1.  Open a project in Revit.
2.  Navigate to `Add-Ins` → `QSIT Tools` → `Type Optimizer`.
3.  In the `Type Optimizer` dialog:
    * **Category**: select the family category to target.
    * **Grid**: check the types you wish to process.
4.  Click any of the action buttons at the bottom:
    * **Delete**: remove unused.
    * **Duplicate/Rename**: create `QSIT_` copies.
    * **Update Instances**: swap to the `QSIT_` copies.
    * **Assign Comment**: type your comment, then click.
    * **Randomize Comment**: auto-generate and assign.
    * **Place**: click in your model to place a new instance of the selected type.

## 🛠 Development

### Project structure

* `App.cs` – ribbon and icon loading
* `QSITTypeOptimizerCommand.cs` – external command entry point
* `MainForm.*` – WinForms UI logic
* `*.EventHandler.cs` – handlers for document, operation, placement events

### Resource embedding

* Ensure `Resources/qsit_16.png` and `qsit_32.png` are marked `Build Action: Resource` in the `.csproj`.
* Confirm the folder name and casing match exactly in your `LoadPng("Resources/…")` calls.

### Debugging

* Use `Attach to Process` → `Revit.exe` from Visual Studio.
* Set breakpoints in your event handlers or button-click methods.

## ❓ Troubleshooting

* **Cannot locate resource ‘Resources/qsit_16.png’**
    * Verify the PNGs are included in the project with `Build Action: Resource`.
    * Confirm the folder name and casing match exactly in your `LoadPng("Resources/…")` calls.
* **Duplicate button shows twice**
    * Check your `.addin` file and `App.OnStartup` to ensure you only create one `PushButtonData` and one `<AddIn Type="Command">` entry.
* **API exceptions on `Transaction.Start()`**
    * Make sure you’re launching your WinForm from a valid `IExternalCommand` context, and all document modifications or complex reads are handled within an `IExternalEventHandler` triggered by `ExternalEvent.Raise()`.

---

Built by Ahmed Abdelaziz for streamlined Revit family-type management.

Feedback and contributions are welcome!
