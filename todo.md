# WhatsApp Tray Manager Development Checklist

## Project Setup
- [x] Create a new .NET 6 WinForms project named `WhatsAppTrayManager` (Updated to .NET 8.0)
- [x] Create a test project named `WhatsAppTrayManager.Tests` (using xUnit or your preferred framework)
- [x] Configure project references so the test project can access the main project
- [x] Add a dummy test (e.g., `Assert.True(true)`) and verify tests run successfully

## Basic Tray ApplicationContext
- [x] Create `TrayApplicationContext` class inheriting from `ApplicationContext`
- [x] Initialize a `NotifyIcon` in the ApplicationContext constructor
  - [x] Set a temporary icon (e.g., `SystemIcons.Application`)
  - [x] Set `notifyIcon.Visible = true`
  - [x] Set tooltip text (e.g., "WhatsApp Tray Manager")
- [x] Create a context menu with an "Exit" option
  - [x] Wire the "Exit" menu item to call `Application.Exit()`
- [x] Update `Program.cs` to run the `TrayApplicationContext`
- [x] Write unit tests to verify the NotifyIcon is initialized and visible
- [x] Write tests (or simulate) that the Exit menu properly terminates the application

## Icon Manager Implementation
- [x] Create `IconManager` class with a method `GetIcon(string key)`
- [x] Define expected icon keys: "default", "gray", "msg-1", "msg-2", "msg-3", "msg-4", "msg-5", "msg-5plus"
- [x] Initially implement GetIcon as a stub returning a default icon
- [x] Modify TrayApplicationContext to use `IconManager.GetIcon("default")` for the tray icon
- [x] Write tests for `IconManager`:
  - [x] Verify `GetIcon("default")` returns a non-null Icon
  - [x] (Optionally) Test that an unknown key throws an exception or returns a fallback

## Generate Placeholder Icons
- [x] Implement a private method in `IconManager` to generate placeholder icons (`GenerateIcon(string key)`)
  - [x] For "default": create a green circle icon
  - [x] For "gray": create a gray circle icon
  - [x] For "msg-1" to "msg-5": create icons with the corresponding number over a base shape
  - [x] For "msg-5plus": create an icon with "5+" text
- [x] Cache generated icons in a dictionary to avoid regeneration
- [x] Optionally implement disposal logic for generated icons
- [x] Write tests to verify:
  - [x] Each key returns a non-null, distinct Icon
  - [x] Repeated calls for the same key return the same (cached) icon

## Configuration File Handling
- [x] Create a `Settings` class with properties:
  - [x] `StartWithWindows` (bool)
  - [x] `PollIntervalSeconds` (int)
- [x] Implement `SettingsHandler` with:
  - [x] `Load()` method to read `config.json` (create default settings if missing or malformed)
  - [x] `Save(Settings settings)` method to write settings to `config.json`
- [x] Integrate SettingsHandler in startup (read config in Program or ApplicationContext)
- [x] Write tests:
  - [x] Verify Load returns defaults when the file is missing
  - [x] Verify Save creates a valid config file and Load can read it back
  - [x] Verify that a malformed config results in default settings

## Auto-Start (Registry) Implementation
- [ ] In SettingsHandler, implement `ApplyAutoStart(bool enable)` method:
  - [ ] If `true`: add a registry key at `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` with name "WhatsAppTrayManager" and exe path
  - [ ] If `false`: remove the registry key if it exists
- [ ] Modify startup to call `ApplyAutoStart(settings.StartWithWindows)`
- [ ] Abstract registry operations behind an interface (e.g., `IRegistryService`) for testability
- [ ] Write tests using a fake registry service to verify correct behavior for enabling/disabling auto-start

## WhatsApp Window Manager – Process Detection
- [ ] Create a `WhatsAppWindowManager` class with properties:
  - [ ] `IsRunning` (bool)
  - [ ] `WhatsAppWindowHandle` (IntPtr)
  - [ ] A reference to the WhatsApp process (if applicable)
- [ ] Implement `FindWhatsApp()` method to:
  - [ ] Search for the WhatsApp process (using Process.GetProcessesByName or similar)
  - [ ] Set `IsRunning` based on detection and store the main window handle
  - [ ] Subscribe to the process’ `Exited` event
- [ ] Add events:
  - [ ] `WhatsAppStarted`
  - [ ] `WhatsAppExited`
- [ ] Integrate with TrayApplicationContext to:
  - [ ] Update tray icon (use "gray" if not running, "default" if running)
  - [ ] Show a one-time balloon tip "WhatsApp is not running" when appropriate
- [ ] Write tests using a fake process finder to simulate WhatsApp running/not running and verify event firing and icon state

## Tray Icon Interaction and Basic Window Toggle (Stubbed)
- [ ] In TrayApplicationContext or a dedicated TrayIconController:
  - [ ] Subscribe to WindowManager events (WhatsAppStarted, WhatsAppExited)
  - [ ] Update NotifyIcon icon based on WhatsApp running state (default vs. gray)
  - [ ] Implement left-click handler:
    - [ ] If WhatsApp is running and visible, call a stubbed `HideWindow()`
    - [ ] If WhatsApp is running and hidden, call a stubbed `ShowWindow()`
  - [ ] Wire up context menu items:
    - [ ] "Show WhatsApp" – triggers show action
    - [ ] "Quit WhatsApp" – triggers quitting WhatsApp (via WindowManager)
