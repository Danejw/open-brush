// Copyright (c) 2015 - 2021 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

namespace Doozy.Runtime.Reactor
{
    /// <summary> Reset options for the value of a Progressor </summary>
    public enum ResetValue
    {
        /// <summary> Value does not reset </summary>
        Disabled,
        
        /// <summary> Value resets to the min value </summary>
        ToStartValue,
        
        /// <summary> Value resets to the max value </summary>
        ToEndValue,
        
        /// <summary> Value resets to a custom value </summary>
        ToCustomValue
    }
}
