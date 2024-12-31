//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.7.0
//     from Assets/Scripts/Controls.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class @Controls: IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @Controls()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""Controls"",
    ""maps"": [
        {
            ""name"": ""Vehicle"",
            ""id"": ""756719a6-9975-4a57-9d52-a7f193cf9b9f"",
            ""actions"": [
                {
                    ""name"": ""Accelerate"",
                    ""type"": ""Button"",
                    ""id"": ""3e7a6997-972f-429a-ab24-10e16e17e64c"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Brake"",
                    ""type"": ""Button"",
                    ""id"": ""572f2796-3ab8-4459-93e4-32d581e6f502"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Left"",
                    ""type"": ""Button"",
                    ""id"": ""8eb6a573-7538-4f3a-a47c-ac136aedfbfc"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Right"",
                    ""type"": ""Button"",
                    ""id"": ""5f1598df-d683-443a-8461-361250be67ba"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Pause"",
                    ""type"": ""Button"",
                    ""id"": ""8e7b27b3-a186-4cf9-a019-842e30f40fc9"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""35acee14-f987-480f-919b-e5b65194c193"",
                    ""path"": ""<XInputController>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Accelerate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""8478fdda-8e80-4722-bee7-60826c9c8267"",
                    ""path"": ""<XInputController>/buttonWest"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Xbox control scheme"",
                    ""action"": ""Brake"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""03761544-d909-4096-8ada-c311622e9624"",
                    ""path"": ""<XInputController>/dpad/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Xbox control scheme"",
                    ""action"": ""Left"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""904561a5-0fa3-4662-b9d9-7b5f2f53e970"",
                    ""path"": ""<XInputController>/dpad/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Xbox control scheme"",
                    ""action"": ""Right"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a63250ee-f1dc-4186-9a33-86d027c5c04f"",
                    ""path"": ""<XInputController>/start"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Xbox control scheme"",
                    ""action"": ""Pause"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""Xbox control scheme"",
            ""bindingGroup"": ""Xbox control scheme"",
            ""devices"": []
        }
    ]
}");
        // Vehicle
        m_Vehicle = asset.FindActionMap("Vehicle", throwIfNotFound: true);
        m_Vehicle_Accelerate = m_Vehicle.FindAction("Accelerate", throwIfNotFound: true);
        m_Vehicle_Brake = m_Vehicle.FindAction("Brake", throwIfNotFound: true);
        m_Vehicle_Left = m_Vehicle.FindAction("Left", throwIfNotFound: true);
        m_Vehicle_Right = m_Vehicle.FindAction("Right", throwIfNotFound: true);
        m_Vehicle_Pause = m_Vehicle.FindAction("Pause", throwIfNotFound: true);
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

    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }

    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // Vehicle
    private readonly InputActionMap m_Vehicle;
    private List<IVehicleActions> m_VehicleActionsCallbackInterfaces = new List<IVehicleActions>();
    private readonly InputAction m_Vehicle_Accelerate;
    private readonly InputAction m_Vehicle_Brake;
    private readonly InputAction m_Vehicle_Left;
    private readonly InputAction m_Vehicle_Right;
    private readonly InputAction m_Vehicle_Pause;
    public struct VehicleActions
    {
        private @Controls m_Wrapper;
        public VehicleActions(@Controls wrapper) { m_Wrapper = wrapper; }
        public InputAction @Accelerate => m_Wrapper.m_Vehicle_Accelerate;
        public InputAction @Brake => m_Wrapper.m_Vehicle_Brake;
        public InputAction @Left => m_Wrapper.m_Vehicle_Left;
        public InputAction @Right => m_Wrapper.m_Vehicle_Right;
        public InputAction @Pause => m_Wrapper.m_Vehicle_Pause;
        public InputActionMap Get() { return m_Wrapper.m_Vehicle; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(VehicleActions set) { return set.Get(); }
        public void AddCallbacks(IVehicleActions instance)
        {
            if (instance == null || m_Wrapper.m_VehicleActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_VehicleActionsCallbackInterfaces.Add(instance);
            @Accelerate.started += instance.OnAccelerate;
            @Accelerate.performed += instance.OnAccelerate;
            @Accelerate.canceled += instance.OnAccelerate;
            @Brake.started += instance.OnBrake;
            @Brake.performed += instance.OnBrake;
            @Brake.canceled += instance.OnBrake;
            @Left.started += instance.OnLeft;
            @Left.performed += instance.OnLeft;
            @Left.canceled += instance.OnLeft;
            @Right.started += instance.OnRight;
            @Right.performed += instance.OnRight;
            @Right.canceled += instance.OnRight;
            @Pause.started += instance.OnPause;
            @Pause.performed += instance.OnPause;
            @Pause.canceled += instance.OnPause;
        }

        private void UnregisterCallbacks(IVehicleActions instance)
        {
            @Accelerate.started -= instance.OnAccelerate;
            @Accelerate.performed -= instance.OnAccelerate;
            @Accelerate.canceled -= instance.OnAccelerate;
            @Brake.started -= instance.OnBrake;
            @Brake.performed -= instance.OnBrake;
            @Brake.canceled -= instance.OnBrake;
            @Left.started -= instance.OnLeft;
            @Left.performed -= instance.OnLeft;
            @Left.canceled -= instance.OnLeft;
            @Right.started -= instance.OnRight;
            @Right.performed -= instance.OnRight;
            @Right.canceled -= instance.OnRight;
            @Pause.started -= instance.OnPause;
            @Pause.performed -= instance.OnPause;
            @Pause.canceled -= instance.OnPause;
        }

        public void RemoveCallbacks(IVehicleActions instance)
        {
            if (m_Wrapper.m_VehicleActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(IVehicleActions instance)
        {
            foreach (var item in m_Wrapper.m_VehicleActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_VehicleActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }
    public VehicleActions @Vehicle => new VehicleActions(this);
    private int m_XboxcontrolschemeSchemeIndex = -1;
    public InputControlScheme XboxcontrolschemeScheme
    {
        get
        {
            if (m_XboxcontrolschemeSchemeIndex == -1) m_XboxcontrolschemeSchemeIndex = asset.FindControlSchemeIndex("Xbox control scheme");
            return asset.controlSchemes[m_XboxcontrolschemeSchemeIndex];
        }
    }
    public interface IVehicleActions
    {
        void OnAccelerate(InputAction.CallbackContext context);
        void OnBrake(InputAction.CallbackContext context);
        void OnLeft(InputAction.CallbackContext context);
        void OnRight(InputAction.CallbackContext context);
        void OnPause(InputAction.CallbackContext context);
    }
}