- [ ] Write tests:
  - [ ] Verify that left-click toggles between calling HideWindow and ShowWindow on a fake WindowManager
  - [ ] Verify that context menu commands call the correct methods on WindowManager

## Implement Show/Hide Window (Win32 Integration)
- [ ] In WhatsAppWindowManager, implement:
  - [ ] `HideWindow()`
    - [ ] Use P/Invoke (`ShowWindow` with `SW_HIDE`) to hide the window
    - [ ] Save the window’s current state/placement
    - [ ] Set a flag (e.g., `isHiddenToTray`)
  - [ ] `ShowWindow()` (or `RestoreWindow()`)
    - [ ] Use P/Invoke (`ShowWindow` with `SW_RESTORE` or `SW_SHOWMAXIMIZED`) to restore the window
    - [ ] Use `SetForegroundWindow` to bring it to focus
    - [ ] Reset the hidden flag
- [ ] Update tray icon handlers to call these real methods instead of stubs
- [ ] Write integration tests (if possible) or manual tests using a dummy window to verify hide/show behavior

## Prevent WhatsApp from Closing via X
- [ ] In WhatsAppWindowManager, implement a method to disable the close button on WhatsApp:
  - [ ] Use P/Invoke (`GetSystemMenu` and `RemoveMenu` with `SC_CLOSE`) to remove the close option
- [ ] Ensure that clicking the X on WhatsApp does not close the window (it remains running)
- [ ] For the "Quit WhatsApp" menu item, implement a method (e.g., `QuitWhatsApp()`) that forcefully closes WhatsApp (e.g., using Process.Kill)
- [ ] Write tests (as possible) to verify that the close button is disabled and that Quit triggers the proper method on a fake process

## Unread Message Detection
- [ ] Create `UnreadCountDetector` class with:
  - [ ] A timer to poll every `PollIntervalSeconds` (from settings)
  - [ ] A property `CurrentUnreadCount`
  - [ ] An event `UnreadCountChanged(int newCount)`
- [ ] Implement `CheckUnreadCount()` method:
  - [ ] Method 1: Read window title (via WindowManager or P/Invoke) and parse for unread count
  - [ ] Method 2: If needed, use UI Automation to find a text element with the unread count
  - [ ] (Skip OCR fallback for v1)
  - [ ] Update `CurrentUnreadCount` and raise event if changed
- [ ] Integrate UnreadCountDetector to:
  - [ ] Start when WhatsApp is running; stop if not
  - [ ] Subscribe in TrayIconController to update the tray icon accordingly
- [ ] Write tests:
  - [ ] Test parsing logic (simulate various window title formats)
  - [ ] Use a fake unread count source to simulate changes and verify event firing

## Update Tray Icon Based on Unread Count
- [ ] In the tray icon event handler for `UnreadCountChanged`:
  - [ ] Determine the correct icon key:
    - [ ] 0 unread → "default"
    - [ ] 1-5 unread → "msg-<n>"
    - [ ] >5 unread → "msg-5plus"
  - [ ] Update `notifyIcon.Icon` using IconManager
  - [ ] Update `notifyIcon.Text` (tooltip) with the current unread count
- [ ] Write tests to simulate unread count events and verify icon key and tooltip updates

## "Start with Windows" Menu Toggle
- [ ] In the tray context menu, implement the "Start with Windows" toggle:
  - [ ] Initialize the menu item's checked state based on `Settings.StartWithWindows`
  - [ ] On click, toggle the check state and update:
    - [ ] The in-memory Settings
    - [ ] The registry via `ApplyAutoStart(newState)`
    - [ ] Save the updated config using `SettingsHandler.Save()`
- [ ] Write tests using a fake registry service and settings handler to verify toggle behavior

## Final Integration & Cleanup
- [ ] Ensure all components are integrated: TrayApplicationContext, IconManager, WhatsAppWindowManager, UnreadCountDetector, and SettingsHandler
- [ ] Implement proper disposal in TrayApplicationContext:
  - [ ] Dispose NotifyIcon, timers, and unsubscribe from events
- [ ] Verify that "Exit Tray Tool" properly disposes resources and exits the app (without affecting WhatsApp if it’s still running)
- [ ] Perform manual testing for:
  - [ ] Starting the tool with and without WhatsApp running (verify icons and notifications)
  - [ ] Toggling show/hide of WhatsApp via tray icon and context menu
  - [ ] Unread message count changes and correct tray icon updates
  - [ ] "Quit WhatsApp" functionality and auto-start toggle via settings
  - [ ] Multi-monitor window restore (if possible)
  - [ ] Resource cleanup on exit

## Documentation & Final Review
- [ ] Comment code clearly (especially any P/Invoke or workarounds)
- [ ] Review naming conventions and structure for maintainability
- [ ] Finalize and commit the code with complete tests and documentation