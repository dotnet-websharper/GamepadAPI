namespace WebSharper.Gamepad.Sample

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Notation
open WebSharper.UI.Client
open WebSharper.UI.Templating
open WebSharper.Gamepad

[<JavaScript>]
module Client =
    // The template is loaded from the DOM, so editing index.html and refreshing
    // the browser is enough; a rebuild is only needed when holes change.
    type IndexTemplate = Template<"wwwroot/index.html", ClientLoad.FromDocument>

    let status = Var.Create "Connect a gamepad, or use the arrow keys and spacebar to simulate one."
    let buttons = Var.Create "None"
    let leftX = Var.Create "0.00"
    let leftY = Var.Create "0.00"
    let triggerL = Var.Create "0.00"
    let triggerR = Var.Create "0.00"

    let mutable gamepadIndex : int option = None

    // Keyboard fallback so the demo works without a physical gamepad.
    let fakeButtons = Array.create 16 false
    let fakeAxes = [| 0.0; 0.0 |]

    let buttonName i = sprintf "Button %d" i

    let rec updateGamepad () =
        let pads = JS.Window.Navigator.GetGamepads()
        match gamepadIndex with
        | Some i when i >= 0 && i < pads.Length && not (isNull pads.[i]) ->
            let gp = pads.[i]
            let pressed =
                gp.Buttons
                |> Array.mapi (fun i b -> if b.Pressed then Some (buttonName i) else None)
                |> Array.choose id
            buttons := if pressed.Length > 0 then String.concat ", " pressed else "None"
            leftX := gp.Axes.[0].ToString("0.00")
            leftY := gp.Axes.[1].ToString("0.00")
            triggerL := if gp.Buttons.Length > 6 then gp.Buttons.[6].Value.ToString("0.00") else "0.00"
            triggerR := if gp.Buttons.Length > 7 then gp.Buttons.[7].Value.ToString("0.00") else "0.00"
        | _ ->
            buttons := if fakeButtons.[0] then "Button 0 (Spacebar)" else "None"
            leftX := fakeAxes.[0].ToString("0.00")
            leftY := fakeAxes.[1].ToString("0.00")
            triggerL := "0.00"
            triggerR := "0.00"

        // Gamepad input is polled, so keep the readout in sync on every frame.
        JS.RequestAnimationFrame(fun _ -> updateGamepad ()) |> ignore

    let handleGamepadConnected (event: Dom.Event) =
        let e = As<GamepadEvent> event
        gamepadIndex <- Some e.Gamepad.Index
        status := $"Connected: {e.Gamepad.Id}"

    let handleGamepadDisconnected (event: Dom.Event) =
        let e = As<GamepadEvent> event
        if gamepadIndex = Some e.Gamepad.Index then
            gamepadIndex <- None
        status := "Disconnected. Connect a gamepad or use the keyboard."

    let handleKeyDown (event: Dom.Event) =
        let key = As<Dom.KeyboardEvent> event
        match key.Key with
        | "ArrowUp" -> fakeAxes.[1] <- -1.0
        | "ArrowDown" -> fakeAxes.[1] <- 1.0
        | "ArrowLeft" -> fakeAxes.[0] <- -1.0
        | "ArrowRight" -> fakeAxes.[0] <- 1.0
        | " " -> fakeButtons.[0] <- true
        | _ -> ()

    let handleKeyUp (event: Dom.Event) =
        let key = As<Dom.KeyboardEvent> event
        match key.Key with
        | "ArrowUp" | "ArrowDown" -> fakeAxes.[1] <- 0.0
        | "ArrowLeft" | "ArrowRight" -> fakeAxes.[0] <- 0.0
        | " " -> fakeButtons.[0] <- false
        | _ -> ()

    [<SPAEntryPoint>]
    let Main () =
        JS.Window.AddEventListener("gamepadconnected", handleGamepadConnected)
        JS.Window.AddEventListener("gamepaddisconnected", handleGamepadDisconnected)
        JS.Window.AddEventListener("keydown", handleKeyDown)
        JS.Window.AddEventListener("keyup", handleKeyUp)

        updateGamepad ()

        IndexTemplate()
            .status(status.V)
            .buttons(buttons.V)
            .leftX(leftX.V)
            .leftY(leftY.V)
            .triggerL(triggerL.V)
            .triggerR(triggerR.V)
            .Bind()
