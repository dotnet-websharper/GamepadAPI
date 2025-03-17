# WebSharper Gamepad API Binding

This repository provides an F# [WebSharper](https://websharper.com/) binding for the [Gamepad API](https://developer.mozilla.org/en-US/docs/Web/API/Gamepad_API), enabling WebSharper applications to detect and interact with game controllers.

## Repository Structure

The repository consists of two main projects:

1. **Binding Project**:

   - Contains the F# WebSharper binding for the Gamepad API.

2. **Sample Project**:
   - Demonstrates how to use the Gamepad API with WebSharper syntax.
   - Includes a GitHub Pages demo: [View Demo](https://dotnet-websharper.github.io/GamepadAPI/).

## Installation

To use this package in your WebSharper project, add the NuGet package:

```bash
   dotnet add package WebSharper.Gamepad
```

## Building

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) installed on your machine.

### Steps

1. Clone the repository:

   ```bash
   git clone https://github.com/dotnet-websharper/Gamepad.git
   cd Gamepad
   ```

2. Build the Binding Project:

   ```bash
   dotnet build WebSharper.Gamepad/WebSharper.Gamepad.fsproj
   ```

3. Build and Run the Sample Project:

   ```bash
   cd WebSharper.Gamepad.Sample
   dotnet build
   dotnet run
   ```

4. Open the hosted demo to see the Sample project in action:
   [https://dotnet-websharper.github.io/Gamepad/](https://dotnet-websharper.github.io/Gamepad/)

## Example Usage

Below is an example of how to use the Gamepad API in a WebSharper project:

```fsharp
namespace WebSharper.Gamepad.Sample

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Templating
open WebSharper.Gamepad

[<JavaScript>]
module Client =
    // The templates are loaded from the DOM, so you just can edit index.html
    // and refresh your browser, no need to recompile unless you add or remove holes.
    type IndexTemplate = Template<"wwwroot/index.html", ClientLoad.FromDocument>

    let statusMessage = Var.Create "Connect a gamepad and press any button"
    let buttonPressed = Var.Create "None"
    let leftStickX = Var.Create "0"
    let leftStickY = Var.Create "0"

    let mutable gamepadIndex: int option = None

    // Function to update gamepad inputs in real-time
    let rec updateGamepad () =
        match gamepadIndex with
        | Some index ->
            let navigator = As<Navigator>(JS.Window.Navigator)
            let gamepads = navigator.GetGamepads()
            let gamepad = gamepads.[index]

            if not (isNull gamepad) then
                // Detect which buttons are being pressed
                let buttons =
                    gamepad.Buttons
                    |> Array.mapi (fun i button -> if button.Pressed then Some (sprintf "Button %d" i) else None)
                    |> Array.choose id
                    |> fun arr -> if arr.Length > 0 then String.concat ", " arr else "None"

                buttonPressed.Value <- buttons
                leftStickX.Value <- gamepad.Axes.[0].ToString()
                leftStickY.Value <- gamepad.Axes.[1].ToString()

                // Recursively call updateGamepad on each animation frame
                JS.RequestAnimationFrame(fun _ -> updateGamepad()) |> ignore
        | None -> ()

    // Event handler for gamepad connection
    let handleGamepadConnected (event: Dom.Event) =
        let e = As<GamepadEvent> event
        gamepadIndex <- Some e.Gamepad.Index
        statusMessage.Value <- $"Gamepad Connected: {e.Gamepad.Id}"
        updateGamepad ()

    // Event handler for gamepad disconnection
    let handleGamepadDisconnected (_: Dom.Event) =
        statusMessage.Value <- "Gamepad Disconnected"
        gamepadIndex <- None

    // Fake gamepad controls using keyboard input
    let mutable fakeGamepad =
        {| Buttons = Array.init 16 (fun _ -> {| pressed = false |})
           Axes = [| 0.0; 0.0 |] |}

    let handleKeyDown (e: Dom.Event) =
        let keyEvent = As<Dom.KeyboardEvent> e
        match keyEvent.Key with
        | "ArrowUp" -> fakeGamepad.Axes.[1] <- -1.0
        | "ArrowDown" -> fakeGamepad.Axes.[1] <- 1.0
        | "ArrowLeft" -> fakeGamepad.Axes.[0] <- -1.0
        | "ArrowRight" -> fakeGamepad.Axes.[0] <- 1.0
        | " " -> fakeGamepad.Buttons.[0]?pressed <- true
        | _ -> ()

    let handleKeyUp (e: Dom.Event) =
        let keyEvent = As<Dom.KeyboardEvent> e
        match keyEvent.Key with
        | "ArrowUp" | "ArrowDown" -> fakeGamepad.Axes.[1] <- 0.0
        | "ArrowLeft" | "ArrowRight" -> fakeGamepad.Axes.[0] <- 0.0
        | " " -> fakeGamepad.Buttons.[0]?pressed <- false
        | _ -> ()

    // Function to update fake gamepad inputs using keyboard
    let rec updateFakeGamepad () =
        buttonPressed.Value <- if fakeGamepad.Buttons.[0]?pressed then "Button 0 (Spacebar)" else "None"
        leftStickX.Value <- fakeGamepad.Axes.[0].ToString()
        leftStickY.Value <- fakeGamepad.Axes.[1].ToString()
        JS.RequestAnimationFrame(fun _ -> updateFakeGamepad ()) |> ignore

    [<SPAEntryPoint>]
    let Main () =
        // Listen for gamepad connect and disconnect events
        JS.Window.AddEventListener("gamepadconnected", handleGamepadConnected)
        JS.Window.AddEventListener("gamepaddisconnected", handleGamepadDisconnected)
        JS.Window.AddEventListener("keydown", handleKeyDown)
        JS.Window.AddEventListener("keyup", handleKeyUp)

        updateFakeGamepad ()

        IndexTemplate.Main()
            .Status(statusMessage.V)
            .Buttons(buttonPressed.V)
            .LX(leftStickX.V)
            .LY(leftStickY.V)
            .Doc()
        |> Doc.RunById "main"
```

## Important Considerations

- **Browser Support**: The Gamepad API is supported in most modern browsers, but some implementations may vary.
- **Polling vs. Events**: Gamepad input is usually polled rather than event-driven, requiring regular updates.
- **Security Restrictions**: Some browsers require user interaction before accessing gamepad data.
