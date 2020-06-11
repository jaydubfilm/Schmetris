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
                    ""expectedControlType"": ""Digital"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""Rotate"",
                    ""type"": ""Button"",
                    ""id"": ""38e421be-ede4-4746-a637-d75216c445b7"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""AD"",
                    ""id"": ""c605e376-3012-4fda-a851-1f3c6b9565fd"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Side Movement"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""1105738b-ca00-413b-a670-0d9f4b053a70"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Side Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""402ec1ce-0f0a-4831-ae34-4ee33df7c50b"",
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
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Side Movement"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""93c05d85-d9f0-4f4a-8f1a-b4a42a125793"",
                    ""path"": ""<Keyboard>/leftArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Side Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""e570be30-c752-4bd6-a80f-99988102bb3f"",
                    ""path"": ""<Keyboard>/rightArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Side Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
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
                }
            ]
        },
        {
            ""name"": ""Prototyping"",
            ""id"": ""3d672be6-1e0f-4c73-94c3-1a6d0e584ea5"",
            ""actions"": [
                {
                    ""name"": ""Left"",
                    ""type"": ""Button"",
                    ""id"": ""fb9224f0-4fcf-42c4-9957-5d74cdc6329e"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Right"",
                    ""type"": ""Button"",
                    ""id"": ""d6c8bc59-17c9-466e-9928-e34192fba159"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Up"",
                    ""type"": ""Button"",
                    ""id"": ""99300332-5dbe-4b3b-b0be-3e695547a95c"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Down"",
                    ""type"": ""Button"",
                    ""id"": ""bcfadd7c-e759-491c-ae43-d84dbd09e4ff"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Import"",
                    ""type"": ""Button"",
                    ""id"": ""12710870-f5dd-4691-9db9-e12f61213aef"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Export"",
                    ""type"": ""Button"",
                    ""id"": ""8012499f-2889-404d-bb39-c9de4f63afad"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""Button With One Modifier"",
                    ""id"": ""a89f2350-70f9-4e7a-9ab4-70fb22b65299"",
                    ""path"": ""ButtonWithOneModifier"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Left"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""modifier"",
                    ""id"": ""f1eeb715-a6c0-44bc-a63a-04fceca99e27"",
                    ""path"": ""<Keyboard>/leftAlt"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Left"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""button"",
                    ""id"": ""ccee3d84-0f96-4a5b-8bae-41c49d689e94"",
                    ""path"": ""<Keyboard>/leftArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Left"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Button With One Modifier"",
                    ""id"": ""f6125263-be48-4ac7-a6ae-bb2b90af5527"",
                    ""path"": ""ButtonWithOneModifier"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Right"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""modifier"",
                    ""id"": ""5938f9c7-ea23-44d7-a2df-ab2450c8f90b"",
                    ""path"": ""<Keyboard>/leftAlt"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Right"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""button"",
                    ""id"": ""d31b49a0-776b-4894-a329-66076e3059e2"",
                    ""path"": ""<Keyboard>/rightArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Right"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Button With One Modifier"",
                    ""id"": ""a9938cdc-ed4b-49dd-ab14-18d627f0a4c8"",
                    ""path"": ""ButtonWithOneModifier"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Up"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""modifier"",
                    ""id"": ""94f798df-5d60-48b2-8349-47adcdd7e0c2"",
                    ""path"": ""<Keyboard>/leftAlt"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Up"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""button"",
                    ""id"": ""bf0bf2d5-f0ad-42fb-b6d3-92a3fd205374"",
                    ""path"": ""<Keyboard>/upArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Up"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Button With One Modifier"",
                    ""id"": ""a57d0e9e-8a05-4457-bb53-c8774a0de7eb"",
                    ""path"": ""ButtonWithOneModifier"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Down"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""modifier"",
                    ""id"": ""4aabd0aa-a688-471c-9096-41e08e9dc2ab"",
                    ""path"": ""<Keyboard>/leftAlt"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Down"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""button"",
                    ""id"": ""9d702096-be1c-4dfd-9a3e-4db9d790def0"",
                    ""path"": ""<Keyboard>/downArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Down"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Button With One Modifier"",
                    ""id"": ""c5c6dbcb-3ff8-4c7f-83ff-1d1789818c94"",
                    ""path"": ""ButtonWithOneModifier"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Import"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""modifier"",
                    ""id"": ""2bd820fb-5375-4243-85dc-0a70e8003520"",
                    ""path"": ""<Keyboard>/leftAlt"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Import"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""button"",
                    ""id"": ""15838e5b-02eb-406c-9875-527c1b2ec346"",
                    ""path"": ""<Keyboard>/i"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Import"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Button With One Modifier"",
                    ""id"": ""0f12f8ef-225e-4dc7-ba03-3504f5eadd1b"",
                    ""path"": ""ButtonWithOneModifier"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Export"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""modifier"",
                    ""id"": ""3feb0394-c865-4915-8c37-e4566e6f88bc"",
                    ""path"": ""<Keyboard>/leftAlt"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Export"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""button"",
                    ""id"": ""14c6ae52-8454-43cb-a71d-4a77240122eb"",
                    ""path"": ""<Keyboard>/e"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Export"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
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
            // Prototyping
            m_Prototyping = asset.FindActionMap("Prototyping", throwIfNotFound: true);
            m_Prototyping_Left = m_Prototyping.FindAction("Left", throwIfNotFound: true);
            m_Prototyping_Right = m_Prototyping.FindAction("Right", throwIfNotFound: true);
            m_Prototyping_Up = m_Prototyping.FindAction("Up", throwIfNotFound: true);
            m_Prototyping_Down = m_Prototyping.FindAction("Down", throwIfNotFound: true);
            m_Prototyping_Import = m_Prototyping.FindAction("Import", throwIfNotFound: true);
            m_Prototyping_Export = m_Prototyping.FindAction("Export", throwIfNotFound: true);
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
        public struct DefaultActions
        {
            private @SalvagerInput m_Wrapper;
            public DefaultActions(@SalvagerInput wrapper) { m_Wrapper = wrapper; }
            public InputAction @SideMovement => m_Wrapper.m_Default_SideMovement;
            public InputAction @Rotate => m_Wrapper.m_Default_Rotate;
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
                }
            }
        }
        public DefaultActions @Default => new DefaultActions(this);

        // Prototyping
        private readonly InputActionMap m_Prototyping;
        private IPrototypingActions m_PrototypingActionsCallbackInterface;
        private readonly InputAction m_Prototyping_Left;
        private readonly InputAction m_Prototyping_Right;
        private readonly InputAction m_Prototyping_Up;
        private readonly InputAction m_Prototyping_Down;
        private readonly InputAction m_Prototyping_Import;
        private readonly InputAction m_Prototyping_Export;
        public struct PrototypingActions
        {
            private @SalvagerInput m_Wrapper;
            public PrototypingActions(@SalvagerInput wrapper) { m_Wrapper = wrapper; }
            public InputAction @Left => m_Wrapper.m_Prototyping_Left;
            public InputAction @Right => m_Wrapper.m_Prototyping_Right;
            public InputAction @Up => m_Wrapper.m_Prototyping_Up;
            public InputAction @Down => m_Wrapper.m_Prototyping_Down;
            public InputAction @Import => m_Wrapper.m_Prototyping_Import;
            public InputAction @Export => m_Wrapper.m_Prototyping_Export;
            public InputActionMap Get() { return m_Wrapper.m_Prototyping; }
            public void Enable() { Get().Enable(); }
            public void Disable() { Get().Disable(); }
            public bool enabled => Get().enabled;
            public static implicit operator InputActionMap(PrototypingActions set) { return set.Get(); }
            public void SetCallbacks(IPrototypingActions instance)
            {
                if (m_Wrapper.m_PrototypingActionsCallbackInterface != null)
                {
                    @Left.started -= m_Wrapper.m_PrototypingActionsCallbackInterface.OnLeft;
                    @Left.performed -= m_Wrapper.m_PrototypingActionsCallbackInterface.OnLeft;
                    @Left.canceled -= m_Wrapper.m_PrototypingActionsCallbackInterface.OnLeft;
                    @Right.started -= m_Wrapper.m_PrototypingActionsCallbackInterface.OnRight;
                    @Right.performed -= m_Wrapper.m_PrototypingActionsCallbackInterface.OnRight;
                    @Right.canceled -= m_Wrapper.m_PrototypingActionsCallbackInterface.OnRight;
                    @Up.started -= m_Wrapper.m_PrototypingActionsCallbackInterface.OnUp;
                    @Up.performed -= m_Wrapper.m_PrototypingActionsCallbackInterface.OnUp;
                    @Up.canceled -= m_Wrapper.m_PrototypingActionsCallbackInterface.OnUp;
                    @Down.started -= m_Wrapper.m_PrototypingActionsCallbackInterface.OnDown;
                    @Down.performed -= m_Wrapper.m_PrototypingActionsCallbackInterface.OnDown;
                    @Down.canceled -= m_Wrapper.m_PrototypingActionsCallbackInterface.OnDown;
                    @Import.started -= m_Wrapper.m_PrototypingActionsCallbackInterface.OnImport;
                    @Import.performed -= m_Wrapper.m_PrototypingActionsCallbackInterface.OnImport;
                    @Import.canceled -= m_Wrapper.m_PrototypingActionsCallbackInterface.OnImport;
                    @Export.started -= m_Wrapper.m_PrototypingActionsCallbackInterface.OnExport;
                    @Export.performed -= m_Wrapper.m_PrototypingActionsCallbackInterface.OnExport;
                    @Export.canceled -= m_Wrapper.m_PrototypingActionsCallbackInterface.OnExport;
                }
                m_Wrapper.m_PrototypingActionsCallbackInterface = instance;
                if (instance != null)
                {
                    @Left.started += instance.OnLeft;
                    @Left.performed += instance.OnLeft;
                    @Left.canceled += instance.OnLeft;
                    @Right.started += instance.OnRight;
                    @Right.performed += instance.OnRight;
                    @Right.canceled += instance.OnRight;
                    @Up.started += instance.OnUp;
                    @Up.performed += instance.OnUp;
                    @Up.canceled += instance.OnUp;
                    @Down.started += instance.OnDown;
                    @Down.performed += instance.OnDown;
                    @Down.canceled += instance.OnDown;
                    @Import.started += instance.OnImport;
                    @Import.performed += instance.OnImport;
                    @Import.canceled += instance.OnImport;
                    @Export.started += instance.OnExport;
                    @Export.performed += instance.OnExport;
                    @Export.canceled += instance.OnExport;
                }
            }
        }
        public PrototypingActions @Prototyping => new PrototypingActions(this);
        public interface IDefaultActions
        {
            void OnSideMovement(InputAction.CallbackContext context);
            void OnRotate(InputAction.CallbackContext context);
        }
        public interface IPrototypingActions
        {
            void OnLeft(InputAction.CallbackContext context);
            void OnRight(InputAction.CallbackContext context);
            void OnUp(InputAction.CallbackContext context);
            void OnDown(InputAction.CallbackContext context);
            void OnImport(InputAction.CallbackContext context);
            void OnExport(InputAction.CallbackContext context);
        }
    }
}
