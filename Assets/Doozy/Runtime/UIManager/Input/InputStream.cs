// Copyright (c) 2015 - 2021 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using Doozy.Runtime.Signals;
using UnityEngine;
// ReSharper disable MemberCanBePrivate.Global

namespace Doozy.Runtime.UIManager.Input
{
    public static class InputStream
    {
        public const string k_StreamCategory = "Input";
        public const string k_StreamName = nameof(InputStream);
        
        private static SignalStream s_stream;
        public static SignalStream stream => s_stream ??= SignalsService.GetStream(k_StreamCategory, k_StreamName);

        public const string k_NavigateStreamCategory = "Navigate";
        public const string k_NavigateLeft = "Left";
        public const string k_NavigateRight = "Right";
        public const string k_NavigateUp = "Up";
        public const string k_NavigateDown = "Down";
        
        private static SignalStream s_navigateLeftStream;
        private static SignalStream s_navigateRightStream;
        private static SignalStream s_navigateUpStream;
        private static SignalStream s_navigateDownStream;
        public static SignalStream navigateLeftStream => s_navigateLeftStream ??= SignalsService.GetStream(k_NavigateStreamCategory, k_NavigateLeft);
        public static SignalStream navigateRightStream => s_navigateRightStream ??= SignalsService.GetStream(k_NavigateStreamCategory, k_NavigateRight);
        public static SignalStream navigateUpStream => s_navigateUpStream ??= SignalsService.GetStream(k_NavigateStreamCategory, k_NavigateUp);
        public static SignalStream navigateDownStream => s_navigateDownStream ??= SignalsService.GetStream(k_NavigateStreamCategory, k_NavigateDown);
        
        private static SignalReceiver inputStreamReceiver { get; set; }
        private static void ConnectToInputStream()
        {
            stream.ConnectReceiver(inputStreamReceiver);
            connected = true;
        }
        private static void DisconnectFromInputStream()
        {
            stream.DisconnectReceiver(inputStreamReceiver);
            connected = false;
        }

        private static bool connected { get; set; } = false;
        
        public static void Start()
        {
            if (connected)
                return;
            
            inputStreamReceiver = new SignalReceiver().SetOnSignalCallback(signal =>
            {
                if (!signal.hasValue) return;
                if (!(signal.valueAsObject is InputSignalData { inputActionName: UIInputActionName.Navigate } data)) return;
                Navigate(data);
            });
            
            ConnectToInputStream();
        }

        public static void Stop()
        {
            if (!connected)
                return;
            DisconnectFromInputStream();
            inputStreamReceiver = null;
        }

        private static void Navigate(InputSignalData data)
        {
            Vector2 direction = data.callbackContext.ReadValue<Vector2>();
            
            if (direction.x < 0)
            {
                navigateLeftStream.SendSignal(data);
                return;
            }
            
            if (direction.x > 0)
            {
                navigateRightStream.SendSignal(data);
                return;
            }
            
            if (direction.y < 0)
            {
                navigateDownStream.SendSignal(data);
                return;
            }
            
            if (direction.y > 0)
            {
                navigateUpStream.SendSignal(data);
                return;
            }
        }
        
        
    }
}
