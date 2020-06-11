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
        }
    ],
    ""controlSchemes"": []
}");
            // Default
            m_Default = asset.FindActionMap("Default", throwIfNotFound: true);
            m_Default_SideMovement = m_Default.FindAction("Side Movement", throwIfNotFound: true);
            m_Default_Rotate = m_Default.FindAction("Rotate", throwIfNotFound: true);
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
        public interface IDefaultActions
        {
            void OnSideMovement(InputAction.CallbackContext context);
            void OnRotate(InputAction.CallbackContext context);
        }
    }
}
