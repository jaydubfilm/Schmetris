// GENERATED AUTOMATICALLY FROM 'Assets/Input/SalvagerInput.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace StarSalvager.Utilities.Inputs
{
    public class @SalvagerInput : IInputActionCollection, IDisposable
    {
        public InputActionAsset asset { get; }
        public @SalvagerInput()
        {
            asset = InputActionAsset.FromJson(@"{
    ""name"": ""SalvagerInput"",
    ""maps"": [
        {
            ""name"": ""Default"",
            ""id"": ""797d13e9-b7f2-4d4f-9720-44578183acd4"",
            ""actions"": [
                {
                    ""name"": ""Side Movement"",
                    ""type"": ""Value"",
                    ""id"": ""5c466441-94f9-4d6e-b37d-7a2cfeddb5ee"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Rotate"",
                    ""type"": ""Button"",
                    ""id"": ""38e421be-ede4-4746-a637-d75216c445b7"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""Left Click"",
                    ""type"": ""Button"",
                    ""id"": ""482d4967-c4f1-4578-a107-58883fd0b134"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""Right Click"",
                    ""type"": ""Button"",
                    ""id"": ""e6882470-0e5e-4b15-81e6-e0268e091b1c"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""Pause"",
                    ""type"": ""Button"",
                    ""id"": ""ed19b199-88c4-47ad-99d1-e8570dcd382f"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""SmartAction1"",
                    ""type"": ""Button"",
                    ""id"": ""e2eb2b5c-b9b5-4e20-bc42-b583bd66256c"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""SmartAction2"",
                    ""type"": ""Button"",
                    ""id"": ""5e24e476-5e45-48f3-84ab-eda07748ce4a"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""SmartAction3"",
                    ""type"": ""Button"",
                    ""id"": ""13c775c0-f0d7-419f-a344-abde5b074c9d"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""SmartAction4"",
                    ""type"": ""Button"",
                    ""id"": ""5d09092d-457a-4720-9f18-b4b584fd9e21"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""Continue"",
                    ""type"": ""Button"",
                    ""id"": ""2d121d19-1f96-4c2a-9d4e-b4b432d26d0e"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""SelfDestruct"",
                    ""type"": ""Button"",
                    ""id"": ""d695bc48-5fac-42ac-9dcc-e49e6bcb524e"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""SpeedChange"",
                    ""type"": ""Value"",
                    ""id"": ""f6122248-cde6-4de5-a88f-144268190a85"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": ""Press""
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""AD"",
                    ""id"": ""c605e376-3012-4fda-a851-1f3c6b9565fd"",
                    ""path"": ""1DAxis"",
                    ""interactions"": ""Press(behavior=2)"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Side Movement"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""Negative"",
                    ""id"": ""da475843-ecbe-4efd-a322-8570837b1da2"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Side Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Positive"",
                    ""id"": ""bd20b2b4-c1c7-4108-8ccb-5f0b1bb650c3"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Side Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Arrows"",
                    ""id"": ""d9f5e109-d46a-411f-b4c8-b353b2d244da"",
                    ""path"": ""1DAxis"",
                    ""interactions"": ""Press(behavior=2)"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Side Movement"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""Negative"",
                    ""id"": ""534165d2-92c7-4b0b-82fd-482e9c50e3ab"",
                    ""path"": ""<Keyboard>/leftArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Side Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Positive"",
                    ""id"": ""64b310c6-b817-413e-aab9-5e4029411c5d"",
                    ""path"": ""<Keyboard>/rightArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Side Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""RightHand"",
                    ""id"": ""53558e0d-11c7-45bf-957c-a0f4c041f8a4"",
                    ""path"": ""1DAxis"",
                    ""interactions"": ""Press(behavior=2)"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Side Movement"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""Negative"",
                    ""id"": ""206a2499-5381-4ff7-849b-24c601012475"",
                    ""path"": ""<Keyboard>/comma"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Side Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Positive"",
                    ""id"": ""952d9430-6dab-4d9e-97b7-be6895f6c366"",
                    ""path"": ""<Keyboard>/slash"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Side Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""D-Pad"",
                    ""id"": ""1135902d-d7e1-4c58-81e6-27dcd1fec064"",
                    ""path"": ""1DAxis"",
                    ""interactions"": ""Press(behavior=2)"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Side Movement"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""Negative"",
                    ""id"": ""81e53de8-46cb-41cb-8619-7e7837959b9a"",
                    ""path"": ""<Gamepad>/dpad/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Side Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Positive"",
                    ""id"": ""94f163f2-aff0-47fb-9d38-84b08bcd58b8"",
                    ""path"": ""<Gamepad>/dpad/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Side Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""626d3d1b-580b-4cf2-b8a6-d8305cd616b2"",
                    ""path"": ""<Gamepad>/leftStick/x"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Side Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""WS"",
                    ""id"": ""459661cc-fd75-406a-a631-9109f238873f"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Rotate"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""9f532104-7c58-4287-8df5-9159e146cfb1"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Rotate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""2df4da74-6936-45c1-afd1-9251a201d14b"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Rotate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Arrows"",
                    ""id"": ""2596d261-f92e-4f46-90be-f27c84e113c5"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Rotate"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""49e03006-4a42-405b-8210-48e41197d8f3"",
                    ""path"": ""<Keyboard>/downArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Rotate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""db9226e5-b073-4578-8dad-b8ba9558290a"",
                    ""path"": ""<Keyboard>/upArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Rotate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""RightHand"",
                    ""id"": ""2478723c-69fa-4600-9f65-321e6a164736"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Rotate"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""5b6be539-a10b-4158-af09-9a9858a2fb33"",
                    ""path"": ""<Keyboard>/k"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Rotate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""d7851392-7d49-4c5c-8eec-343312cdf2fa"",
                    ""path"": ""<Keyboard>/semicolon"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Rotate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Controller"",
                    ""id"": ""eec1d48d-7c8f-4d8f-a2ef-c702a84a0be3"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Rotate"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""c7144637-5793-4411-b6d2-bb6d1140244d"",
                    ""path"": ""<Gamepad>/leftShoulder"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Rotate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""6a0341ce-58a4-43ba-a98b-67a2d1b4b9c3"",
                    ""path"": ""<Gamepad>/rightShoulder"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Rotate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""119459ad-6f1f-40d8-b072-93af4aee8a25"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Left Click"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""1ac23440-ae8b-41b1-9be0-b12f08afb6bf"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Right Click"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""52732cc6-3184-47e8-b094-608c0ef7ae73"",
                    ""path"": ""<Keyboard>/escape"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pause"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d541f7ab-000a-4249-8fee-45ac0fa6475a"",
                    ""path"": ""<Gamepad>/start"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pause"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""5ce964b7-0b1f-4db0-809a-fa6ba2389678"",
                    ""path"": ""<Keyboard>/1"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SmartAction1"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""6b1ba9bc-b466-44d1-9a15-89f064727662"",
                    ""path"": ""<Gamepad>/buttonWest"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SmartAction1"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""70947527-d913-4d8d-9e8e-f27b3aa38fb6"",
                    ""path"": ""<Keyboard>/2"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SmartAction2"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""55433e3b-dc06-4c82-ad9f-0a89ef36ac77"",
                    ""path"": ""<Gamepad>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SmartAction2"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""cdecaf24-137c-46af-9e5a-d7bcfc13c852"",
                    ""path"": ""<Keyboard>/3"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SmartAction3"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""f26edeed-e68a-4ce7-9db8-866a810b401a"",
                    ""path"": ""<Gamepad>/buttonNorth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SmartAction3"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""5579b4d4-5853-4c62-adfd-fcc6d1a87a06"",
                    ""path"": ""<Keyboard>/4"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SmartAction4"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""cca2e7d1-5007-4001-977d-3336d5c6c41e"",
                    ""path"": ""<Gamepad>/buttonEast"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SmartAction4"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""c0473a48-657d-427c-a202-de8357cf1539"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Continue"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d8bd2909-177f-4910-a084-f640d886dd37"",
                    ""path"": ""<Gamepad>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Continue"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""c83c9a9a-9a06-4de5-ba89-f4e0eae989b7"",
                    ""path"": ""<Keyboard>/x"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SelfDestruct"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""QE"",
                    ""id"": ""30c66add-9c1b-4b7b-a904-86673e2733c8"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SpeedChange"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""3ea6f926-b4dd-4001-8448-58c652579f1e"",
                    ""path"": ""<Keyboard>/q"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SpeedChange"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""1e9404c9-0ce9-4b1b-8575-0541cbb69e42"",
                    ""path"": ""<Keyboard>/e"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SpeedChange"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""888c49cb-81d8-45fc-86ff-6b5dc3e7a132"",
                    ""path"": ""<Gamepad>/dpad/y"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SpeedChange"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""Menu Controls"",
            ""id"": ""6fecb893-0e53-4cb0-bf3a-8f41b8cca48e"",
            ""actions"": [
                {
                    ""name"": ""Navigate"",
                    ""type"": ""Value"",
                    ""id"": ""b8fdaf2b-3ddd-4a13-aeb4-6573d99a2c2f"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Left Click"",
                    ""type"": ""Button"",
                    ""id"": ""957b3a89-8a62-453d-bfbf-a23955633829"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""Point"",
                    ""type"": ""Value"",
                    ""id"": ""60a7a036-e38f-4c29-9c97-2203dc8215c2"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Submit"",
                    ""type"": ""Button"",
                    ""id"": ""22d31fb4-3dd6-49a7-b269-2807962c8e11"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Cancel"",
                    ""type"": ""Button"",
                    ""id"": ""9da05bd6-0a21-49a6-88d9-cdf7ed83efbd"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Pause"",
                    ""type"": ""Button"",
                    ""id"": ""26a07a3d-ead1-40d5-a33f-2a5d02aadfc8"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""Scroll"",
                    ""type"": ""Value"",
                    ""id"": ""ad18de5f-5d0c-46ea-a7e6-978097cb036f"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""Right Click"",
                    ""type"": ""Button"",
                    ""id"": ""bf427530-d7d9-4cde-aa01-5e95d4450558"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""ad0de1ae-2860-449a-b8de-c6a40992847e"",
                    ""path"": ""<Gamepad>/dpad"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""WASD Keys"",
                    ""id"": ""d6b51d93-d112-4c7c-a1f2-1cf4dc9869ca"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Navigate"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""1f5bf865-3f18-49de-8f80-663938e81d50"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""78951841-613a-4a07-b727-0c363bb3b373"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""76e07777-6f44-412c-9ba3-000dcbdd741a"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""b2371b0b-142d-4886-9f4f-ce89b60468c6"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Arrow Keys"",
                    ""id"": ""f6cbd85a-5165-4c6b-bb99-0d43290c804f"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Navigate"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""8207a6a3-61af-458e-a8c5-6093bbd8d8ac"",
                    ""path"": ""<Keyboard>/upArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""a091a228-65c7-4f07-85eb-95a037f4b9d3"",
                    ""path"": ""<Keyboard>/downArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""9cf16ae5-19b6-438d-8659-ac9519c2d0af"",
                    ""path"": ""<Keyboard>/leftArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""ccd6801e-4088-487b-9a98-fd4efe358ff1"",
                    ""path"": ""<Keyboard>/rightArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""31fefdc1-88a3-4fff-b273-f95b568f75b4"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Left Click"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""955e5e09-1a9d-4539-aa87-03e1329181ad"",
                    ""path"": ""<Touchscreen>/press"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Left Click"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""3b11559c-3dc2-41cd-a3bd-5804c6ad7c22"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Point"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e0193ffa-6ae5-4703-ac15-14b252216e97"",
                    ""path"": ""<Touchscreen>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Point"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""621e8caa-173b-4505-be32-59fc0d985bb8"",
                    ""path"": ""*/{Submit}"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Submit"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d9154608-8fc3-4c9f-97a9-1cfd9f9987af"",
                    ""path"": ""<Gamepad>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Submit"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""5ca4f60e-ba11-4bdf-b080-e9d315691738"",
                    ""path"": ""*/{Cancel}"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Cancel"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a5701a6d-2b31-4e7a-a101-8ef61c877ded"",
                    ""path"": ""<Gamepad>/buttonEast"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Cancel"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""160e9409-2c47-4c39-ace1-f90c28aefef9"",
                    ""path"": ""<Keyboard>/escape"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pause"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""c01a7d74-1259-4e4f-93e1-b00d97736602"",
                    ""path"": ""<Gamepad>/start"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pause"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""45880ac0-7840-4400-9135-910251c4c870"",
                    ""path"": ""<Mouse>/scroll"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Scroll"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""41928269-e262-4d2c-aede-d62b0a50c16e"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Right Click"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
            // Default
            m_Default = asset.FindActionMap("Default", throwIfNotFound: true);
            m_Default_SideMovement = m_Default.FindAction("Side Movement", throwIfNotFound: true);
            m_Default_Rotate = m_Default.FindAction("Rotate", throwIfNotFound: true);
            m_Default_LeftClick = m_Default.FindAction("Left Click", throwIfNotFound: true);
            m_Default_RightClick = m_Default.FindAction("Right Click", throwIfNotFound: true);
            m_Default_Pause = m_Default.FindAction("Pause", throwIfNotFound: true);
            m_Default_SmartAction1 = m_Default.FindAction("SmartAction1", throwIfNotFound: true);
            m_Default_SmartAction2 = m_Default.FindAction("SmartAction2", throwIfNotFound: true);
            m_Default_SmartAction3 = m_Default.FindAction("SmartAction3", throwIfNotFound: true);
            m_Default_SmartAction4 = m_Default.FindAction("SmartAction4", throwIfNotFound: true);
            m_Default_Continue = m_Default.FindAction("Continue", throwIfNotFound: true);
            m_Default_SelfDestruct = m_Default.FindAction("SelfDestruct", throwIfNotFound: true);
            m_Default_SpeedChange = m_Default.FindAction("SpeedChange", throwIfNotFound: true);
            // Menu Controls
            m_MenuControls = asset.FindActionMap("Menu Controls", throwIfNotFound: true);
            m_MenuControls_Navigate = m_MenuControls.FindAction("Navigate", throwIfNotFound: true);
            m_MenuControls_LeftClick = m_MenuControls.FindAction("Left Click", throwIfNotFound: true);
            m_MenuControls_Point = m_MenuControls.FindAction("Point", throwIfNotFound: true);
            m_MenuControls_Submit = m_MenuControls.FindAction("Submit", throwIfNotFound: true);
            m_MenuControls_Cancel = m_MenuControls.FindAction("Cancel", throwIfNotFound: true);
            m_MenuControls_Pause = m_MenuControls.FindAction("Pause", throwIfNotFound: true);
            m_MenuControls_Scroll = m_MenuControls.FindAction("Scroll", throwIfNotFound: true);
            m_MenuControls_RightClick = m_MenuControls.FindAction("Right Click", throwIfNotFound: true);
        }

        public void Dispose()
        {
            UnityEngine.Object.Destroy(asset);
        }

        public InputBinding? bindingMask
        {
            get => asset.bindingMask;
            set => asset.bindingMask = value;
        }

        public ReadOnlyArray<InputDevice>? devices
        {
            get => asset.devices;
            set => asset.devices = value;
        }

        public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

        public bool Contains(InputAction action)
        {
            return asset.Contains(action);
        }

        public IEnumerator<InputAction> GetEnumerator()
        {
            return asset.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Enable()
        {
            asset.Enable();
        }

        public void Disable()
        {
            asset.Disable();
        }

        // Default
        private readonly InputActionMap m_Default;
        private IDefaultActions m_DefaultActionsCallbackInterface;
        private readonly InputAction m_Default_SideMovement;
        private readonly InputAction m_Default_Rotate;
        private readonly InputAction m_Default_LeftClick;
        private readonly InputAction m_Default_RightClick;
        private readonly InputAction m_Default_Pause;
        private readonly InputAction m_Default_SmartAction1;
        private readonly InputAction m_Default_SmartAction2;
        private readonly InputAction m_Default_SmartAction3;
        private readonly InputAction m_Default_SmartAction4;
        private readonly InputAction m_Default_Continue;
        private readonly InputAction m_Default_SelfDestruct;
        private readonly InputAction m_Default_SpeedChange;
        public struct DefaultActions
        {
            private @SalvagerInput m_Wrapper;
            public DefaultActions(@SalvagerInput wrapper) { m_Wrapper = wrapper; }
            public InputAction @SideMovement => m_Wrapper.m_Default_SideMovement;
            public InputAction @Rotate => m_Wrapper.m_Default_Rotate;
            public InputAction @LeftClick => m_Wrapper.m_Default_LeftClick;
            public InputAction @RightClick => m_Wrapper.m_Default_RightClick;
            public InputAction @Pause => m_Wrapper.m_Default_Pause;
            public InputAction @SmartAction1 => m_Wrapper.m_Default_SmartAction1;
            public InputAction @SmartAction2 => m_Wrapper.m_Default_SmartAction2;
            public InputAction @SmartAction3 => m_Wrapper.m_Default_SmartAction3;
            public InputAction @SmartAction4 => m_Wrapper.m_Default_SmartAction4;
            public InputAction @Continue => m_Wrapper.m_Default_Continue;
            public InputAction @SelfDestruct => m_Wrapper.m_Default_SelfDestruct;
            public InputAction @SpeedChange => m_Wrapper.m_Default_SpeedChange;
            public InputActionMap Get() { return m_Wrapper.m_Default; }
            public void Enable() { Get().Enable(); }
            public void Disable() { Get().Disable(); }
            public bool enabled => Get().enabled;
            public static implicit operator InputActionMap(DefaultActions set) { return set.Get(); }
            public void SetCallbacks(IDefaultActions instance)
            {
                if (m_Wrapper.m_DefaultActionsCallbackInterface != null)
                {
                    @SideMovement.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnSideMovement;
                    @SideMovement.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnSideMovement;
                    @SideMovement.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnSideMovement;
                    @Rotate.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnRotate;
                    @Rotate.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnRotate;
                    @Rotate.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnRotate;
                    @LeftClick.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnLeftClick;
                    @LeftClick.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnLeftClick;
                    @LeftClick.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnLeftClick;
                    @RightClick.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnRightClick;
                    @RightClick.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnRightClick;
                    @RightClick.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnRightClick;
                    @Pause.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnPause;
                    @Pause.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnPause;
                    @Pause.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnPause;
                    @SmartAction1.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnSmartAction1;
                    @SmartAction1.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnSmartAction1;
                    @SmartAction1.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnSmartAction1;
                    @SmartAction2.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnSmartAction2;
                    @SmartAction2.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnSmartAction2;
                    @SmartAction2.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnSmartAction2;
                    @SmartAction3.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnSmartAction3;
                    @SmartAction3.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnSmartAction3;
                    @SmartAction3.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnSmartAction3;
                    @SmartAction4.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnSmartAction4;
                    @SmartAction4.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnSmartAction4;
                    @SmartAction4.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnSmartAction4;
                    @Continue.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnContinue;
                    @Continue.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnContinue;
                    @Continue.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnContinue;
                    @SelfDestruct.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnSelfDestruct;
                    @SelfDestruct.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnSelfDestruct;
                    @SelfDestruct.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnSelfDestruct;
                    @SpeedChange.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnSpeedChange;
                    @SpeedChange.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnSpeedChange;
                    @SpeedChange.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnSpeedChange;
                }
                m_Wrapper.m_DefaultActionsCallbackInterface = instance;
                if (instance != null)
                {
                    @SideMovement.started += instance.OnSideMovement;
                    @SideMovement.performed += instance.OnSideMovement;
                    @SideMovement.canceled += instance.OnSideMovement;
                    @Rotate.started += instance.OnRotate;
                    @Rotate.performed += instance.OnRotate;
                    @Rotate.canceled += instance.OnRotate;
                    @LeftClick.started += instance.OnLeftClick;
                    @LeftClick.performed += instance.OnLeftClick;
                    @LeftClick.canceled += instance.OnLeftClick;
                    @RightClick.started += instance.OnRightClick;
                    @RightClick.performed += instance.OnRightClick;
                    @RightClick.canceled += instance.OnRightClick;
                    @Pause.started += instance.OnPause;
                    @Pause.performed += instance.OnPause;
                    @Pause.canceled += instance.OnPause;
                    @SmartAction1.started += instance.OnSmartAction1;
                    @SmartAction1.performed += instance.OnSmartAction1;
                    @SmartAction1.canceled += instance.OnSmartAction1;
                    @SmartAction2.started += instance.OnSmartAction2;
                    @SmartAction2.performed += instance.OnSmartAction2;
                    @SmartAction2.canceled += instance.OnSmartAction2;
                    @SmartAction3.started += instance.OnSmartAction3;
                    @SmartAction3.performed += instance.OnSmartAction3;
                    @SmartAction3.canceled += instance.OnSmartAction3;
                    @SmartAction4.started += instance.OnSmartAction4;
                    @SmartAction4.performed += instance.OnSmartAction4;
                    @SmartAction4.canceled += instance.OnSmartAction4;
                    @Continue.started += instance.OnContinue;
                    @Continue.performed += instance.OnContinue;
                    @Continue.canceled += instance.OnContinue;
                    @SelfDestruct.started += instance.OnSelfDestruct;
                    @SelfDestruct.performed += instance.OnSelfDestruct;
                    @SelfDestruct.canceled += instance.OnSelfDestruct;
                    @SpeedChange.started += instance.OnSpeedChange;
                    @SpeedChange.performed += instance.OnSpeedChange;
                    @SpeedChange.canceled += instance.OnSpeedChange;
                }
            }
        }
        public DefaultActions @Default => new DefaultActions(this);

        // Menu Controls
        private readonly InputActionMap m_MenuControls;
        private IMenuControlsActions m_MenuControlsActionsCallbackInterface;
        private readonly InputAction m_MenuControls_Navigate;
        private readonly InputAction m_MenuControls_LeftClick;
        private readonly InputAction m_MenuControls_Point;
        private readonly InputAction m_MenuControls_Submit;
        private readonly InputAction m_MenuControls_Cancel;
        private readonly InputAction m_MenuControls_Pause;
        private readonly InputAction m_MenuControls_Scroll;
        private readonly InputAction m_MenuControls_RightClick;
        public struct MenuControlsActions
        {
            private @SalvagerInput m_Wrapper;
            public MenuControlsActions(@SalvagerInput wrapper) { m_Wrapper = wrapper; }
            public InputAction @Navigate => m_Wrapper.m_MenuControls_Navigate;
            public InputAction @LeftClick => m_Wrapper.m_MenuControls_LeftClick;
            public InputAction @Point => m_Wrapper.m_MenuControls_Point;
            public InputAction @Submit => m_Wrapper.m_MenuControls_Submit;
            public InputAction @Cancel => m_Wrapper.m_MenuControls_Cancel;
            public InputAction @Pause => m_Wrapper.m_MenuControls_Pause;
            public InputAction @Scroll => m_Wrapper.m_MenuControls_Scroll;
            public InputAction @RightClick => m_Wrapper.m_MenuControls_RightClick;
            public InputActionMap Get() { return m_Wrapper.m_MenuControls; }
            public void Enable() { Get().Enable(); }
            public void Disable() { Get().Disable(); }
            public bool enabled => Get().enabled;
            public static implicit operator InputActionMap(MenuControlsActions set) { return set.Get(); }
            public void SetCallbacks(IMenuControlsActions instance)
            {
                if (m_Wrapper.m_MenuControlsActionsCallbackInterface != null)
                {
                    @Navigate.started -= m_Wrapper.m_MenuControlsActionsCallbackInterface.OnNavigate;
                    @Navigate.performed -= m_Wrapper.m_MenuControlsActionsCallbackInterface.OnNavigate;
                    @Navigate.canceled -= m_Wrapper.m_MenuControlsActionsCallbackInterface.OnNavigate;
                    @LeftClick.started -= m_Wrapper.m_MenuControlsActionsCallbackInterface.OnLeftClick;
                    @LeftClick.performed -= m_Wrapper.m_MenuControlsActionsCallbackInterface.OnLeftClick;
                    @LeftClick.canceled -= m_Wrapper.m_MenuControlsActionsCallbackInterface.OnLeftClick;
                    @Point.started -= m_Wrapper.m_MenuControlsActionsCallbackInterface.OnPoint;
                    @Point.performed -= m_Wrapper.m_MenuControlsActionsCallbackInterface.OnPoint;
                    @Point.canceled -= m_Wrapper.m_MenuControlsActionsCallbackInterface.OnPoint;
                    @Submit.started -= m_Wrapper.m_MenuControlsActionsCallbackInterface.OnSubmit;
                    @Submit.performed -= m_Wrapper.m_MenuControlsActionsCallbackInterface.OnSubmit;
                    @Submit.canceled -= m_Wrapper.m_MenuControlsActionsCallbackInterface.OnSubmit;
                    @Cancel.started -= m_Wrapper.m_MenuControlsActionsCallbackInterface.OnCancel;
                    @Cancel.performed -= m_Wrapper.m_MenuControlsActionsCallbackInterface.OnCancel;
                    @Cancel.canceled -= m_Wrapper.m_MenuControlsActionsCallbackInterface.OnCancel;
                    @Pause.started -= m_Wrapper.m_MenuControlsActionsCallbackInterface.OnPause;
                    @Pause.performed -= m_Wrapper.m_MenuControlsActionsCallbackInterface.OnPause;
                    @Pause.canceled -= m_Wrapper.m_MenuControlsActionsCallbackInterface.OnPause;
                    @Scroll.started -= m_Wrapper.m_MenuControlsActionsCallbackInterface.OnScroll;
                    @Scroll.performed -= m_Wrapper.m_MenuControlsActionsCallbackInterface.OnScroll;
                    @Scroll.canceled -= m_Wrapper.m_MenuControlsActionsCallbackInterface.OnScroll;
                    @RightClick.started -= m_Wrapper.m_MenuControlsActionsCallbackInterface.OnRightClick;
                    @RightClick.performed -= m_Wrapper.m_MenuControlsActionsCallbackInterface.OnRightClick;
                    @RightClick.canceled -= m_Wrapper.m_MenuControlsActionsCallbackInterface.OnRightClick;
                }
                m_Wrapper.m_MenuControlsActionsCallbackInterface = instance;
                if (instance != null)
                {
                    @Navigate.started += instance.OnNavigate;
                    @Navigate.performed += instance.OnNavigate;
                    @Navigate.canceled += instance.OnNavigate;
                    @LeftClick.started += instance.OnLeftClick;
                    @LeftClick.performed += instance.OnLeftClick;
                    @LeftClick.canceled += instance.OnLeftClick;
                    @Point.started += instance.OnPoint;
                    @Point.performed += instance.OnPoint;
                    @Point.canceled += instance.OnPoint;
                    @Submit.started += instance.OnSubmit;
                    @Submit.performed += instance.OnSubmit;
                    @Submit.canceled += instance.OnSubmit;
                    @Cancel.started += instance.OnCancel;
                    @Cancel.performed += instance.OnCancel;
                    @Cancel.canceled += instance.OnCancel;
                    @Pause.started += instance.OnPause;
                    @Pause.performed += instance.OnPause;
                    @Pause.canceled += instance.OnPause;
                    @Scroll.started += instance.OnScroll;
                    @Scroll.performed += instance.OnScroll;
                    @Scroll.canceled += instance.OnScroll;
                    @RightClick.started += instance.OnRightClick;
                    @RightClick.performed += instance.OnRightClick;
                    @RightClick.canceled += instance.OnRightClick;
                }
            }
        }
        public MenuControlsActions @MenuControls => new MenuControlsActions(this);
        public interface IDefaultActions
        {
            void OnSideMovement(InputAction.CallbackContext context);
            void OnRotate(InputAction.CallbackContext context);
            void OnLeftClick(InputAction.CallbackContext context);
            void OnRightClick(InputAction.CallbackContext context);
            void OnPause(InputAction.CallbackContext context);
            void OnSmartAction1(InputAction.CallbackContext context);
            void OnSmartAction2(InputAction.CallbackContext context);
            void OnSmartAction3(InputAction.CallbackContext context);
            void OnSmartAction4(InputAction.CallbackContext context);
            void OnContinue(InputAction.CallbackContext context);
            void OnSelfDestruct(InputAction.CallbackContext context);
            void OnSpeedChange(InputAction.CallbackContext context);
        }
        public interface IMenuControlsActions
        {
            void OnNavigate(InputAction.CallbackContext context);
            void OnLeftClick(InputAction.CallbackContext context);
            void OnPoint(InputAction.CallbackContext context);
            void OnSubmit(InputAction.CallbackContext context);
            void OnCancel(InputAction.CallbackContext context);
            void OnPause(InputAction.CallbackContext context);
            void OnScroll(InputAction.CallbackContext context);
            void OnRightClick(InputAction.CallbackContext context);
        }
    }
}
