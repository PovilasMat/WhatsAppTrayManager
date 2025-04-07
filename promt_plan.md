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
