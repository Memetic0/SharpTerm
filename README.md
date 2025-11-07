# SharpTerm

A modern, cross-platform Terminal User Interface (TUI) library for .NET 9.

## Overview

SharpTerm provides a comprehensive framework for building interactive terminal applications with a rich widget system, event-driven architecture, and optimized rendering engine. The library leverages .NET 9 features and ANSI escape sequences to deliver high-performance, cross-platform terminal user interfaces.

## Features

**Core Capabilities**
- True 24-bit RGB color support
- Widget-based component architecture
- Event-driven application framework
- Flicker-free rendering with dirty tracking
- Terminal resize detection and responsive layouts
- Transparent widget backgrounds

**Widget Library**
- Label - Text display with alignment options
- Button - Interactive buttons with keyboard and mouse support
- Border - Decorative boxes with multiple styles
- ProgressBar - Visual progress indicators
- TextBox - Full-featured text input with editing
- List - Scrollable item selection with keyboard and mouse navigation

**Input Handling**
- Complete keyboard event processing
- Mouse support including clicks, double-clicks, and scroll wheel
- Windows Console API integration via ReadConsoleInput
- Tab-based focus navigation
- Configurable keyboard shortcuts

**Performance Optimizations**
- Per-widget dirty tracking eliminates unnecessary redraws
- Buffered output operations
- Smart rendering updates only changed regions
- Alternate screen buffer support

## Installation

See [QUICK_START.md](QUICK_START.md) for installation instructions and code examples.

## Documentation

- **[QUICK_START.md](QUICK_START.md)** - Installation and code examples
- **[IMPLEMENTATION_GUIDE.md](IMPLEMENTATION_GUIDE.md)** - Architecture and implementation details

## Sample Applications

The library includes five demonstration applications in `src/SharpTerm.Samples`:

1. **Hello World** - Basic terminal output with color support
2. **Widget Showcase** - Interactive demonstration of all widgets with resize support
3. **TODO App** - Task management with scrollable list, item activation, and detail view
4. **Progress Bar Demo** - Multiple animated progress indicators
5. **Form Input Demo** - Multi-field form with validation and keyboard navigation

Run samples:

```bash
dotnet run --project src/SharpTerm.Samples
```

## Platform Support

**Requirements**
- .NET 9.0 SDK or later
- Windows 10+ with Windows Terminal or ConHost
- Linux with modern terminal emulator supporting ANSI sequences
- macOS with Terminal.app or iTerm2

**Tested Environments**
- Windows 11 with Windows Terminal
- Windows Console Host (conhost.exe)
- Ubuntu 22.04 LTS with gnome-terminal
- macOS Monterey with iTerm2

## Architecture

**Terminal Abstraction**

The library provides a clean abstraction over terminal operations through the `ITerminalDriver` interface, with `AnsiTerminalDriver` implementing ANSI escape sequence rendering and Windows Console API input handling.

**Application Framework**

The `Application` class provides an event loop architecture with:
- Input event processing (keyboard and mouse)
- Widget lifecycle management
- Focus navigation and state management
- Render orchestration with dirty tracking
- Terminal resize event handling

**Widget System**

All widgets inherit from the base `Widget` class and implement the `Render` method. Widgets raise a `Changed` event when their state updates, triggering efficient re-renders of only modified components.

## Building from Source

Clone and build:

```bash
git clone https://github.com/yourusername/SharpTerm.git
cd SharpTerm
dotnet build
```

Run tests:

```bash
dotnet test
```

## Current Status

**Completed Features**
- Core terminal driver with ANSI support
- Six fully-featured widgets
- Application framework with event loop
- Windows Console API integration for input
- Per-widget dirty tracking rendering
- Mouse support (click, double-click, scroll wheel)
- Terminal resize detection
- Five sample applications
- Comprehensive test suite

**Planned Enhancements**
- Additional widgets (CheckBox, RadioButton, Panel, Menu, ScrollView)
- Layout management system (Flex, Grid)
- Theme and styling engine
- Data binding framework
- Cross-platform input handling improvements
- Async operation support
- Accessibility features

## Contributing

Contributions are welcome. Please submit pull requests with:
- Clear description of changes
- Unit tests for new functionality
- Documentation updates
- Code following existing style conventions

## License

MIT License - See LICENSE file for details

## Acknowledgments

Inspired by Terminal.Gui, Spectre.Console, and Blessed. Built with .NET 9 and C# 13.
