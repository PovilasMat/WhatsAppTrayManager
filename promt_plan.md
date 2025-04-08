# I'll write the content of the blueprint and prompt plan to a file named "prompt_plan.md".
prompt_plan_content = r'''# WhatsApp Tray Manager – Implementation Blueprint and Prompt Plan

## Project Blueprint

This document outlines a comprehensive plan for building the WhatsApp Tray Manager application in C# (.NET 6+ with Windows Forms), following the provided specification. The application enhances the WhatsApp Desktop experience by adding a persistent tray icon with dynamic unread badges, manage minimizing to tray behavior, auto-start configuration, and more.

### Architecture & Components:
1. **Tray Icon Controller:** 
   - Manages a system tray icon using Windows Forms `NotifyIcon`.
   - Handles left/right click events and displays a context menu with options:
     - Show WhatsApp
     - Quit WhatsApp
     - Start with Windows (toggle)
     - Exit Tray Tool
   - Dynamically updates the icon based on WhatsApp’s running state and unread message count.

2. **WhatsApp Window Manager:**
   - Detects the running WhatsApp process and attaches to its window.
   - Handles window show/hide (minimize-to-tray) behavior.
   - Intercepts close operations (by disabling the close button) so that clicking X hides the window instead of closing it.
   - Maintains last known window state (position, maximized/minimized) to restore properly.

3. **Unread Count Detector:**
   - Polls every few seconds (configurable, default 5 sec) for unread messages.
   - Reads unread count via window title scraping and UI Automation (OCR fallback is omitted in v1).
   - Triggers updates to the Tray Icon Controller to display numbered icons ("msg-1" to "msg-5", and "msg-5plus" for counts >5).

4. **Icon Manager:**
   - Supplies tray icons for different states.
   - Generates placeholder icons on-the-fly if not provided as external resources.
   - Caches generated icons for efficient reuse.

5. **Settings Handler:**
   - Reads and writes configuration from `config.json` (containing `start_with_windows` and `poll_interval_seconds`).
   - Manages the auto-start registry entry at `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`.

### High-Level Steps:
1. **Project Setup & Basic Tray Icon:**  
   Initialize a .NET 6 WinForms project and test project, create an `ApplicationContext` with a `NotifyIcon` that shows a basic tray icon and a context menu with an Exit option.

2. **Icon Manager and Initial Configuration:**  
   Develop the Icon Manager to generate placeholder icons, and implement config file loading/saving via Settings Handler.  
   
3. **WhatsApp Detection and Tray Integration:**  
   Implement the WhatsApp Window Manager to detect WhatsApp, update tray icon based on its state, and show a one-time notification if WhatsApp isn’t running.

4. **Minimize-to-Tray Functionality:**  
   Enable real show/hide behavior using Win32 API calls (PInvoke) for hiding and restoring the WhatsApp window. Save and restore window state.

5. **Unread Count Monitoring:**  
   Build the UnreadCountDetector to poll for unread messages using title parsing and UI Automation, and update the tray icon accordingly.

6. **Settings Persistence and Auto-Start:**  
   Finalize Settings Handler to persist changes via `config.json` and update the Windows Registry based on the “Start with Windows” setting.

7. **Integration & Cleanup:**  
   Tie all components together, ensure proper disposal and resource cleanup, and perform both automated and manual testing based on provided test cases.

---

## Iterative Development Chunks & Step-by-Step Breakdown

Each development chunk is broken down into small, test-driven steps:

1. **Initialize Solution and Projects:**
   - Create a WinForms project (`WhatsAppTrayManager`) and a test project (`WhatsAppTrayManager.Tests`).
   - Write a dummy test to verify the testing framework.

2. **Basic ApplicationContext with NotifyIcon:**
   - Implement `TrayApplicationContext` with a NotifyIcon and an Exit context menu.
   - Write unit tests to verify the NotifyIcon is visible and the Exit action works.

3. **Icon Manager Stub:**
   - Create an `IconManager` class with a method `GetIcon(string key)`.
   - Stub out functionality to return a default icon, then write tests for known keys and error handling.

4. **Generate Placeholder Icons:**
   - Expand IconManager to generate distinct placeholder icons (for keys: "default", "gray", "msg-1", ..., "msg-5", "msg-5plus") using GDI+.
   - Write tests to verify icon generation and caching behavior.

5. **Configuration File Handling:**
   - Implement `Settings` and `SettingsHandler` classes to load and save `config.json`.
   - Write tests to check default values and proper parsing.

6. **Apply Auto-Start Setting (Registry):**
   - Extend SettingsHandler to write/remove the auto-start registry entry.
   - Write tests using a fake registry service for injection.

7. **WhatsApp Window Manager – Process Detection:**
   - Create `WhatsAppWindowManager` to detect the WhatsApp process and expose its window handle.
   - Use events to signal process start/exit.
   - Write tests by injecting a fake process finder.

8. **Tray Icon Reaction to WhatsApp State:**
   - Wire up WindowManager events to update the tray icon (switching between default and gray icons) and show one-time notifications.
   - Stub out left-click toggle logic and menu items (Show WhatsApp, Quit WhatsApp).
   - Write tests to simulate events and verify correct behavior.

9. **Implement Show/Hide Window (Win32 Integration):**
   - Use PInvoke to implement `HideWindow()` and `ShowWindow()` in WhatsAppWindowManager.
   - Save and restore window state using Win32 API calls.
   - Write integration tests (using a dummy window) and perform manual testing.

10. **Prevent WhatsApp from Closing on X:**
    - Disable the close button of WhatsApp by removing the close option from its system menu.
    - Implement "Quit WhatsApp" action to force-close via WindowManager.
    - Write tests and perform manual verification.

11. **Unread Count Detector Implementation:**
    - Create `UnreadCountDetector` that polls for unread count using window title parsing and UI Automation.
    - Raise an event `UnreadCountChanged(int newCount)` on changes.
    - Write unit tests for the parsing logic and event triggering using a fake data source.

12. **Update Tray Icon on Unread Count Changes:**
    - Subscribe to unread count events and update the tray icon via IconManager.
    - Update the tooltip to reflect the number of unread messages.
    - Write tests for icon mapping (e.g., count 0 -> "default", 1-5 -> "msg-N", >5 -> "msg-5plus").

13. **“Start with Windows” Toggle Implementation:**
    - Link the "Start with Windows" context menu item to SettingsHandler.
    - Toggle and persist the setting, updating both the UI and registry.
    - Write tests simulating menu clicks and verifying config and registry changes.

14. **Final Touches – Stability and Cleanup:**
    - Implement proper disposal of resources in TrayApplicationContext.
    - Ensure one-time notifications and clean state transitions.
    - Write final integration tests and perform manual testing as per the test plan.

---

## TDD-Focused Prompt Set

Each of the following code block prompts corresponds to a step in the process. Use these prompts to incrementally develop the application, ensuring that each step is testable and integrated with previous code.

```markdown
### Step 1: Project Initialization
**Goal:** Set up a new C# solution with a WinForms application and a test project.
1. Create a WinForms .NET 6 project named `WhatsAppTrayManager` with an empty `Program.cs` that will serve as the startup (ensure it calls `Application.Run` on an ApplicationContext or Form, to be implemented later).
2. Create a test project `WhatsAppTrayManager.Tests` using xUnit (add reference to the WinForms project if needed).
3. Write a basic test in `WhatsAppTrayManager.Tests` (e.g., `DummyTests.cs`) to assert that the test framework runs (e.g., `Assert.True(true)`).
4. Run the tests to ensure the setup is correct.
**Best Practices:** Organize the solution with clear project names. Ensure the test project can reference the main project for upcoming tests. No application logic yet.
**Output:** The solution structure and a passing dummy test.

### Step 2: Basic Tray ApplicationContext with Exit
**Goal:** Implement a minimal ApplicationContext with a NotifyIcon and an Exit option.
1. In the WinForms project, create a class `TrayApplicationContext` inheriting `ApplicationContext`. In its constructor, initialize a `NotifyIcon`.
   - Use a temporary icon (e.g., `SystemIcons.Application`) and set `NotifyIcon.Visible = true` and `Text = "WhatsApp Tray Manager"`.
   - Create a ContextMenu (or ContextMenuStrip) with one MenuItem: "Exit". Attach a click event handler that calls `Application.Exit()`.
   - Assign this menu to the NotifyIcon (use `NotifyIcon.ContextMenu` or `ContextMenuStrip` accordingly).
2. Modify `Program.cs` to instantiate `TrayApplicationContext` and run it (`Application.Run(new TrayApplicationContext());`).
3. Write a test class `TrayApplicationContextTests`. Because testing a UI ApplicationContext directly is tricky, expose minimal internal state for verification:
   - Perhaps add a property in `TrayApplicationContext` to retrieve the NotifyIcon (or its visibility status) for testing.
   - Write a test `NotifyIcon_IsVisibleUponStart()` that creates the context (on a separate thread if needed, or call an init method) and asserts that `NotifyIcon.Visible` is true.
   - Write a test `ExitMenu_ExitsApplication()` that simulates a click on the "Exit" menu item. You may simulate by calling the event handler directly. After calling it, verify that `Application.MessageLoop` is no longer running. (If direct verification is hard, you can simulate by overriding `OnMainFormClosed` or using a flag set in an overridden `ExitThread` method.)
4. Run tests. They should pass (or at least the structure should compile; for the exit test, you might have to simulate environment – ensure the code is structured for testability, e.g., you could override `Application.Exit()` via a delegate for testing purposes).
**Best Practices:** Use `using System.Windows.Forms;` for WinForms classes. Ensure proper disposal of NotifyIcon (we'll handle disposal in a later step, possibly). Keep UI code on the main thread – for tests, we might not actually run the message loop, just test the setup logic.

### Step 3: Icon Manager Stub
**Goal:** Introduce an IconManager to manage tray icons, with a simple stub implementation.
1. Create a class `IconManager` in the main project. Give it a method `public Icon GetIcon(string key)` that will eventually return different icons based on `key`.
   - For now, implement `GetIcon` to return a default icon for any input. You can use `SystemIcons.Application` or create a 16x16 blank bitmap and convert to Icon. This is a placeholder.
   - Define expected icon keys as constants or an enum for clarity (e.g., "default", "gray", "msg-1", ..., "msg-5plus"), but for now, only "default" will be used.
2. Modify `TrayApplicationContext` to use `IconManager`. Initialize an `IconManager` instance in the context. Instead of using `SystemIcons.Application` directly, call `notifyIcon.Icon = iconManager.GetIcon("default")`.
3. Write tests in `IconManagerTests` (new class in test project):
   - `GetIcon_ReturnsIconObject`: Call `new IconManager().GetIcon("default")` and assert the result is not null and is of type `Icon`.
   - `GetIcon_UnknownKey_Throws`: Optionally, decide that if an unknown key is passed, the method should throw an ArgumentException. Test that behavior (e.g., `Assert.Throws<ArgumentException>(() => iconManager.GetIcon("invalid"))`).
4. Run tests. The tests for IconManager should pass. Also ensure existing tests still pass.
**Best Practices:** Keep IconManager logic separated from UI so it’s easily testable. Currently it’s simple; we will expand it in the next step. Use .NET naming conventions (e.g., method PascalCase, etc.). Document with summary comments what IconManager is responsible for.

### Step 4: Generate Placeholder Icons
**Goal:** Implement IconManager to generate distinct placeholder .ico images for each required icon state.
1. In `IconManager`, create a method `private Icon GenerateIcon(string key)` that creates an Icon based on the key:
   - If key is "default": perhaps draw a green circle with no number.
   - If "gray": draw a gray circle.
   - If "msg-1" to "msg-5": draw a base icon (e.g., green circle) with the number (1 through 5) in the center.
   - If "msg-5plus": draw a circle with "5+" text.
   - Use `System.Drawing.Bitmap` (size 16x16 or 32x32 for better clarity) and `Graphics` to draw (e.g., fill ellipse, draw text). Use a font that's readable at small size for numbers (you might use a bold font, size ~8 for 16x16).
   - After drawing, convert to Icon: use `Bitmap.GetHicon()` and `Icon.FromHandle`. Be careful to destroy the Hicon (you can destroy it using `DestroyIcon` P/Invoke or via Icon.Dispose later).
   - Cache the generated Icon in a Dictionary<string, Icon> within IconManager so you only generate once per key.
2. Update `GetIcon(string key)` to:
   - If the icon for that key is already in cache, return it.
   - Otherwise, call `GenerateIcon(key)`, store it, and return it.
3. Add a cleanup mechanism (maybe a `Dispose()` for IconManager) to dispose all cached Icons when done – we will use it later on application exit.
4. Write tests for icon generation:
   - `GenerateIcon_CreatesDistinctIcons`: Call GetIcon for "default", "gray", "msg-1", "msg-2", etc., and ensure each call returns an Icon. Possibly test that "gray" icon is visually different from "default". We can do this by comparing some pixel color: e.g., convert the Icon to Bitmap and check one pixel or the average brightness to ensure gray icon is gray (or simpler: call GetIcon("default") and GetIcon("gray") and ensure they are not the same reference, and maybe not the same pixel data. However, comparing pixel data exactly might be flaky; at least ensure the references differ and no exception).
   - `GetIcon_CachesIcons`: Call GetIcon("msg-3") twice and ensure it returns the same reference both times (to verify caching).
5. Run tests. They should pass if the generation logic is correct. The visual differences can be manually verified by running the app and observing the tray icons if possible.
**Best Practices:** Use using/dispose for GDI objects (Graphics, etc.). Keep the design simple – these are placeholders, so no need for perfection in design, just distinguishable colors/text. Ensure thread-safety of IconManager (accessing from UI thread only is fine). Document the method of generation for future reference (so someone can replace with actual icons easily).

### Step 5: Configuration File Handling
**Goal:** Implement reading of config.json for startup settings.
1. Create a class `Settings` with properties `StartWithWindows` (bool) and `PollIntervalSeconds` (int). Match the JSON structure from spec (camelCase or snake_case as needed; the example shows `start_with_windows` and `poll_interval_seconds` in snake_case&#8203;:contentReference[oaicite:32]{index=32}, decide on a strategy: you can use `[JsonPropertyName]` attributes to map to snake_case).
2. Create a class `SettingsHandler` with methods `Load()` and `Save(Settings settings)`. 
   - `Load()` should attempt to read `config.json` from the same directory as the executable. Use `File.Exists` and `File.ReadAllText` plus `JsonSerializer.Deserialize<Settings>` (with appropriate options if needed for naming). If file not found, return a Settings with default values (StartWithWindows=false (or true? spec default example shows true&#8203;:contentReference[oaicite:33]{index=33}, but let's default to false to avoid auto-start unless user enabled) and PollIntervalSeconds=5). If file exists but is malformed, catch exception and also return defaults.
   - `Save(Settings settings)` writes the JSON to `config.json` (use `JsonSerializer.Serialize(settings)`).
   - Ensure any file I/O exceptions are handled (e.g., log or debug output, but don't crash).
3. In `TrayApplicationContext` (or Program before creating context), call `SettingsHandler.Load()` to get the settings. Store it (e.g., as a static or pass into TrayApplicationContext). For now, just keep it for later use.
4. Write tests in `SettingsHandlerTests`:
   - `Load_ReturnsDefaults_WhenFileMissing`: ensure that if no config file present, Load returns an instance with PollIntervalSeconds=5 and StartWithWindows=false (or true if you chose that default from spec – spec example JSON has true, but it's up to design; use false to be safe, as user can enable).
   - `Save_CreatesFile_And_Load_ReadsIt`: Use a temporary file path (you might inject a file path into SettingsHandler for test, so it doesn't write to actual config.json). Write a Settings with known values, call Save, then modify the file path or content and call Load to see if it matches.
   - `Load_ParsesFileCorrectly`: Create a JSON string with specific values, write to a temp file, then Load and verify Settings properties match.
   - Ensure to clean up temp files after tests.
5. Run tests to confirm config handling works. (You may need to adjust file path handling – consider having SettingsHandler accept a path for easier testing.)
**Best Practices:** Use `Environment.GetFolderPath` or `AppContext.BaseDirectory` to find the executable directory. For testing, allow dependency injection of file path or file system (to avoid touching real disk if not desired). Use System.Text.Json with `IgnoreNullValues` or appropriate options to match naming. Keep default values in a single place (maybe constants).

### Step 6: Apply Auto-Start Setting (Registry)
**Goal:** Manage the Windows registry entry for startup based on StartWithWindows setting.
1. In `SettingsHandler`, implement `EnableAutoStart()` and `DisableAutoStart()` methods, or a single `ApplyAutoStart(bool enable)`.
   - Use `Microsoft.Win32.Registry` to open `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`. 
   - If `enable` is true, set the value `"WhatsAppTrayManager"` to the full path of the application executable (you can get this via `Process.GetCurrentProcess().MainModule.FileName` or `Application.ExecutablePath`).
   - If `enable` is false, delete the value `"WhatsAppTrayManager"` if it exists.
   - Handle security exceptions or other errors gracefully (likely not an issue in HKCU).
2. On application startup, after loading settings (Step 5), apply the auto-start: `ApplyAutoStart(settings.StartWithWindows)`. This ensures the registry is updated to match the config (e.g., first run, if user hasn’t changed it, it might default to false and remove any existing entry).
3. Write tests for registry handling:
   - Since editing the real registry in tests is not ideal, abstract registry operations. For example, create an interface `IRegistryService` with methods to set and delete values, and inject a fake implementation in tests that just records what happens.
   - Implement a `RegistryService` for production that calls the actual Registry APIs.
   - In `SettingsHandler`, use `IRegistryService` (pass it in via constructor). In tests, provide a `FakeRegistryService`.
   - Test `ApplyAutoStart(true)` uses the service to set the correct key and value. The fake can store lastWrittenKey = path, etc., and you assert that it matches the expected exe path.
   - Test `ApplyAutoStart(false)` causes the service to attempt deletion.
   - Optionally, if you want to integration-test the real registry, you could call ApplyAutoStart(true) and then read the value back from Registry.CurrentUser in the test to confirm, then clean it, but this might be flaky if no permissions or leaves state behind.
4. Ensure tests pass using the fake service. The integration with actual registry will be tested manually or in a separate environment.
**Best Practices:** Keep registry access minimal and only at startup or when toggling setting. Do not litter registry with multiple writes (just update when necessary). Use using statements or close the registry key after writing. Document in comments the registry path being used.

### Step 7: WhatsApp Window Manager - Process Detection
**Goal:** Detect the running WhatsApp process and track its main window.
1. Create a class `WhatsAppWindowManager`. Give it fields or properties to track if WhatsApp is running (`IsRunning` bool), the Process, and the main window handle (`IntPtr WhatsAppWindowHandle`).
2. Implement a method `FindWhatsApp()` that searches for the process:
   - Use `Process.GetProcessesByName("WhatsApp")`. If it returns any process, pick the first one (assuming one instance). Alternatively, use LINQ to find a process whose MainWindowTitle contains "WhatsApp" if process name is not exactly "WhatsApp" (in case the process name differs).
   - If found, set `IsRunning = true`, store the process (and optionally subscribe to `Process.Exited` event with `EnableRaisingEvents` to detect if it exits).
   - Also retrieve `WhatsAppWindowHandle = process.MainWindowHandle` (if the process has a main window).
   - If not found, set `IsRunning = false` and handle accordingly.
3. Add events in WindowManager: `public event EventHandler WhatsAppStarted;` and `public event EventHandler WhatsAppExited;`. After a `FindWhatsApp()` call, if the state changed from not running to running, raise WhatsAppStarted. If changed from running to not running (process exited), raise WhatsAppExited.
4. Integrate with TrayApplicationContext (or a new higher-level controller class):
   - Instantiate WhatsAppWindowManager at startup. Call `FindWhatsApp()` once during initialization.
   - Depending on result, set the tray icon: if `IsRunning` is false, use gray icon; if true, use default icon.
   - If false, also show the balloon tip "WhatsApp is not running" (use `notifyIcon.ShowBalloonTip(timeout, title, text, ToolTipIcon.Info)`). Ensure this happens only once; maybe track a bool so you don't show it repeatedly if `FindWhatsApp()` is called multiple times. 
   - Optionally, start a timer to periodically call `FindWhatsApp()` every few seconds to catch if WhatsApp is launched later or closed (though the UnreadCountDetector could incorporate this check too – we can decide to poll for process in the same loop as unread count).
5. Write tests:
   - Use dependency injection to test `FindWhatsApp()`: have `WhatsAppWindowManager` accept an interface `IProcessFinder` with a method that returns a list of processes by name. Implement a `FakeProcessFinder` in tests that can simulate found or not found.
   - Test that when fake returns a process, `IsRunning` becomes true and event WhatsAppStarted is raised; when fake returns none, `IsRunning` false and event WhatsAppExited is raised (if it was previously true).
   - You can simulate transitions by first having the fake return none (simulate not running), call FindWhatsApp (no event if initially false->false), then have fake return a process and call FindWhatsApp again (should trigger Started event).
   - Also test that the stored window handle is the process's MainWindowHandle.
   - For tray integration, you might test via TrayIconController (if you have one) or directly: have a fake WindowManager that you flip `IsRunning` and see if tray icon image key changes (for that, you need a way to inspect the tray icon state as before, maybe an internal property).
6. Run tests to ensure detection logic is sound. 
**Best Practices:** Use robust process search (maybe add logic to ensure the process has a window handle before considering it "running" for our purposes, because WhatsApp might start up with no UI for a moment). Ensure to handle multiple processes (if user somehow has two instances, maybe take the first or the one with a window). Avoid heavy CPU usage – if we poll, 5s interval is fine as per config.

### Step 8: Tray Icon Reaction to WhatsApp State
**Goal:** Connect WindowManager events to the tray icon and implement one-time "not running" notification.
1. Update `TrayApplicationContext` or create a new controller (e.g., `TrayIconController`) that holds references to IconManager and WhatsAppWindowManager.
   - If using TrayApplicationContext, within it after initializing NotifyIcon and WindowManager, subscribe to WindowManager.WhatsAppStarted and WhatsAppExited events.
   - On WhatsAppStarted: set `notifyIcon.Icon = iconManager.GetIcon("default")` (assuming 0 unread at start) and maybe update tooltip text to "WhatsApp is running".
   - On WhatsAppExited: set `notifyIcon.Icon = iconManager.GetIcon("gray")`; and show a balloon tip "WhatsApp is not running" **if** we haven't shown it before for this session or state change. Use a bool flag `shownNotRunningTip` that resets when WhatsAppStarted occurs.
   - Also handle the initial state: if at startup FindWhatsApp found nothing, we should have shown the tip. If it found something, no tip.
2. Implement left-click toggle logic stub (since actual show/hide comes later, here we just plan it):
   - Subscribe to `notifyIcon.MouseClick` (or MouseUp) for left-click. In handler, check `e.Button`.
   - If it's a left click (and maybe also handle double-click similarly), then:
     * If WindowManager.IsRunning is false: (WhatsApp not running) – optionally, could show the same "not running" balloon or simply do nothing. (We won't launch WhatsApp as per spec.)
     * If WindowManager.IsRunning is true:
       - If we have a flag indicating "WhatsApp window currently visible or not", toggle it. (We haven't developed that fully yet; maybe maintain a bool `isWaVisible` initially true if started.)
       - If visible: we will call a `HideWindow()` method on WindowManager (which we will implement later; for now, just log or set a flag).
       - If hidden: call `ShowWindow()` method on WindowManager.
     * We might stub these methods as: in WindowManager, add `public bool IsWindowVisible` property (start it as true when process found). In left-click handler, flip this property and raise an event or so. We'll fully implement the hide/show in a coming step.
   - Essentially, set up the structure for toggling, without real hiding yet.
3. Integrate the context menu items:
   - "Show WhatsApp": on click, similar to left-click logic – if WA is running and hidden, show it (for now, maybe just set IsWindowVisible true; we'll do actual show later). If WA running and visible, maybe just bring to front (we handle that later). If WA not running, perhaps disabled or just does nothing.
   - "Quit WhatsApp": on click, if WA is running, call WindowManager.Process.Kill or a stub method WindowManager.Quit() that wraps process closing. After that, we expect WhatsAppExited event to fire and update tray.
   - Ensure "Start with Windows" and "Exit" still function (we will implement Start with Windows toggling in a later step).
4. Write tests:
   - For events: simulate WhatsAppStarted event on WindowManager and ensure tray icon switches to default icon. This can be done by injecting a fake IconManager that records the requested icon key (should be "default"). Similarly, simulate WhatsAppExited and ensure "gray" icon requested and that the balloon tip would show (we can’t easily capture a balloon tip in unit test, but we can expose an internal flag or method call for showing notifications to verify it was invoked).
   - Test left-click handler logic: set up a fake WindowManager state (IsRunning true/false and perhaps IsWindowVisible flag). Simulate a left-click event (you can call the handler directly with a MouseEventArgs for Left button). If WA not running, ensure nothing crashes (maybe a flag "actionInvoked" stays false). If running and visible, ensure it calls WindowManager.Hide (you might have a fake WindowManager with a Hide() method that sets a flag). If running and not visible, ensure it calls WindowManager.Show.
   - Test "Quit WhatsApp": use a fake WindowManager with a `Quit()` method that sets a flag or increments a call count. Simulate clicking Quit menu and verify the fake’s Quit was called and that ultimately WhatsAppExited event would be triggered (you can simulate it or check that our code would handle it).
   - (We will test Start with Windows toggle in a later step when implemented).
5. Run tests and ensure they pass. The actual show/hide functionality is not done yet, but we've laid the groundwork with correct method calls and state tracking.
**Best Practices:** Keep the UI event handlers simple, deferring logic to WindowManager where possible. Use clear naming for flags like `isWaVisible`. Ensure thread affinity (Tray events are on UI thread, WindowManager likely too since we call it from UI thread). Maintain separation: TrayIconController shouldn’t deeply know how WindowManager hides, just calls its methods.

### Step 9: Implement Show/Hide Window (Win32 Integration)
**Goal:** Enable the actual hiding and showing of the WhatsApp window using Windows API calls.
1. In `WhatsAppWindowManager`, implement methods `HideWindow()` and `ShowWindow()`. Use PInvoke to call user32 functions:
   - Define `[DllImport("user32.dll")] static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);` and constants for `SW_HIDE = 0`, `SW_SHOW = 5`, `SW_RESTORE = 9`, `SW_SHOWMAXIMIZED = 3` (etc., as needed).
   - Also import `SetForegroundWindow(IntPtr hWnd)` to bring window to front.
   - Import `GetWindowPlacement` and `SetWindowPlacement` (or simpler: `GetWindowRect` to get position).
   - Also `GetWindowLong`/`SetWindowLong` if needed for style modifications.
2. In `HideWindow()`: if `WhatsAppWindowHandle` is valid:
   - Retrieve current window placement (`GetWindowPlacement`) to store restore state (especially if maximized or minimized).
   - Call `ShowWindow(hWnd, SW_HIDE)`. This will hide the window (it disappears from taskbar but process remains).
   - Optionally, set a flag `isHiddenToTray = true`. Also, you might want to distinguish between user-initiated hide vs window still open.
3. In `ShowWindow()` (to restore): if `WhatsAppWindowHandle` is valid:
   - If you saved placement and it indicated maximized, call `ShowWindow(hWnd, SW_SHOWMAXIMIZED)`.
   - If normal, call `ShowWindow(hWnd, SW_RESTORE)` (which should show and restore if minimized, or just show if hidden).
   - After showing, call `SetForegroundWindow(hWnd)` to bring it to focus.
   - Also, if you saved position coordinates (with GetWindowPlacement or GetWindowRect before hiding), you might call `SetWindowPos` to ensure the window is at the same location. (If GetWindowPlacement was used, using SetWindowPlacement with the saved structure after showing may restore position/size.)
   - Set `isHiddenToTray = false`.
4. Connect these to the Tray toggle logic:
   - In the Tray left-click or "Show WhatsApp" handler, instead of just flipping a flag, call `WindowManager.HideWindow()` or `ShowWindow()` accordingly. The WindowManager should update its own `IsWindowVisible` or `isHiddenToTray` internally.
   - Perhaps also update the tray icon tooltip or state if needed (maybe not; icon stays same aside from unread count).
5. Testing:
   - Write integration-style tests for Hide/ShowWindow if possible. One approach: start a dummy WinForms form (or use an existing process window) as a stand-in for WhatsApp. You can in test get its handle. Then call our WindowManager.HideWindow on that handle and verify the window’s `Visible` property becomes false or call `IsWindowVisible` API to check. Then call ShowWindow and verify the window is visible again and perhaps active.
   - Alternatively, create a small hidden form in the test, assign its Handle to WindowManager.WhatsAppWindowHandle, simulate it being in normal state by setting WindowManager fields, then call HideWindow and check via `IsWindowVisible` PInvoke (import that too). Then ShowWindow and check again.
   - Test that maximized state is preserved: you may need to maximize the dummy window, then hide and restore, and check if after ShowWindow it’s still maximized (maybe via `IsZoomed` API or checking window style).
   - These tests might be flaky depending on environment, but try to automate what’s feasible.
   - Also test that our WindowManager’s internal flags update (set isHiddenToTray true after hide, false after show).
6. Manual testing will be important here: run the TrayManager with a real WhatsApp (or any other app if needed to test) and click the tray icon to hide/show. Ensure the window disappears and reappears properly, and on reappear it comes to front and in correct state.
**Best Practices:** Ensure that Hide/Show are idempotent (hiding an already hidden window or showing an already visible one should not error). Use logging or Debug.WriteLine for now to track actions (useful for debugging). Manage window handles carefully – avoid using an invalid handle. Also consider if WhatsApp was minimized (SW_MINIMIZE) when we try to hide – SW_HIDE should still hide it, that’s fine.

### Step 10: Prevent WhatsApp from Closing on X
**Goal:** Intercept or prevent the WhatsApp window from closing so that clicking X just hides it.
1. Simplest approach: disable the close button on the WhatsApp window. In WindowManager, after finding the window handle, call `GetSystemMenu(hWnd, false)` and `RemoveMenu(hMenu, SC_CLOSE, MF_BYCOMMAND)` to remove the close option.
   - PInvoke: `[DllImport("user32.dll")] static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);`, `[DllImport("user32.dll")] static extern bool RemoveMenu(IntPtr hMenu, uint uPosition, uint uFlags);`
   - Constants: `SC_CLOSE = 0xF060`, `MF_BYCOMMAND = 0x0000`.
   - This will gray out/disable the X button.
   - Call this in `FindWhatsApp()` when you detect the process and have the handle (and possibly again if needed if WA restarts).
2. Additionally (optional), you can set up a hook to catch WM_CLOSE if you want extra safety:
   - (This is advanced; if doing, you’d create a Win32 hook via `SetWinEventHook` for EVENT_OBJECT_DESTROY on that hWnd to detect if it's closing, then call HideWindow and cancel. But since disabling the close button likely suffices, we skip actual hooks to avoid complexity.)
3. Ensure that when the user clicks X, nothing happens visually (the window might not close because we removed the menu item).
   - The WhatsAppWindowManager could still listen for the Process.Exited event. If despite removal, the process ends, then WhatsAppExited will fire and tray will go gray. But ideally, user cannot close via X now.
4. Update "Quit WhatsApp" menu to force close WhatsApp:
   - Implement WindowManager.QuitWhatsApp() to re-enable the close menu (if needed) and send a WM_CLOSE or just call Process.Kill.
   - Simpler: call `process.Kill()` directly to terminate WhatsApp (since user explicitly chose to quit).
   - After kill, wait for Process.Exited event to fire (which triggers our existing logic to update tray icon).
5. Testing:
   - Automated: We can simulate disabling close on a dummy window and verify the close box is disabled. Possibly check the window style or send a SC_CLOSE command via SendMessage to see if it's ignored.
   - For Quit, we can test that calling WindowManager.QuitWhatsApp sets IsRunning to false (perhaps via the Process.Exited event simulation in tests).
   - Most critically, manual testing: Start WhatsApp, click the X – it should not close (if our app is running and already removed the menu). Instead, maybe nothing apparent happens, or at most the window might remain (if we didn't hide on X automatically). 
   - We might want to hide the window immediately when X is clicked. Removing the menu prevents the click from doing anything, but the window stays open. To mimic typical minimize-to-tray, we can also handle the X click via the FormClosing event if it were our own app. But for external, maybe use UI Automation to catch the invoke of close button:
     * Alternatively: Use `SetWinEventHook` for EVENT_OBJECT_INVOKED on the close button. But that's complex.
   - Possibly simpler: since close is disabled, instruct user via documentation to use tray to hide. But better: also hide when X is clicked even if it’s disabled. Maybe monitor focus or border events.
   - Due to complexity, we'll settle for disabling close which means clicking X will do nothing (window stays open). The user can then use tray to hide it. This is a slight deviation (the spec wanted clicking X to hide it automatically).
   - If time permits, implement a low-level CBT hook to catch the close attempt: (Skip in this prompt for now).
6. So, proceed with just disabling close and rely on user to use tray icon. Document this limitation if needed.
   - The minimize button on WhatsApp will still minimize to taskbar normally; user can then click tray icon to restore as well (which should also be supported).
7. Ensure "Quit WhatsApp" still works (tested earlier).
**Best Practices:** This solution is a trade-off – simpler and safer than injecting hooks. We ensure the user has a clear way to exit (tray menu). We should clearly name methods (like maybe call it `PreventClose()` instead of hook to make code intention clear). Add comments that further enhancement could hook WM_CLOSE if needed.

### Step 11: UnreadCountDetector with Title and UIAutomation
**Goal:** Poll for unread messages count and notify tray icon of changes.
1. Create class `UnreadCountDetector`. Give it:
   - A property `CurrentUnreadCount` (int).
   - An event `UnreadCountChanged(int newCount)`.
   - Possibly a reference to `WhatsAppWindowManager` or directly to the window handle or process (so it knows where to get info).
   - A Timer (System.Timers.Timer or System.Windows.Forms.Timer). Use the poll interval from settings (default 5 sec).
2. Implement `Start()` method in UnreadCountDetector:
   - Only start if WindowManager.IsRunning is true. If WhatsApp is not running, perhaps do nothing or start and just always get 0.
   - Timer callback should call a method `CheckUnreadCount()`.
3. Implement `CheckUnreadCount()`:
   - If WindowManager.IsRunning is false or window handle is invalid, possibly reset CurrentUnreadCount to 0 and return.
   - Else, attempt to read unread count:
     * Method 1: Window title. Use `Process.MainWindowTitle` or `GetWindowText(WhatsAppWindowHandle, ...)` PInvoke.
       - Parse the title for numbers. For example, if title contains a number in parentheses or other delimiters. Implement a parsing function `ParseUnreadFromTitle(string title)` which looks for any integer in the string and returns it (or returns 0 if none found). Maybe specifically look for patterns like "(%d)" or so.
     * Method 2: UI Automation. Use `AutomationElement.FromHandle(windowHandle)` to get the root element of WhatsApp.
       - Then try `FindFirst(TreeScope.Descendants, Condition)` to locate an element that might represent unread count. If we know WhatsApp Web’s structure, perhaps there's an element with name like "5 unread messages" or just "5".
       - We might attempt to find any element whose Name is a digit or contains "unread". For example, use a Condition that checks if NameProperty is not null and if it contains only digits or ends with "+".
       - If found, parse that to int (with '+' meaning >5).
     * If neither yields anything, set count = 0.
   - Compare with `CurrentUnreadCount`. If different, update `CurrentUnreadCount` and raise `UnreadCountChanged(newCount)`.
4. Integrate with TrayApplicationContext:
   - After WindowManager detects WhatsApp is running (e.g., on WhatsAppStarted event), start the UnreadCountDetector (initialize timer).
   - If WhatsApp exits, you can stop the detector’s timer to avoid unnecessary checks.
   - Subscribe to UnreadCountDetector.UnreadCountChanged event in TrayIconController. In handler, get the newCount and update the tray icon via IconManager (as done in previous steps): choose appropriate icon ("default" for 0, "msg-N" for 1-5, "msg-5plus" for >5). Also update `notifyIcon.Text` to include the count.
5. Write tests:
   - For parsing logic: test `ParseUnreadFromTitle` with various possible titles: "WhatsApp", "WhatsApp (2)", "My Chat - WhatsApp", "WhatsApp 5 messages", etc. Ensure it correctly extracts the number or returns 0.
   - For the detector logic, we can inject a fake data source. Approach: create an interface `IUnreadCountReader` with method `Read()` that returns an int. In production, implement it with the above logic (title + UIA). In tests, implement a FakeUnreadCountReader that returns preset values in sequence (simulate unread count changes).
   - Use the fake to test that the detector raises events correctly: e.g., set initial reader to return 0, then change it to 3, simulate timer tick calls (maybe call CheckUnreadCount() manually), verify event fired with 3, then if reader stays 3, no new event, then if goes to 6, event fired with 6 etc.
   - We can also simulate the integration: create a stub WindowManager that returns IsRunning true and provides a window handle. For UIA, testing might be too complex to simulate, so rely on title for tests.
   - Also test that when WhatsApp is not running, the detector either doesn’t run or reports 0.
6. Note: UIAutomation part might require adding a reference to UIAutomationClient. Also, running UIA on WhatsApp might need the Accessibility turned on. We implement it as best-effort; if it fails (e.g., throws exception because of security sandbox), catch and just use title.
   - We can test UIA logic on something like a dummy window with a known control name if desired, but it’s optional. Perhaps limit our UIA attempt to avoid hanging.
7. Run tests to ensure the detector logic works for simulated data. 
   - The actual UIA + title integration will be tested manually: Open WhatsApp with some unread chats, run tray manager, see if icon updates to the correct number. 
   - Also test scenario: if unread count goes down to 0 (e.g., user reads messages), the event triggers icon back to default.
**Best Practices:** Do not make UIA calls on the UI thread if they might be slow (System.Windows.Automation calls can be slow). Possibly run CheckUnreadCount in a background thread or use Task.Run. But since we're polling every 5s, doing it on a timer thread is fine. Just ensure any UI update (NotifyIcon) is marshalled to UI thread (maybe use `SynchronizationContext.Post` or a WinForms Timer instead for simplicity).
   - To keep simple, we might use a WinForms Timer so that Tick happens on UI thread and we can update NotifyIcon directly.
   - Document that OCR is omitted by design.

### Step 12: Update Tray Icon on UnreadCountChanged
**Goal:** Tie the unread count events to the tray icon’s icon and tooltip updates.
1. Ensure `TrayApplicationContext` (or TrayIconController) is subscribed to `unreadDetector.UnreadCountChanged`.
2. In the handler, implement:
   - int count = newCount (from event args).
   - Determine iconKey: if WindowManager.IsRunning is false, probably ignore (or set gray, but if WA not running we likely stopped detector anyway). If running:
     * If count == 0: iconKey = "default".
     * If 1 <= count <= 5: iconKey = $"msg-{count}".
     * If count > 5: iconKey = "msg-5plus".
   - Call `notifyIcon.Icon = iconManager.GetIcon(iconKey)`.
   - Update `notifyIcon.Text` (tooltip). We have to keep tooltip under 63 chars due to Windows limitation. Something like `WhatsApp - X unread messages` or if 0, `WhatsApp - No unread messages` (or just "WhatsApp" when 0).
3. Also, as a minor enhancement, if count > 0 and WhatsApp window is hidden, we could flash or accentuate the tray icon (blinking or different icon) – but not required by spec, skip.
4. Testing:
   - We already tested icon mapping logic with small counts in previous steps, but write a specific test: simulate UnreadCountChanged events and verify the tray chooses correct icon. Use a FakeIconManager that records requested key.
   - For example, fire event with 0 -> expect "default", with 4 -> "msg-4", with 7 -> "msg-5plus".
   - Also verify tooltip text changes (we can expose the NotifyIcon or its Text in a testable way, maybe via an internal property or wrapping NotifyIcon in an interface for testing).
   - Possibly create an `ITrayIcon` interface that our TrayApplicationContext uses, and in tests use a fake implementation that captures icon and text set.
   - If not, we might reflectively read `notifyIcon.Text` after event or subclass NotifyIcon in tests.
5. Ensure tests pass. 
   - If using an interface for NotifyIcon operations (to facilitate test), implement a real class that wraps actual NotifyIcon for production, and a fake for tests.
   - This adds complexity, but ensures full TDD. If time is short, at least test the logic mapping function by making it a separate method in TrayIconController that we can call with a count and get back a string for icon key and tooltip string.
6. After this, the unread count feature should be fully integrated. When running the app, one can test by sending themselves WhatsApp messages: the tray icon should update to show 1,2,... or 5+ as messages accumulate, and go back to default when all read.
**Best Practices:** Keep the UI updates minimal and on the UI thread. Avoid heavy string operations frequently (but updating tooltip every few seconds is fine). If using an interface for tray icon, ensure it's properly disposed and doesn’t break actual functionality.

### Step 13: “Start with Windows” Toggle Implementation
**Goal:** Make the "Start with Windows" context menu item interactive and persistent.
1. Now that we have SettingsHandler with registry control, tie it to the menu item:
   - In TrayApplicationContext, after loading settings in startup, set the "Start with Windows" menu item checked state = settings.StartWithWindows.
   - Attach an event handler for the menu item click (if not already). In the handler:
     * Invert the checked state (e.g., if was checked, uncheck it and vice versa).
     * Call SettingsHandler.ApplyAutoStart(newCheckedState) to update the registry immediately.
     * Update the Settings object in memory (settings.StartWithWindows = newCheckedState).
     * Call SettingsHandler.Save(settings) to write the config file with the new setting.
   - Possibly show a brief notification or tooltip that setting is saved (not necessary).
2. Write tests:
   - Use a FakeRegistryService (from Step 6) with SettingsHandler in the context of TrayApplicationContext for testing this toggle.
   - Simulate user clicking the "Start with Windows" menu:
     * Initially, ensure settings.StartWithWindows is false and menu.Checked = false.
     * Trigger the click handler. Then verify:
       - settings.StartWithWindows became true,
       - FakeRegistryService was instructed to add the Run key,
       - SettingsHandler.Save was called (we can make a FakeSettingsHandler or spy on it to ensure Save with updated value).
       - Menu.Checked is now true.
     * Simulate another click (toggle off) and verify inverse: registry remove called, config updated, etc.
   - Also test that on startup, if settings.StartWithWindows = true, the menu item is initialized checked.
3. Run tests. They should cover the logic thoroughly.
4. Manual test: toggle the setting in the tray menu and then exit the app; reopen the app and see if it reads the setting correctly; also check registry Run key manually to see if it was added/removed.
**Best Practices:** Avoid duplicating config state. The source of truth is the config file and registry; our in-memory `settings` should be updated in sync. Make sure to handle any exceptions (e.g., if registry write fails, perhaps show an error message using a BalloonTip). Keep the UI responsive by doing minimal work in the click handler (the operations are quick though).

### Step 14: Finishing Touches – Stability and Cleanup
**Goal:** Finalize resource cleanup and handle any edge cases.
1. **Resource Disposal:** Implement `TrayApplicationContext.Dispose(bool disposing)` override. Inside:
   - Dispose the NotifyIcon (so it’s removed from tray).
   - Dispose the context menu if needed.
   - Stop and dispose the UnreadCountDetector’s timer.
   - Perhaps kill the hook or re-enable WhatsApp close if our app is exiting (not strictly necessary, but polite: e.g., call GetSystemMenu(hWnd, true) to restore original menu).
   - Dispose or kill any remaining references. Also unsubscribe from events (WindowManager events, Process.Exited, etc.) to avoid memory leaks.
   - Call base.Dispose(disposing).
2. Ensure that selecting "Exit Tray Tool" from menu triggers disposal:
   - In the Exit menu handler, instead of Application.Exit() directly, we might call `this.ExitThread()` (which triggers ApplicationContext to end). .NET will call Dispose on ApplicationContext when exiting. Alternatively, keep Application.Exit(), which should end the message loop and then Dispose (since our context is ApplicationContext).
   - If Application.Exit is used, ensure that our ApplicationContext’s Dispose is still invoked (ApplicationContext should get disposed automatically on exit).
   - We might explicitly call notifyIcon.Dispose() in the Exit handler just to be sure it’s removed promptly.
3. **One-time notifications logic:** We already handle the "WhatsApp is not running" notification on start and when WA exits unexpectedly. Double-check the logic so that it doesn’t spam:
   - Use a bool field `notRunningNotified` that is set true after showing the balloon. 
   - When WA starts, reset `notRunningNotified = false` so that if WA stops again later, we can show it again.
   - Test this logic: simulate WA exits twice in a row without start in between – the second time, the flag would prevent second notification.
   - Simulate WA exits, then starts, then exits again – should notify again after restart.
4. **Multi-monitor restore check:** If possible, test manually on multi-monitor. Our use of GetWindowPlacement/SetWindowPlacement should handle it. No code change here, just note to testers.
5. **Config fallback:** Already handled (we create defaults if missing or corrupted).
6. **Final automated test pass:** Run all unit tests to ensure everything still passes after integrating all pieces.
7. **Manual test pass:** Run the application and go through the manual test cases from spec:
   - Start with WA closed: tray shows gray, one-time tooltip appears.
   - Launch WA: tray turns normal (default icon). Hide WA via X or tray click: WA stays running, window hidden.
   - Receive messages: tray icon updates number.
   - Click tray icon: WA toggles visibility.
   - Use "Show WhatsApp" from menu: brings WA back.
   - Use "Quit WhatsApp": WA closes, tray turns gray.
   - Use "Exit Tray Tool": tray app exits but leaves WA running if it was (make sure WA doesn’t also quit).
   - Toggle "Start with Windows": check registry and that next start reflects it.
   - Crash simulation: kill WA process externally, tray detects and goes gray, shows tooltip (only once).
8. All tests (automated and manual) should pass. At this point, prepare the code for final review or deployment.
**Best Practices:** Clearly comment any workaround we did (like disabling close button instead of global hook). Ensure naming consistency and remove any leftover debug logs. The code should be clean and ready for maintenance.