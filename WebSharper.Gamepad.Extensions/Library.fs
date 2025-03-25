namespace WebSharper.Gamepad

open WebSharper
open WebSharper.JavaScript

[<JavaScript; AutoOpen>]
module Extensions =

    type Navigator with
        [<Inline "$this.getGamepads()">]
        member this.GetGamepads() : Gamepad[] = X<Gamepad[]>
        
    type Window with
        [<Inline "$this.ongamepadconnected">]
        member this.OnGamepadConnected with get(): (Dom.Event -> unit) = ignore
        [<Inline "$this.ongamepadconnected = $callback">]
        member this.OnGamepadConnected with set(callback: Dom.Event -> unit) = ()

        [<Inline "$this.ongamepaddisconnected">]
        member this.OnGamepadDisconnected with get(): (Dom.Event -> unit) = ignore
        [<Inline "$this.ongamepaddisconnected = $callback">]
        member this.OnGamepadDisconnected with set(callback: Dom.Event -> unit) = ()
