namespace WebSharper.Gamepad

open WebSharper
open WebSharper.JavaScript
open WebSharper.InterfaceGenerator

module Definition =

    let GamepadButton =
        Class "GamepadButton"
        |+> Instance [
            "pressed" =? T<bool>
            "touched" =? T<bool>
            "value" =? T<float>
        ]

    let GamepadHapticEffectParams =
        Pattern.Config "GamepadHapticEffectParams" {
            Required = []
            Optional = [
                "duration", T<float> 
                "startDelay", T<float> 
                "strongMagnitude", T<float> 
                "weakMagnitude", T<float> 
                "leftTrigger", T<float> 
                "rightTrigger", T<float> 
            ]
        }

    let GamepadHapticActuator =
        Class "GamepadHapticActuator"
        |+> Instance [
            "effects" =? !|T<string>

            "playEffect" => T<string>?``type`` * GamepadHapticEffectParams?``params`` ^-> T<Promise<_>>[T<string>]
            "pulse" => T<float>?value * T<float>?duration ^-> T<Promise<_>>[T<bool>]
            "reset" => T<unit> ^-> T<Promise<_>>[T<string>]
        ]

    let GamepadPose =
        Class "GamepadPose"
        |+> Instance [
            "hasOrientation" =? T<bool> 
            "hasPosition" =? T<bool> 
            "position" =? !| T<float>
            "linearVelocity" =? !| T<float> 
            "linearAcceleration" =? !| T<float> 
            "orientation" =? !| T<float> 
            "angularVelocity" =? !| T<float>
            "angularAcceleration" =? !| T<float> 
        ]

    let Gamepad =
        Class "Gamepad"
        |+> Instance [
            "axes" =@ !| T<float>
            "buttons" =@ !| GamepadButton
            "connected" =@ T<bool>
            "id" =@ T<string>
            "index" =@ T<int>
            "mapping" =@ T<string>
            "timestamp" =@ T<float>

            "hand" =? T<string>
            "hapticActuators" =? !| GamepadHapticActuator
            "vibrationActuator" =? GamepadHapticActuator            
            "pose" =? GamepadPose            
        ]

    let GamepadEventInit = 
        Pattern.Config "GamepadEventInit" {
            Required = []
            Optional = [
                "gamepad", Gamepad.Type
            ]
        }

    let GamepadEvent =
        Class "GamepadEvent"
        |=> Inherits T<Dom.Event>
        |+> Instance [
            "gamepad" =? Gamepad 
        ]
        |+> Static [
            Constructor (T<string>?eventType * !? GamepadEventInit?options)
        ]

    let Navigator =
        Class "Navigator"
        |+> Instance [
            "getGamepads" => T<unit> ^-> !|Gamepad 
        ]

    let Window = 
        Class "Window"
        |+> Instance [
            "ongamepadconnected" => T<unit> ^-> T<unit> 
            |> ObsoleteWithMessage "Use OnGamepadConnected instead"
            "ongamepadconnected" => T<Dom.Event> ^-> T<unit> 
            |> WithSourceName "OnGamepadConnected"

            "ongamepaddisconnected" => T<unit> ^-> T<unit> 
            |> ObsoleteWithMessage "Use OnGamepadDisconnected instead"
            "ongamepaddisconnected" => T<Dom.Event> ^-> T<unit> 
            |> WithSourceName "OnGamepadDisconnected"
        ]

    let Assembly =
        Assembly [
            Namespace "WebSharper.Gamepad" [
                Window
                Navigator
                GamepadEvent
                GamepadEventInit
                Gamepad
                GamepadPose
                GamepadHapticActuator
                GamepadHapticEffectParams
                GamepadButton
            ]
        ]

[<Sealed>]
type Extension() =
    interface IExtension with
        member ext.Assembly =
            Definition.Assembly

[<assembly: Extension(typeof<Extension>)>]
do ()
