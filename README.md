<p align="center">
  <img src="assets/logo.png" alt="WiXCraft logo" width="256" />
</p>

# WiXCraft

WiXCraft is a small toolkit for building **custom WPF embedded UI** installers with [WiX Toolset v5](https://wixtoolset.org/). It replaces the boilerplate normally required to implement `IEmbeddedUI`, wire up MSI message handling, progress tracking, maintenance-mode flows, and install cancellation.

The repository contains the WiXCraft libraries, MSBuild integration, and a working sample (`ExampleInterface` + `ExampleSetup`).

![Example embedded installer UI](assets/screen.png)

## What you get

- **MSBuild packages** that generate WiX include files and embed your UI assembly into the MSI
- **`WiXCraft.Core`** — embedded UI engine (`Initialize`, block until the user starts install, progress, cancel coordination)
- **`WiXCraft.CustomActions`** — deferred custom action that honors user cancellation from the embedded UI
- **`ExampleInterface`** — reference WPF installer (MVVM, MahApps.Metro, maintenance/repair/modify/remove, feature selection, live install action text, animated finish state)
- **`ExampleSetup`** — sample WiX v5 package that produces an MSI using the embedded UI

## Repository layout

```
src/
  WiXCraft.slnx              Solution entry point
  WiXCraft.Core/             Core embedded UI engine (NuGet: WiXCraft.Core)
  WiXCraft.Wpf/              WPF helpers (async sequence hooks, NuGet: WiXCraft.Wpf)
  WiXCraft.Installer/        WiX MSBuild integration (NuGet: WiXCraft.Installer)
  WiXCraft.Installer.Wpf/    WPF embedded UI entry point + props/targets
  WiXCraft.CustomActions/    Cancellation custom action DLL
  ExampleInterface/          Sample WPF embedded UI project
  ExampleSetup/              Sample WiX installer (.msi)
  Directory.Build.props      Shared versions + development-mode defaults
```

## Prerequisites

- Windows
- [.NET Framework 4.8 SDK](https://dotnet.microsoft.com/download/dotnet-framework)
- [.NET SDK](https://dotnet.microsoft.com/download) (for `dotnet build`)
- [WiX Toolset v5](https://wixtoolset.org/docs/intro/) (pulled in via `WixToolset.Sdk` for the sample)

Embedded UI runs as **x86** under `msiexec`, matching the platform WiXCraft targets.

## Build the sample MSI

From the repository root:

```powershell
dotnet build src\WiXCraft.slnx
```

The MSI is written to:

```
src\ExampleSetup\bin\x64\Debug\ExampleSetup.msi
```

Run it directly or install with:

```powershell
msiexec /i src\ExampleSetup\bin\x64\Debug\ExampleSetup.msi
```

To exercise maintenance mode, install once, then run the MSI again or use **Apps & features → Modify**.

## How it works

1. **`ExampleSetup`** sets `UseWiXCraftInstaller=true` and references `ExampleInterface`. Before build, WiXCraft generates `WiXCraft.generated.wxi` with embedded UI binary entries and cancellation custom action wiring.
2. **`ExampleInterface`** is a WPF class library (`net48`, `x86`) with:
   - `UseWiXCraftEmbeddedUi=true` / `UseWiXCraftEmbeddedUiWpf=true`
   - `EmbeddedUiHostFactory` pointing at your `IInstallerUiHostFactory` implementation
3. At install time, MSI loads the embedded UI DLL. **`EmbeddedUiEntryPoint`** (from `WiXCraft.Installer.Wpf`) delegates to your host factory.
4. Your **`IInstallerUiHost`** runs a WPF `Application`, shows your window, and blocks MSI until the user chooses an action (install, repair, modify, remove).
5. **`IInstallerUiContext`** exposes session properties, features, progress, MSI message handling, and **`ModeOptions`** so your UI knows which install operations and MSI UI levels are enabled.

## Installer UI modes

Implement **`CreateModeOptions()`** on your `IInstallerUiHostFactory` (or inherit **`InstallerUiHostFactoryBase`**) to declare how the embedded UI interacts with MSI:

| Option | Purpose |
|--------|---------|
| `RequiredInitializeLevel` | UI level the embedded UI requires when MSI calls `Initialize` (default: `Full`) |
| `PostInitializeLevel` | UI level returned to MSI after pre-install UI completes (default: `NoChange \| SourceResolutionOnly`) |
| `SupportedOperations` | Flags: fresh install, repair, modify, uninstall, upgrade |
| `RepairReinstallMode` | `REINSTALLMODE` value used for repair (default: `ecmus`) |
| `HandleEngineDialogs` | When `true`, interactive MSI messages (errors, warnings, file-in-use prompts, etc.) are shown via your dialog handler (default: `true`) |

Example — install and modify only:

```csharp
public override InstallerUiModeOptions CreateModeOptions()
{
  return new InstallerUiModeOptions
  {
    SupportedOperations = InstallerOperationModes.FreshInstall | InstallerOperationModes.Modify,
  };
}
```

The engine validates the selected operation before install starts. The sample UI hides buttons for unsupported operations.

### Engine dialog handling

When MSI sends interactive messages during install (`Error`, `Warning`, `User`, `FilesInUse`, etc.), assign an **`IInstallerMessageDialogHandler`** on **`IInstallerUiContext.MessageDialogHandler`** before the install starts. The handler runs on the UI thread and its return value is passed back to MSI.

The sample UI registers **`WpfInstallerMessageDialogHandler`**, which shows a MahApps **`CustomDialog`** overlay on the wizard window (Dark.Red theme + installer button styles). All MSI button sets (`Retry/Cancel`, `Abort/Retry/Ignore`, etc.) are supported. A native Win32 fallback is used only if the owner is not a **`MetroWindow`**.

```csharp
context.MessageDialogHandler = new WpfInstallerMessageDialogHandler(() => wizardWindow);
```

Set **`HandleEngineDialogs = false`** on **`InstallerUiModeOptions`** to skip UI prompts and return the MSI default button instead (useful for silent or automated scenarios).

Subscribe to **`InstallMessageReceived`** if you want to log or customize behavior before the default dialog handler runs.

**Visual demo:** `ExampleSetup` sets `WIXCRAFT_DEMO_ENGINE_DIALOG=1` by default. During install, a custom action posts a **Retry/Cancel** engine prompt near the end of the execute sequence so you can confirm the MahApps overlay.

### Lifecycle hooks

Implement **`CreateLifecycle()`** on your host factory (or inherit **`InstallerUiHostFactoryBase`**) to run code at key points in the embedded UI session:

| Hook | When it runs | Typical use |
|------|----------------|-------------|
| `OnInitializing` | After the host creates the UI, before the window is shown | Load saved config, seed session properties |
| `OnInstallStarting` | When the user starts install/repair/modify/remove, before MSI execute continues | Validate input, write `Session` properties; set `args.Cancel = true` to block |
| `OnInstallCompleted` | When MSI sends `InstallEnd` (or at shutdown if that message was not received) | Record outcome; use **`Session.TrySetProperty`** — the session handle is closed after shutdown |

The context also exposes matching events (**`Initializing`**, **`InstallStarting`**, **`InstallCompleted`**) so view models can subscribe without implementing the lifecycle interface.

Write session properties in **`OnInstallStarting`** when possible. For **`OnInstallCompleted`**, prefer **`TrySetProperty`** because MSI may already have closed the session if the hook runs from **`EnableExit`** during shutdown.

```csharp
public override IInstallerUiLifecycle CreateLifecycle()
{
  return new MyInstallerUiLifecycle();
}

// In the host, after the view model exists:
context.RaiseInitializing();

// Before installStartEvent.Set():
if (!context.RaiseInstallStarting(selectedOperation)) return;

// When install ends:
context.RaiseInstallCompleted(succeeded);
```

The sample **`ExampleInstallerUiLifecycle`** writes `WIXCRAFT_*` session properties; the diagnostics page logs each hook.

### Execute sequence observer

**`IInstallerUiContext.ExecuteSequence`** tracks MSI execute sequence traffic from `ProcessMessage` — no custom actions required for observation.

| Event / data | Source message | Use |
|--------------|----------------|-----|
| `ExecuteSequenceStarted` | `InstallStart` | Execute phase began |
| `ExecuteSequenceEnded` | `InstallEnd` | Execute phase finished |
| `ActionStarted` | `ActionStart` | Standard or custom action name + description |
| `ActionProgress` | `ActionData` | Sub-step detail for the current action |
| `ActionCompleted` | Next `ActionStart` / `InstallEnd` | Heuristic completion of the prior action |
| `WhenAction(name, handler)` | `ActionStart` filter | Convenience subscription |
| `EntryAdded` / `Entries` | All of the above | Timeline / logging |

```csharp
context.ExecuteSequence.ActionStarted += (_, args) =>
{
  // args.ActionName e.g. InstallFiles, PublishProduct, WiXCraft_CheckEmbeddedUICancellation
};

context.ExecuteSequence.EntryAdded += (_, entry) => timeline.Add(entry);
```

The sample **Diagnostics → Execute sequence** tab shows a live action timeline during install.

### Install property bag (no custom actions)

Use **`context.InstallProperties`** (`InstallerPropertyBag`) to write public MSI session properties before install starts. Names are normalized with the `WIXCRAFT_` prefix by default.

```csharp
context.InstallProperties.TrySet("DB_SERVER", serverName);
context.SetInstallProperties(new Dictionary<string, string>
{
  ["DB_NAME"] = databaseName,
  ["WEB_PORT"] = port,
});
```

The WiX author consumes these in **`Product.wxs`** with standard `Property`, `SetProperty`, component conditions, or registry/file tables — no author-written DLL custom actions required for simple config flow.

Use **`Session.TrySetProperty`** / **`InstallProperties.TrySet`** (not the indexer) when the session handle may be closed.

### Sequence hooks (interactive, WiXCraft CA)

Declare hook **placement** in your `.wixproj` (MSBuild generates wxs). Register hook **handlers** in **`ConfigureSequenceHooks`** on your host factory.

```xml
<ItemGroup>
  <WiXCraftSequenceHook Include="BeforeInstallFiles"
                        Before="InstallFiles"
                        Payload="Optional payload"
                        Buttons="OKCancel" />
</ItemGroup>
```

```csharp
public override void ConfigureSequenceHooks(IInstallerUiContext context)
{
  context.SequenceHooks.Register("BeforeInstallFiles", ctx =>
  {
    // Validate UI state, update labels, optionally ctx.Cancel = true
    return SequenceHookResult.Continue;
  });
}
```

**Async handlers** (MSI waits until the task completes; UI message loop keeps pumping):

In **`IInstallerUiHost.Run`**, configure the WPF async invoker once:

```csharp
context.ConfigureSequenceHookAsyncInvoker(wizardView.Dispatcher);
```

Then register async hooks in **`ConfigureSequenceHooks`**:

```csharp
context.SequenceHooks.RegisterAsync("BeforeInstallFiles", async ctx =>
{
  await PingDatabaseAsync(ctx.Context.InstallProperties);
  return SequenceHookResult.Continue;
});
```

If you already have a **`Dispatcher`** at registration time, you can skip the context invoker:

```csharp
context.SequenceHooks.RegisterAsync("BeforeInstallFiles", wizardView.Dispatcher, async ctx =>
{
  await ValidateAsync(ctx);
  return SequenceHookResult.Continue;
});
```

Requires the **`WiXCraft.Wpf`** package (pulled in automatically with **`WiXCraft.Installer.Wpf`**).

WiXCraft ships one shared **`WiXCraft_SequenceHook`** immediate CA. Each hook sets `WiXCraft_SequenceHook=HOOKID=...;PAYLOAD=...` and invokes the CA, which posts an MSI **`User`** message to the embedded UI (`WIXCRAFT_HOOK:{id}` protocol).

| Tier | Mechanism | Author writes C# CA? |
|------|-----------|----------------------|
| Observe sequence | `ExecuteSequence` / `ActionStart` | No |
| Pass config to MSI | `InstallProperties` + declarative WiX | No |
| Interactive gate at a sequence point | `WiXCraftSequenceHook` + UI handler | No (WiXCraft CA only) |
| Elevated/deferred machine work | `ApplySessionProperties` stub + future packaged CAs | Optional WiXCraft deferred CA |

### Deferred / elevated work (limits)

UI hooks and property bags run in the **embedded UI / immediate** context. They cannot replace deferred, elevated custom actions (IIS, SQL, services) by themselves.

WiXCraft includes a deferred **`ApplySessionProperties`** stub that logs and documents the Tier 2 pattern: set properties from the UI before install, then consume them in declarative WiX or a packaged WiXCraft deferred CA.

**Decision guide:**
- Progress labels / timeline → **`ExecuteSequence`**
- Config from wizard → **`InstallProperties`** + WiX tables
- Block/continue before a sequence step → **`WiXCraftSequenceHook`**
- Machine-wide imperative install logic → declarative WiX or deferred CA

## Create your own embedded UI

### 1. WPF UI project

Create a `net48` WPF class library with platform `x86`:

```xml
<PropertyGroup>
  <TargetFramework>net48</TargetFramework>
  <UseWPF>true</UseWPF>
  <PlatformTarget>x86</PlatformTarget>
  <UseWiXCraftEmbeddedUi>true</UseWiXCraftEmbeddedUi>
  <UseWiXCraftEmbeddedUiWpf>true</UseWiXCraftEmbeddedUiWpf>
  <EmbeddedUiHostFactory>YourAssembly!YourNamespace.YourInstallerUiHostFactory</EmbeddedUiHostFactory>
</PropertyGroup>

<ItemGroup>
  <PackageReference Include="WiXCraft.Installer.Wpf" Version="1.0.0" />
</ItemGroup>
```

Implement:

- `IInstallerUiHostFactory` → returns your `IInstallerUiHost`, `InstallerUiModeOptions`, optional `IInstallerUiLifecycle`, and `ConfigureSequenceHooks`
- `IInstallerUiHost` → run WPF, forward `ProcessMessage` / `EnableExit` to your UI

See `ExampleInterface` for a full pattern with dependency injection, MVVM, and views.

### 2. WiX installer project

```xml
<PropertyGroup>
  <UseWiXCraftInstaller>true</UseWiXCraftInstaller>
  <WiXCraftIncludeEmbeddedUi>true</WiXCraftIncludeEmbeddedUi>
</PropertyGroup>

<ItemGroup>
  <ProjectReference Include="..\YourUiProject\YourUiProject.csproj" />
  <ProjectReference Include="..\WiXCraft.CustomActions\WiXCraft.CustomActions.csproj" />
  <PackageReference Include="WiXCraft.Installer" Version="1.0.0" />
</ItemGroup>
```

In `Product.wxs` (or equivalent):

```xml
<?include WiXCraft.generated.wxi ?>
```

## Development mode

`src\Directory.Build.props` sets `WiXCraftDevelopmentMode=true` by default. In this mode the solution builds against **project references** instead of NuGet packages, so you can iterate on WiXCraft and the sample together.

Set `WiXCraftDevelopmentMode=false` (and pack/publish the WiXCraft packages) to consume WiXCraft as NuGet packages like a downstream project would.

## NuGet packages

| Package | Purpose |
|---------|---------|
| `WiXCraft.Core` | Embedded UI engine and installer context APIs |
| `WiXCraft.Wpf` | WPF helpers (`DispatcherSequenceHookAsyncInvoker`, async hook registration) |
| `WiXCraft.Installer` | MSBuild targets for WiX projects |
| `WiXCraft.Installer.Wpf` | WPF `IEmbeddedUI` entry point, config generation, Core reference |

Package versions are defined in `src\Directory.Build.props`.

## Sample UI highlights

`ExampleInterface` is intentionally polished to demonstrate what a production-style embedded UI can look like:

- Dark red MahApps.Metro theme with custom installer resources
- Windows 11 Mica and rounded window corners (when supported)
- Feature selection with maintenance flows (repair / modify / remove)
- Live **current action** text during install
- Animated success/failure finish screen
- Collapsible **Details** section (session properties + log) for diagnostics

## Notes

- Embedded UI must stay **x86** (`PlatformTarget`) for compatibility with WiX embedded UI hosting.
