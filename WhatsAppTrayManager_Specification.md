
# WhatsApp Tray Manager – Full Developer Specification

## 🧭 Purpose
A lightweight Windows 11 utility that enhances the WhatsApp Desktop experience by managing its minimize-to-tray behavior, auto-start configuration, unread message monitoring, and consistent tray presence — even when WhatsApp itself does not natively support these features.

---

## 🛠 Technology Stack

- **Language:** C# (preferably .NET 6+)
- **UI Framework:** Windows Forms
- **Interop APIs:** 
  - `user32.dll` – for window control
  - `System.Windows.Forms` – for tray icon
  - `System.Diagnostics` – for process detection
  - `UIAutomationClient` – for reading UI elements
  - `Microsoft.Win32` – for registry operations
- **External Libraries (Optional):**
  - Tesseract OCR via wrapper (as fallback)
- **Build Target:** Windows 11 x64

---

## 📦 Features

### 🔹 Tray Icon Management
- Always visible tray icon.
- Left-click restores or hides WhatsApp.
- Right-click menu:
  - **Show WhatsApp**
  - **Quit WhatsApp**
  - **Start with Windows** (toggle)
  - **Exit Tray Tool**
- Tray icon dynamically changes based on:
  - Unread message count (1–5+)
  - WhatsApp running state (normal vs. grayed-out icon)

### 🔹 WhatsApp Window Handling
- Detect and attach to running WhatsApp instance (do not launch).
- Intercept close (`WM_CLOSE`) to hide window and remove from taskbar.
- Track last window state and monitor.
- On restore, apply saved position/state.

### 🔹 Unread Message Detection
- Polling-based detection (every 5 seconds).
- Uses best-effort methods:
  - Window title scraping
  - UI Automation (if accessible)
  - OCR fallback (optional and slow; avoid unless necessary)
- Changes tray icon based on:
  - 0 unread = base icon
  - 1–5 unread = numbered icons
  - >5 unread = “5+” icon

### 🔹 Startup Behavior
- Starts minimized to tray.
- Reads configuration from `config.json` next to `.exe`:
```json
{
  "start_with_windows": true,
  "poll_interval_seconds": 5
}
```
- Auto-start handled via:
  - Writing to `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`

### 🔹 Error Handling & Status Feedback
- If WhatsApp is not found:
  - Tray icon turns gray
  - One-time balloon tooltip: “WhatsApp is not running”
- Continues running and polling silently until WhatsApp is started.
- No repeated alerts unless WhatsApp status changes.

---

## 🧩 Architecture Overview

### Components:
1. **Tray Icon Controller**
   - Displays and updates system tray icon
   - Handles left/right click events
   - Manages the context menu and config state

2. **WhatsApp Window Manager**
   - Detects running instance of WhatsApp
   - Handles show, hide, restore, and minimize logic
   - Intercepts X button via `SetWindowsHookEx` or window subclassing

3. **Unread Count Detector**
   - Uses polling to detect unread messages
   - Communicates with the tray controller to update icon

4. **Settings Handler**
   - Reads/writes `config.json`
   - Manages registry keys for auto-start

5. **Icon Manager**
   - Loads pre-rendered `.ico` files
   - Maps unread counts to icons

---

## 💾 Data Handling

### Configuration
- File: `config.json`
- Location: Same directory as executable
- Contents:
  - `start_with_windows`: bool
  - `poll_interval_seconds`: int

### Registry
- Key: `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`
- Value: `"WhatsAppTrayManager"` = path to `.exe`

### Icons
- 8 icons total:
  - `default.ico`, `gray.ico`, `msg-1.ico` ... `msg-5.ico`, `msg-5plus.ico`

---

## 🧪 Testing Plan

### Manual Test Cases
| Scenario                                | Expected Result                                    |
|-----------------------------------------|----------------------------------------------------|
| Start tool with WhatsApp running        | Tray shows icon; reflects unread count             |
| Start tool without WhatsApp             | Gray icon, tooltip warning                         |
| Click X on WhatsApp                     | WhatsApp hides; removed from taskbar               |
| Left-click tray icon                    | WhatsApp toggles visibility                        |
| Right-click → Show WhatsApp             | WhatsApp restores on correct monitor               |
| Right-click → Quit WhatsApp             | WhatsApp closes completely                         |
| Right-click → Exit Tray Tool            | Tray app exits, WhatsApp continues (if open)       |
| Enable/disable “Start with Windows”     | Updates config and registry correctly              |
| Receive 1–5+ messages                   | Tray icon updates to matching unread badge         |
| WhatsApp crashes/exits unexpectedly     | Tray switches to gray; no repeated alerts          |

### Automated/Edge Testing (Optional)
- Memory and CPU monitoring over 24h run.
- Multi-monitor behavior with different DPI.
- Config corruption fallback (e.g. missing or bad JSON).

---

## 📌 Future Enhancements (Not Required for v1)
- Global hotkey to toggle WhatsApp (e.g. Ctrl+Shift+W)
- Settings window for GUI config editing
- Localization/multi-language support
- Auto-update from GitHub

---

## 🧷 Summary

This tool enhances the WhatsApp Desktop experience by enabling reliable, native-feeling tray behavior. It makes WhatsApp behave like a modern Windows-native messaging app with persistent background operation and message awareness.

